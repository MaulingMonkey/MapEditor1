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
		Layer SelectedLayer;

		int MapZoom = 2;
		int AssetsZoom = 2;

		public MapEditorForm() {
			ClientSize = new Size(800,600);
			DoubleBuffered = true;
			Text = "Map Editor 1";

			Application.Idle += delegate { Invalidate(); };
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

		void DoSave() {
			var savedialog = new SaveFileDialog()
				{ AddExtension = true
				, DefaultExt = "mp1"
				, Filter = "Map Files|*.mp1"
				, InitialDirectory = @"I:\home\projects\"
				, Title = "Save map file..."
				};
			var saveresult = savedialog.ShowDialog(this);
			if ( saveresult == DialogResult.OK ) try {
				var ms = new MemoryStream();
				var bf = new BinaryFormatter();
				bf.Serialize( ms, Map );
				ms.Position = 0;
				var testdeserialize = (Map)bf.Deserialize(ms);
				File.WriteAllBytes( savedialog.FileName, ms.ToArray() );
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
					SelectedLayer = Map.Layers.FirstOrDefault();
				}
				break;
			case Keys.Control | Keys.S:
				args.SuppressKeyPress = args.Handled = false;
				DoSave();
				break;
			case Keys.Control | Keys.O:
				var opendialog = new OpenFileDialog()
					{ DefaultExt="mp1"
					, Filter = "Map Files|*.mp1"
					, InitialDirectory = @"I:\home\projects\"
					, Title = "Open map file..."
					};
				if ( opendialog.ShowDialog(this) == DialogResult.OK ) try {
					var bf = new BinaryFormatter();
					var ms = new MemoryStream( File.ReadAllBytes(opendialog.FileName) );
					Map = (Map)bf.Deserialize(ms);
					SelectedLayer = Map.Layers.FirstOrDefault();
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
				if ( Map==null || SelectedAssetIndex<0 || SelectedAssetIndex>=Map.Assets.Count ) break;

				var result = MessageBox.Show
					( this
					, "Are you sure you want to remove this asset from the list?"
					, "Confirmation"
					, MessageBoxButtons.YesNo
					, MessageBoxIcon.Question
					, MessageBoxDefaultButton.Button1
					);
				if ( result == DialogResult.Yes ) {
					Map.Assets.RemoveAt( SelectedAssetIndex );
				}
				break;
			default:
				args.SuppressKeyPress = args.Handled = false;
				break;
			}
		}

		static readonly Brush BitBrush = new SolidBrush(Color.FromArgb(128,Color.Red));

		int SelectedAssetIndex = -1;

		IEnumerable<Rectangle> GenerateAssetPositions() {
			if ( Map==null || Map.Assets.Count==0 ) yield break;

			var assets_w = Map.Assets.Max( a => a.Width*AssetsZoom );

			for ( int y=2, i=0 ; y<ClientSize.Height && i<Map.Assets.Count ; y+=Map.Assets[i].Height*AssetsZoom+2, ++i ) {
				var target = new Rectangle(ClientSize.Width-assets_w+(assets_w-Map.Assets[i].Width*AssetsZoom)/2-2, y, Map.Assets[i].Width*AssetsZoom, Map.Assets[i].Height*AssetsZoom);
				yield return target;
			}
		}

		struct LayerLayoutEntry {
			public Rectangle LinePosition;

			public Rectangle TypeIconPosition;
			public Rectangle TextPosition;
			public Rectangle VisibilityIconPosition;
			public Rectangle RenameIconPosition;
		}

		IEnumerable<LayerLayoutEntry> GenerateLayerPositions( int right ) {
			if ( Map==null ) yield break;

			var textw = Map.Layers.Max( l => TextRenderer.MeasureText( l.Name, Font ).Width );

			int left = right-2-16-2-textw-2-16-2-16-2;
			int y = 2;
			foreach ( var layer in Map.Layers ) {
				var x = left;

				var entry = new LayerLayoutEntry();

				x += 2;
				entry.TypeIconPosition       = new Rectangle(x,y,   16,16); x += 16+2;
				entry.TextPosition           = new Rectangle(x,y,textw,16); x += textw+2;
				entry.VisibilityIconPosition = new Rectangle(x,y,   16,16); x += 16+2;
				entry.RenameIconPosition     = new Rectangle(x,y,   16,16); x += 16+2;
				Debug.Assert( x==right );

				entry.LinePosition = new Rectangle(left+1,y-1,right-left-2,18);

				yield return entry;
				y += 16+2;
			}
		}

		protected override void  OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);

			var asset_positions = GenerateAssetPositions().ToArray();

			int posx = ClientSize.Width;

			for ( int i=0 ; i<asset_positions.Length ; ++i ) {
				if ( asset_positions[i].Contains(e.Location) ) {
					SelectedAssetIndex = i;
					return;
				}
				posx = Math.Min(posx,asset_positions[i].Left);
			}

			var layer_positions = GenerateLayerPositions(posx-2).ToArray();

			for ( int i=0 ; i<layer_positions.Length ; ++i ) {
				if ( layer_positions[i].LinePosition.Contains(e.Location) ) {
					SelectedLayer = Map.Layers[i];
					return;
				}
			}

			SelectedXY = null;
			if ( Map==null ) return;

			var x = e.Location.X/Map.SnapX/MapZoom;
			var y = e.Location.Y/Map.SnapY/MapZoom;

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
					if ( 0<=SelectedAssetIndex && SelectedAssetIndex<Map.Assets.Count )
					using ( var fx = Graphics.FromImage(layer.Canvas) )
					{
						var asset = Map.Assets[SelectedAssetIndex];
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

			SelectedXY = null;
			if ( Map==null ) return;

			var x = e.Location.X/Map.SnapX/MapZoom;
			var y = e.Location.Y/Map.SnapY/MapZoom;

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
					if ( 0<=SelectedAssetIndex && SelectedAssetIndex<Map.Assets.Count )
					using ( var fx = Graphics.FromImage(layer.Canvas) )
					{
						var asset = Map.Assets[SelectedAssetIndex];
						fx.DrawImage( asset, x*Map.SnapX, y*Map.SnapY, asset.Width, asset.Height );
					}
					break;
				default: break;
				}
			} else {
				Debug.Fail("!!@!#!@#!@$");
			}
		}

		Point? SelectedXY = null;

		Point MapFocus = new Point(0,0);

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
						fx.DrawImage( bitmaplayer.Canvas, 0, 0, bitmaplayer.Canvas.Width*MapZoom, bitmaplayer.Canvas.Height*MapZoom );
					} else if ( bitlayer != null ) {
						for ( int y=0 ; y<Map.Height ; ++y )
						for ( int x=0 ; x<Map.Width  ; ++x )
						{
							if ( bitlayer.Data[x,y] ) fx.FillRectangle( BitBrush, x*Map.SnapX*MapZoom, y*Map.SnapY*MapZoom, Map.SnapX*MapZoom, Map.SnapY*MapZoom );
						}
					} else {
						throw new InvalidOperationException( "Invalid layer type: "+layer.GetType() );
					}
				}
			}

			var side = ClientSize.Width;

			if ( Map!=null && Map.Assets.Count>0 ) {
				var assets_w = Map.Assets.Max( a => a.Width*AssetsZoom );
				side -= assets_w+2;
				var positions = GenerateAssetPositions().ToArray();

				fx.FillRectangle( Brushes.Black, ClientSize.Width-assets_w-4, 0, assets_w+4, ClientSize.Height );
				for ( int y=2, i=0 ; y<ClientSize.Height && i<Map.Assets.Count ; y+=Map.Assets[i].Height*AssetsZoom+2, ++i ) {
					var target = positions[i];
					fx.DrawImage( Map.Assets[i], target );
					if ( i==SelectedAssetIndex ) {
						fx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
						fx.DrawRectangle( Pens.White, target );
						fx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
					}
				}
			}

			var layerpos = GenerateLayerPositions(side).ToArray();
			if ( layerpos.Length>0 ) {
				var left  = layerpos[0].TypeIconPosition  .Left -2;
				var right = layerpos[0].RenameIconPosition.Right+2;
				fx.FillRectangle( Brushes.Black, left, 0, right-left, ClientSize.Height );
			}
			for ( int i=0 ; i<layerpos.Length ; ++i ) {
				var layer = Map.Layers[i];
				var pos   = layerpos[i];

				var fg = (layer == SelectedLayer) ? Color.Black  : Color.White;
				var bg = (layer == SelectedLayer) ? Color.Orange : Color.Black;

				var typeicon = (layer is BitmapLayer ? BitmapLayerIcon : layer is BitLayer ? BitLayerIcon : null);
				if ( layer==SelectedLayer ) using ( var brush = new SolidBrush(bg) ) fx.FillRectangle( brush, pos.LinePosition );
				fx.DrawImage( typeicon, pos.TypeIconPosition );
				TextRenderer.DrawText( fx, layer.Name, Font, pos.TextPosition, fg, bg );
				fx.DrawImage( VisibleLayerIcon, pos.VisibilityIconPosition );
				fx.DrawImage( TextLayerIcon   , pos.RenameIconPosition     );
			}

			if ( DateTime.Now.Millisecond<500 && SelectedXY!=null ) {
				var xy = SelectedXY.Value;
				fx.FillRectangle( Brushes.Red, xy.X*Map.SnapX*MapZoom, xy.Y*Map.SnapY*MapZoom, Map.SnapX*MapZoom, Map.SnapY*MapZoom );
			}
		}

		static readonly Bitmap
			BitmapLayerIcon  = Resources.ImageLayerIcon,
			BitLayerIcon     = Resources.BitLayerIcon,
			VisibleLayerIcon = Resources.LayerVisibility,
			TextLayerIcon    = Resources.LayerRename;
	}
}
