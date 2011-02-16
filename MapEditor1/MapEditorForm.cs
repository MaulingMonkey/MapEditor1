using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using MapEditor1.Properties;

namespace MapEditor1 {
	[Serializable] class Layer {
		public bool   Visible;
		public string Name = "Unnamed";
	}

	[Serializable] class BitmapLayer : Layer {
		public Bitmap Canvas;
	}

	[Serializable] class BitLayer : Layer {
		public bool[,] Data;
	}

	[Serializable]
	class Map {
		public int SnapX, SnapY;
		public int Width, Height;

		public readonly List<Layer> Layers = new List<Layer>();
		public readonly List<Bitmap> Assets = new List<Bitmap>();
	}

	[System.ComponentModel.DesignerCategory("")]
	class MapEditorForm : Form {
		Map   Map;
		int   MapZoom = 2;
		Point MapOffset = new Point(0,0);

		Point MapOffsetTL { get {
			var o = MapOffset;
			o.Offset( MapArea.Width/2, MapArea.Height/2 );
			return o;
		}}

		UI.LayersSidebar LayersSidebar = new UI.LayersSidebar();
		UI.AssetsSidebar AssetsSidebar = new UI.AssetsSidebar();
		public Layer  SelectedLayer { get { return LayersSidebar.SelectedLayer; }}
		public Bitmap SelectedAsset { get { return AssetsSidebar.SelectedAsset; }}

		public MapEditorForm() {
			BackColor      = Color.Black;
			ClientSize     = new Size(800,600);
			DoubleBuffered = true;
			Text           = "Map Editor 1";

			Application.Idle += delegate { Invalidate(); };
			LayersSidebar.GetLayers = () => Map == null ? null : Map.Layers;
			AssetsSidebar.GetAssets = () => Map == null ? null : Map.Assets;
			LayersSidebar.Form = this;
			LayersSidebar.Font = Font;
		}

		void Refresh_Tick( object sender, EventArgs e ) {
			Invalidate();
		}

		public void AttemptAddAsset( Bitmap image ) {
			if ( image == null || Map==null ) {
				SystemSounds.Beep.Play();
				return;
			}

			if ( Map!=null && (image.Width/Map.SnapX*Map.SnapX != image.Width || image.Height/Map.SnapY*Map.SnapY != image.Height) ) {
				var doublecheck = MessageBox.Show
					( this
					, "Image isn't a multiple of snap size\n"
					+ "Are you sure you want to add it?"

					, "Image isn't a multiple of snap size"
					, MessageBoxButtons.YesNo
					, MessageBoxIcon.Question
					, MessageBoxDefaultButton.Button1
					);
				if ( doublecheck != DialogResult.Yes ) return;
			}
			Map.Assets.Add( image );
		}

		string _dfn;
		string DocumentFileName { get {
			return _dfn;
		} set {
			_dfn = value;
			Text = "Map Editor 1 -- "+_dfn;
		}}

		void DoSave( string filename ) {
			if ( filename == null ) {
				var savedialog = new SaveFileDialog()
					{ AddExtension = true
					, DefaultExt = "mp1"
					, Filter = "Map Files|*.mp1"
					, InitialDirectory = Program.State.LastUsedDirectory ?? @"I:\home\projects\"
					, Title = "Save map file..."
					};
				var saveresult = savedialog.ShowDialog(this);
				if ( saveresult != DialogResult.OK ) return;
				filename = savedialog.FileName;
			}

			try {
				var ms = new MemoryStream();
				var bf = new BinaryFormatter();
				bf.Serialize( ms, Map );
				ms.Position = 0;
				var testdeserialize = (Map)bf.Deserialize(ms);
				File.WriteAllBytes( filename, ms.ToArray() );
				DocumentFileName = filename;
				Program.State.LastUsedDirectory = Path.GetDirectoryName(filename);
			} catch ( Exception e ) {
				MessageBox.Show
					( this
					, e.Message
					, "Error serializing map"
					, MessageBoxButtons.OK
					);
			}
		}

