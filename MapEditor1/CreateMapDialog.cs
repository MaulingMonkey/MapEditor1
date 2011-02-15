using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace MapEditor1 {
	partial class CreateMapDialog : Form {
		int  GridSnapSize = 8;
		Size MapSize = new Size(42,42);
		bool Success = false;

		public CreateMapDialog() {
			InitializeComponent();

			tbGridSnapSize.Text = "8";
			tbMapSize.Text = "42,42";
		}

		new public Map Show( IWin32Window window ) {
			base.ShowDialog(window);

			if (!Success) return null;

			var bitmaplayer = new BitmapLayer() { Canvas = new Bitmap(GridSnapSize*MapSize.Width,GridSnapSize*MapSize.Height,PixelFormat.Format32bppArgb), Visible=true };
			using ( var fx = Graphics.FromImage(bitmaplayer.Canvas) ) {
				fx.Clear(Color.Black);

				for ( int y=0 ; y<MapSize.Height ; ++y )
				for ( int x=0 ; x<MapSize.Width  ; ++x )
				{
					if ( (x+y)%2==0 ) fx.FillRectangle( Brushes.DarkGray, x*GridSnapSize, y*GridSnapSize, GridSnapSize, GridSnapSize );
				}
			}

			var bitlayer = new BitLayer() { Data = new bool[MapSize.Width,MapSize.Height], Visible=true };
			for ( int y=0 ; y<MapSize.Height ; ++y )
			for ( int x=0 ; x<MapSize.Width  ; ++x )
			{
				bitlayer.Data[x,y] = ((x<10)&&(y<10));
			}

			var map = new Map()
				{ Layers = { bitmaplayer, bitlayer }
				, SnapX  = GridSnapSize
				, SnapY  = GridSnapSize
				, Width  = MapSize.Width
				, Height = MapSize.Height
				};
			return map;
		}

		private void bCreate_Click( object sender, EventArgs e ) {
			Success = true;
			Close();
		}

		private void bCancel_Click( object sender, EventArgs e ) {
			Success = false;
			Close();
		}

		private void tbGridSnapSize_Leave( object sender, EventArgs e ) {
			int value;

			if ( !int.TryParse(tbGridSnapSize.Text,out value) ) {
				tbGridSnapSize.Text = GridSnapSize.ToString();
				SystemSounds.Beep.Play();
			} else {
				GridSnapSize = value;
			}

		}

		private void tbMapSize_Leave( object sender, EventArgs e ) {
			int x,y;

			var split = tbMapSize.Text.Split(',').Select(t=>t.Trim()).ToArray();
			if ( split.Length!=2 || !int.TryParse(split[0],out x) || !int.TryParse(split[1],out y) ) {
				tbMapSize.Text = string.Format("{0},{1}",MapSize.Width,MapSize.Height);
				SystemSounds.Beep.Play();
			} else {
				MapSize = new Size(x,y);
			}
		}
	}
}
