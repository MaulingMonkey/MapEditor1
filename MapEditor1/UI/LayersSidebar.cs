using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapEditor1.Properties;

namespace MapEditor1.UI {
	class LayersSidebar {
		public Func<IList<Layer>> GetLayers;
		public Layer              SelectedLayer;

		public Form Form;
		public Font Font;
		public int  Margin = 5;
		// No Zoom

		struct LayoutEntry {
			public Layer     Layer;

			public Rectangle LinePosition;

			public Rectangle TypeIconPosition;
			public Rectangle TextPosition;
			public Rectangle VisibilityIconPosition;
			public Rectangle RenameIconPosition;
			public Rectangle MoveLayerUpPosition;
			public Rectangle MoveLayerDownPosition;
		}
		public Rectangle BackgroundArea;
		readonly List<LayoutEntry> Layout = new List<LayoutEntry>();

		public Rectangle RefreshLayout( Rectangle target ) {
			BackgroundArea = Rectangle.Empty;
			Layout.Clear();

			if ( GetLayers == null ) return target;
			var layers_ = GetLayers();
			if ( layers_ == null ) return target;
			var layers = layers_.ToArray();

			var textw = layers.Max( layer => TextRenderer.MeasureText( layer.Name, Font ).Width );
			var right = target.Right-Margin;
			var left  = right-16-2-textw-2-16-2-16-2-16-2-16;

			BackgroundArea = new Rectangle
				( left-Margin
				, target.Top
				, (right+Margin) - (left-Margin)
				, target.Height
				);

			int y = target.Top+Margin;
			foreach ( var layer in layers ) {
				var x = left;

				var entry = new LayoutEntry();

				entry.Layer = layer;
				entry.TypeIconPosition       = new Rectangle(x,y,   16,16); x += 16+2;
				entry.TextPosition           = new Rectangle(x,y,textw,16); x += textw+2;
				entry.VisibilityIconPosition = new Rectangle(x,y,   16,16); x += 16+2;
				entry.RenameIconPosition     = new Rectangle(x,y,   16,16); x += 16+2;
				entry.MoveLayerUpPosition    = new Rectangle(x,y,   16,16); x += 16+2;
				entry.MoveLayerDownPosition  = new Rectangle(x,y,   16,16); x += 16+2;
				x-=2;
				Debug.Assert( x==right );

				entry.LinePosition = new Rectangle(left-2,y-1,right-left+4,18);

				Layout.Add(entry);
				y += 16+2;
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
				var layer = entry.Layer;
				var pos   = entry;

				var fg = (layer == SelectedLayer) ? Color.Black  : Color.White;
				var bg = (layer == SelectedLayer) ? Color.Orange : Color.Black;

				var typeicon = (layer is BitmapLayer ? BitmapLayerIcon : layer is BitLayer ? BitLayerIcon : null);
				if ( layer==SelectedLayer ) using ( var brush = new SolidBrush(bg) ) fx.FillRectangle( brush, pos.LinePosition );
				fx.DrawImage( typeicon, pos.TypeIconPosition );
				TextRenderer.DrawText( fx, layer.Name, Font, pos.TextPosition, fg, bg );

				fx.DrawImage( layer.Visible ? VisibleLayerIcon : InvisibleLayerIcon , pos.VisibilityIconPosition );
				fx.DrawImage( TextLayerIcon                                         , pos.RenameIconPosition     );
				fx.DrawImage( MoveLayerUpIcon                                       , pos.MoveLayerUpPosition    );
				fx.DrawImage( MoveLayerDownIcon                                     , pos.MoveLayerDownPosition  );
			}
		}

		static readonly Bitmap
			BitmapLayerIcon    = Resources.ImageLayerIcon,
			BitLayerIcon       = Resources.BitLayerIcon,
			VisibleLayerIcon   = Resources.LayerVisibility,
			InvisibleLayerIcon = Resources.LayerVisibilityOff,
			TextLayerIcon      = Resources.LayerRename,
			MoveLayerUpIcon    = Resources.MoveLayerUp,
			MoveLayerDownIcon  = Resources.MoveLayerDown;

		public bool OnMouseDown( MouseEventArgs args ) {
			if (!BackgroundArea.Contains(args.Location)) return false;

			foreach ( var entry in Layout ) if ( entry.LinePosition.Contains(args.Location) ) {

				if ( entry.VisibilityIconPosition.Contains(args.Location) ) {
					entry.Layer.Visible ^= true;
				} else if ( entry.RenameIconPosition.Contains(args.Location) ) {
					var rld = new RenameLayerDialog(entry.Layer);
					rld.ShowDialog(Form);
				} else if ( entry.MoveLayerUpPosition.Contains(args.Location) ) {
					var layers = GetLayers();
					var index = layers.IndexOf(entry.Layer);
					if ( index==-1 || index<=0 ) break;
					var a = layers[index-1];
					var b = layers[index-0];
					layers[index-1] = b;
					layers[index-0] = a;
				} else if ( entry.MoveLayerDownPosition.Contains(args.Location) ) {
					var layers = GetLayers();
					var index = layers.IndexOf(entry.Layer);
					if ( index==-1 || index>=layers.Count-1 ) break;
					var a = layers[index+1];
					var b = layers[index+0];
					layers[index+1] = b;
					layers[index+0] = a;
				} else {
					SelectedLayer = entry.Layer;
				}

				return true;
			}

			return true;
		}
	}
}
