namespace MapEditor1 {
	partial class RenameLayerDialog {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.lblLayerName = new System.Windows.Forms.Label();
			this.tbLayerName = new System.Windows.Forms.TextBox();
			this.cbLayerVisible = new System.Windows.Forms.CheckBox();
			this.bOK = new System.Windows.Forms.Button();
			this.bCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblLayerName
			// 
			this.lblLayerName.AutoSize = true;
			this.lblLayerName.Location = new System.Drawing.Point(12, 9);
			this.lblLayerName.Name = "lblLayerName";
			this.lblLayerName.Size = new System.Drawing.Size(67, 13);
			this.lblLayerName.TabIndex = 0;
			this.lblLayerName.Text = "Layer Name:";
			// 
			// tbLayerName
			// 
			this.tbLayerName.Location = new System.Drawing.Point(85, 6);
			this.tbLayerName.Name = "tbLayerName";
			this.tbLayerName.Size = new System.Drawing.Size(100, 20);
			this.tbLayerName.TabIndex = 1;
			// 
			// cbLayerVisible
			// 
			this.cbLayerVisible.AutoSize = true;
			this.cbLayerVisible.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbLayerVisible.Location = new System.Drawing.Point(129, 32);
			this.cbLayerVisible.Name = "cbLayerVisible";
			this.cbLayerVisible.Size = new System.Drawing.Size(56, 17);
			this.cbLayerVisible.TabIndex = 2;
			this.cbLayerVisible.Text = "Visible";
			this.cbLayerVisible.UseVisualStyleBackColor = true;
			// 
			// bOK
			// 
			this.bOK.Location = new System.Drawing.Point(12, 55);
			this.bOK.Name = "bOK";
			this.bOK.Size = new System.Drawing.Size(75, 23);
			this.bOK.TabIndex = 3;
			this.bOK.Text = "OK";
			this.bOK.UseVisualStyleBackColor = true;
			this.bOK.Click += new System.EventHandler(this.bOK_Click);
			// 
			// bCancel
			// 
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(109, 55);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(75, 23);
			this.bCancel.TabIndex = 4;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// RenameLayerDialog
			// 
			this.AcceptButton = this.bOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(196, 88);
			this.Controls.Add(this.bCancel);
			this.Controls.Add(this.bOK);
			this.Controls.Add(this.cbLayerVisible);
			this.Controls.Add(this.tbLayerName);
			this.Controls.Add(this.lblLayerName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RenameLayerDialog";
			this.Text = "Rename Layer";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblLayerName;
		private System.Windows.Forms.TextBox tbLayerName;
		private System.Windows.Forms.CheckBox cbLayerVisible;
		private System.Windows.Forms.Button bOK;
		private System.Windows.Forms.Button bCancel;
	}
}