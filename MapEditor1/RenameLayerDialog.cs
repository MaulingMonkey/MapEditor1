using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapEditor1 {
	partial class RenameLayerDialog : Form {
		Layer Layer;

		public RenameLayerDialog(): this(new Layer(){Name="Demo",Visible=true}) {}

		public RenameLayerDialog( Layer layer ) {
			InitializeComponent();

			Layer = layer;
			tbLayerName.Text       = layer.Name;
			cbLayerVisible.Checked = layer.Visible;
		}

		private void bOK_Click( object sender, EventArgs e ) {
			Layer.Name    = tbLayerName.Text;
			Layer.Visible = cbLayerVisible.Checked;
			Close();
		}

		private void bCancel_Click( object sender, EventArgs e ) {
			Close();
		}
	}
}