		protected override void OnKeyDown( KeyEventArgs args ) {
			args.SuppressKeyPress = args.Handled = true;
			base.OnKeyDown(args);

			switch ( args.KeyData ) {
			case Keys.Control | Keys.N:
				var dialog = new CreateMapDialog();
				var newmap = dialog.Show(this);
				if ( newmap!=null ) {
					Map = newmap;
					LayersSidebar.SelectedLayer = Map.Layers.FirstOrDefault();
				}
				break;
			case Keys.Control | Keys.Shift | Keys.S:
				DoSave(null);
				break;
			case Keys.Control | Keys.S:
				DoSave(DocumentFileName);
				break;
			case Keys.Control | Keys.O:
				var opendialog = new OpenFileDialog()
					{ DefaultExt="mp1"
					, Filter = "Map Files|*.mp1"
					, InitialDirectory = Program.State.LastUsedDirectory ?? @"I:\home\projects\"
					, Title = "Open map file..."
					};
				if ( opendialog.ShowDialog(this) == DialogResult.OK ) try {
					var bf = new BinaryFormatter();
					var ms = new MemoryStream( File.ReadAllBytes(opendialog.FileName) );
					Map = (Map)bf.Deserialize(ms);
					DocumentFileName = opendialog.FileName;
					LayersSidebar.SelectedLayer = Map.Layers.FirstOrDefault();
					MapOffset = new Point
						( -(Map.Width *Map.SnapX*MapZoom)/2
						, -(Map.Height*Map.SnapY*MapZoom)/2
						);
				} catch ( Exception e ) {
					MessageBox.Show
						( this
						, e.Message
						, "Error deserializing map"
						, MessageBoxButtons.OK
						);
				}
				break;
			case Keys.Control | Keys.A:
				var fd = new OpenFileDialog()
					{ InitialDirectory = @"I:\home\art"
					};

				if ( fd.ShowDialog(this) == DialogResult.OK ) try {
					AttemptAddAsset( (Bitmap)Bitmap.FromFile(fd.FileName) );
				} catch ( Exception ) {
					SystemSounds.Beep.Play();
				}

				break;
			case Keys.Control | Keys.V:
				try {
					var image = (Bitmap)Clipboard.GetImage();
					AttemptAddAsset(image);
				} catch ( Exception ) { SystemSounds.Beep.Play(); }
				break;
			case Keys.Delete:
				if ( Map==null ) break;
				if ( AssetsSidebar.SelectedAsset == null ) break;

				var result = MessageBox.Show
					( this
					, "Are you sure you want to remove this asset from the list?"
					, "Confirmation"
					, MessageBoxButtons.YesNo
					, MessageBoxIcon.Question
					, MessageBoxDefaultButton.Button1
					);
				if ( result == DialogResult.Yes ) {
					Map.Assets.Remove( SelectedAsset );
				}
				break;
			default:
				args.SuppressKeyPress = args.Handled = false;
				break;
			}
		}

		static readonly Brush BitBrush = new SolidBrush(Color.FromArgb(128,Color.Red));

		Point? MMB_Scroll;

		protected override void  OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);

			if ( AssetsSidebar.OnMouseDown(e) ) return;
			if ( LayersSidebar.OnMouseDown(e) ) return;

			if ( e.Button == MouseButtons.Middle ) MMB_Scroll = e.Location;

			SelectedXY = null;
			if ( Map==null ) return;

			var x = (e.Location.X-MapOffsetTL.X)/Map.SnapX/MapZoom;
			var y = (e.Location.Y-MapOffsetTL.Y)/Map.SnapY/MapZoom;

			if ( 0<=x && x<Map.Width )
			if ( 0<=y && y<Map.Height )
			{
				SelectedXY = new Point(x,y);
			}
			if ( SelectedXY==null ) return;

			var xy = SelectedXY.Value;

			if ( SelectedLayer == null ) return;

			if ( SelectedLayer is BitLayer ) {
				var layer = (BitLayer)SelectedLayer;
				switch ( e.Button ) {
				case MouseButtons.Left:  layer.Data[xy.X,xy.Y] = true ; break;
				case MouseButtons.Right: layer.Data[xy.X,xy.Y] = false; break;
				default: break;
				}
			} else if (SelectedLayer is BitmapLayer ) {
				var layer = (BitmapLayer)SelectedLayer;
				switch ( e.Button ) {
				case MouseButtons.Left:
					if ( SelectedAsset!=null )
					using ( var fx = Graphics.FromImage(layer.Canvas) )
					{
						var asset = SelectedAsset;
						fx.DrawImage( asset, x*Map.SnapX, y*Map.SnapY, asset.Width, asset.Height );
					}
					break;
				default: break;
				}
			} else {
				Debug.Fail("!!@!#!@#!@$");
			}
		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove(e);

			if ( (e.Button & MouseButtons.Middle) != MouseButtons.None ) {
				var mmb = MMB_Scroll ?? e.Location;
				var dx = e.Location.X - mmb.X;
				var dy = e.Location.Y - mmb.Y;
				MMB_Scroll = e.Location;

				MapOffset.X += dx;
				MapOffset.Y += dy;
			}

			SelectedXY = null;
			if ( Map==null ) return;

			var x = (e.Location.X-MapOffsetTL.X)/Map.SnapX/MapZoom;
			var y = (e.Location.Y-MapOffsetTL.Y)/Map.SnapY/MapZoom;

			if ( 0<=x && x<Map.Width )
			if ( 0<=y && y<Map.Height )
			{
				SelectedXY = new Point(x,y);
			}
			if ( SelectedXY==null ) return;

