using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MapEditor1.UI {
	class AssetsSidebar {
		public Func<IEnumerable<Bitmap>> GetAssets;
		private Bitmap _SelectedAsset;
		public Bitmap SelectedAsset { get {
			return _SelectedAsset;
		} set {
			_SelectedAsset = value;
			if ( value==null ) return;

			var entry = Layout.First(e=>e.Asset==value);

			// Recalculate autoscroll:
			Scroll = (entry.UnscrolledPosition.Top+entry.UnscrolledPosition.Bottom)/2-BackgroundArea.Height/2;
			if ( Scroll < 0         ) Scroll=0;
			if ( Scroll > MaxScroll ) Scroll = MaxScroll;
		}}

		public int Margin = 5;
		public int ScrollbarWidth = 5;
		public int Zoom   = 2;

		int Scroll    = 0;
		int MaxScroll = 0;

		struct LayoutEntry {
			public Rectangle UnscrolledPosition;
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
			var left  = right-assets_w-ScrollbarWidth-Margin;

			BackgroundArea = new Rectangle
				( left-Margin
				, target.Top
				, (right+Margin) - (left-Margin)
				, target.Height
				);
			
			int y,i;
			for ( y=target.Top+2, i=0 ; /*y<target.Bottom &&*/ i<assets.Length ; y+=assets[i].Height*Zoom+2, ++i ) {
				// Don't clip: We now offset Layout by Position and need all entries generated

				Layout.Add( new LayoutEntry()
					{ UnscrolledPosition = new Rectangle
						( left+(assets_w-assets[i].Width*Zoom)/2
						, y
						, assets[i].Width *Zoom
						, assets[i].Height*Zoom
						)
					, Asset = assets[i]
					});
			}

			MaxScroll = Math.Max(0,y-target.Height);

			return new Rectangle
				( target.Left
				, target.Top
				, left-target.Left
				, target.Height
				);
		}

		Rectangle GetScrolledPosition( LayoutEntry le ) {
			var p = le.UnscrolledPosition;
			p.Y -= Scroll;
			return p;
		}

		public void RenderTo( Graphics fx ) {
			if ( Layout.Count==0 ) return;

			foreach ( var entry in Layout ) {
				var pos = GetScrolledPosition(entry);
				fx.DrawImage( entry.Asset, pos );
				if ( entry.Asset == SelectedAsset ) {
					fx.PixelOffsetMode = PixelOffsetMode.Default;
					fx.DrawRectangle( Pens.White, pos );
					fx.PixelOffsetMode = PixelOffsetMode.Half;
				}
			}

			fx.FillRectangle( Brushes.DarkRed, BackgroundArea.Right-Margin-ScrollbarWidth, BackgroundArea.Top, ScrollbarWidth, BackgroundArea.Height );
			int t = BackgroundArea.Top + Scroll*BackgroundArea.Height/(BackgroundArea.Height+MaxScroll);
			int h = BackgroundArea.Height*BackgroundArea.Height/(BackgroundArea.Height+MaxScroll);
			fx.FillRectangle( Brushes.Red, BackgroundArea.Right-Margin-ScrollbarWidth, t, ScrollbarWidth, h );
		}

		public bool OnMouseDown( MouseEventArgs args ) {
			if (!BackgroundArea.Contains(args.Location) ) return false;

			SelectedAsset = null;
			foreach ( var entry in Layout ) if ( GetScrolledPosition(entry).Contains(args.Location) ) {
				SelectedAsset = entry.Asset;
				return true;
			}

			return true;
		}
	}
}
