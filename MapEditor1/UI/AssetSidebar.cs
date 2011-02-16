using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MapEditor1.UI {
	class AssetsSidebar {
		public Func<IEnumerable<Bitmap>> GetAssets;
		public Bitmap                    SelectedAsset;

		public int Margin = 5;
		public int Zoom   = 2;

		struct LayoutEntry {
			public Rectangle Position;
			public Bitmap    Asset;
		}

		public Rectangle BackgroundArea;
		readonly List<LayoutEntry> Layout = new List<LayoutEntry>();
		public Rectangle RefreshLayout( Rectangle target ) {
			BackgroundArea = Rectangle.Empty;
			Layout.Clear();

			if ( GetAssets == null ) return target;
			var assets_ = GetAssets();
			if ( assets_ == null ) return target;
			var assets = assets_.ToArray();
			if ( assets.Length == 0 ) return target;

			var assets_w = assets.Max( a => a.Width*Zoom );
			// Content metrics:
			var right = target.Right-Margin;
			var left  = right-assets_w;

			BackgroundArea = new Rectangle
				( left-Margin
				, target.Top
				, (right+Margin) - (left-Margin)
				, target.Height
				);
			
			for ( int y=target.Top+2, i=0 ; y<target.Bottom && i<assets.Length ; y+=assets[i].Height*Zoom+2, ++i ) {
				Layout.Add( new LayoutEntry()
					{ Position = new Rectangle
						( left+(assets_w-assets[i].Width*Zoom)/2
						, y
						, assets[i].Width *Zoom
						, assets[i].Height*Zoom
						)
					, Asset = assets[i]
					});
			}

			return new Rectangle
				( target.Left
				, target.Top
				, left-target.Left
				, target.Height
				);
		}

		public void RenderTo( Graphics fx ) {
			foreach ( var entry in Layout ) {
				fx.DrawImage( entry.Asset, entry.Position );
				if ( entry.Asset == SelectedAsset ) {
					fx.PixelOffsetMode = PixelOffsetMode.Default;
					fx.DrawRectangle( Pens.White, entry.Position );
					fx.PixelOffsetMode = PixelOffsetMode.Half;
				}
			}
		}

		public bool OnMouseDown( MouseEventArgs args ) {
			if (!BackgroundArea.Contains(args.Location) ) return false;

			SelectedAsset = null;
			foreach ( var entry in Layout ) if ( entry.Position.Contains(args.Location) ) {
				SelectedAsset = entry.Asset;
				return true;
			}

			return true;
		}
	}
}