			var xy = SelectedXY.Value;

			if ( SelectedLayer == null ) return;

			if ( SelectedLayer is BitLayer ) {
				var layer = (BitLayer)SelectedLayer;
				switch ( e.Button ) {
				case MouseButtons.Left:  layer.Data[xy.X,xy.Y] = true ; break;
				case MouseButtons.Right: layer.Data[xy.X,xy.Y] = false; break;
				default: break;
				}
			} else if (SelectedLayer is BitmapLayer ) {
				var layer = (BitmapLayer)SelectedLayer;
				switch ( e.Button ) {
				case MouseButtons.Left:
					if ( SelectedAsset!=null )
					using ( var fx = Graphics.FromImage(layer.Canvas) )
					{
						var asset = SelectedAsset;
						fx.DrawImage( asset, x*Map.SnapX, y*Map.SnapY, asset.Width, asset.Height );
					}
					break;
				default: break;
				}
			} else {
				Debug.Fail("!!@!#!@#!@$");
			}
		}

		protected override void OnMouseUp( MouseEventArgs e ) {
			if ( e.Button == MouseButtons.Middle ) MMB_Scroll = null;

			base.OnMouseUp(e);
		}

		protected override void OnMouseWheel( MouseEventArgs e ) {
			if ( SelectedAsset == null ) {
				if ( Map != null ) AssetsSidebar.SelectedAsset = Map.Assets.First();
			} else {
				var index = Map.Assets.IndexOf(SelectedAsset);
				if ( index==-1 ) {
					AssetsSidebar.SelectedAsset = Map.Assets.First();
				} else {
					index -= e.Delta/100;
					if ( index<0 ) index=0;
					if ( index>=Map.Assets.Count ) index=Map.Assets.Count-1;

					AssetsSidebar.SelectedAsset = Map.Assets[index];
				}
			}

			base.OnMouseWheel(e);
		}

		Point? SelectedXY = null;

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint(e);

			var fx = e.Graphics;

			fx.Clear( BackColor );
			fx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			fx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

			if ( Map!=null ) {
				foreach ( var layer in Map.Layers )
				if ( layer.Visible )
				{
					var bitmaplayer = layer as BitmapLayer;
					var bitlayer = layer as BitLayer;

					if ( bitmaplayer != null ) {
						fx.DrawImage( bitmaplayer.Canvas, MapOffsetTL.X, MapOffsetTL.Y, bitmaplayer.Canvas.Width*MapZoom, bitmaplayer.Canvas.Height*MapZoom );
					} else if ( bitlayer != null ) {
						var off = MapOffsetTL;

						for ( int y=0 ; y<Map.Height ; ++y )
						for ( int x=0 ; x<Map.Width  ; ++x )
						{
							if ( bitlayer.Data[x,y] ) fx.FillRectangle( BitBrush, x*Map.SnapX*MapZoom+off.X, y*Map.SnapY*MapZoom+off.Y, Map.SnapX*MapZoom, Map.SnapY*MapZoom );
						}
					} else {
						throw new InvalidOperationException( "Invalid layer type: "+layer.GetType() );
					}
				}
			}

			var side = ClientSize.Width;

			var target = ClientRectangle;
			target = AssetsSidebar.RefreshLayout( target );
			target = LayersSidebar.RefreshLayout( target );
			MapArea = target; // everything left over
			fx.FillRectangle( Brushes.Black, AssetsSidebar.BackgroundArea );
			fx.FillRectangle( Brushes.Black, LayersSidebar.BackgroundArea );
			AssetsSidebar.RenderTo(fx);
			LayersSidebar.RenderTo(fx);

			if ( DateTime.Now.Millisecond<500 && SelectedXY!=null ) {
				var xy = SelectedXY.Value;
				var asset = SelectedAsset;

				if ( asset != null && SelectedLayer is BitmapLayer ) {
					fx.DrawImage( asset, xy.X*Map.SnapX*MapZoom + MapOffsetTL.X, xy.Y*Map.SnapY*MapZoom + MapOffsetTL.Y, asset.Width*MapZoom, asset.Height*MapZoom );
				} else {
					fx.FillRectangle( Brushes.Red, xy.X*Map.SnapX*MapZoom + MapOffsetTL.X, xy.Y*Map.SnapY*MapZoom + MapOffsetTL.Y, Map.SnapX*MapZoom, Map.SnapY*MapZoom );
				}
			}
		}

		Rectangle MapArea;

		static readonly Bitmap
			BitmapLayerIcon  = Resources.ImageLayerIcon,
			BitLayerIcon     = Resources.BitLayerIcon,
			VisibleLayerIcon = Resources.LayerVisibility,
			TextLayerIcon    = Resources.LayerRename;
	}
}
