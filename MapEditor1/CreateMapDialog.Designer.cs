namespace MapEditor1 {
	partial class CreateMapDialog {
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
			this.lblGridSnapSize = new System.Windows.Forms.Label();
			this.tbGridSnapSize = new System.Windows.Forms.TextBox();
			this.lblMapSize = new System.Windows.Forms.Label();
			this.tbMapSize = new System.Windows.Forms.TextBox();
			this.bCreate = new System.Windows.Forms.Button();
			this.bCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblGridSnapSize
			// 
			this.lblGridSnapSize.AutoSize = true;
			this.lblGridSnapSize.Location = new System.Drawing.Point(12, 9);
			this.lblGridSnapSize.Name = "lblGridSnapSize";
			this.lblGridSnapSize.Size = new System.Drawing.Size(80, 13);
			this.lblGridSnapSize.TabIndex = 0;
			this.lblGridSnapSize.Text = "Grid Snap Size:";
			// 
			// tbGridSnapSize
			// 
			this.tbGridSnapSize.Location = new System.Drawing.Point(98, 6);
			this.tbGridSnapSize.Name = "tbGridSnapSize";
			this.tbGridSnapSize.Size = new System.Drawing.Size(100, 20);
			this.tbGridSnapSize.TabIndex = 1;
			this.tbGridSnapSize.Leave += new System.EventHandler(this.tbGridSnapSize_Leave);
			// 
			// lblMapSize
			// 
			this.lblMapSize.AutoSize = true;
			this.lblMapSize.Location = new System.Drawing.Point(12, 35);
			this.lblMapSize.Name = "lblMapSize";
			this.lblMapSize.Size = new System.Drawing.Size(80, 13);
			this.lblMapSize.TabIndex = 2;
			this.lblMapSize.Text = "Map Size (w,h):";
			// 
			// tbMapSize
			// 
			this.tbMapSize.Location = new System.Drawing.Point(98, 32);
			this.tbMapSize.Name = "tbMapSize";
			this.tbMapSize.Size = new System.Drawing.Size(100, 20);
			this.tbMapSize.TabIndex = 3;
			this.tbMapSize.Leave += new System.EventHandler(this.tbMapSize_Leave);
			// 
			// bCreate
			// 
			this.bCreate.Location = new System.Drawing.Point(12, 58);
			this.bCreate.Name = "bCreate";
			this.bCreate.Size = new System.Drawing.Size(75, 23);
			this.bCreate.TabIndex = 4;
			this.bCreate.Text = "Create";
			this.bCreate.UseVisualStyleBackColor = true;
			this.bCreate.Click += new System.EventHandler(this.bCreate_Click);
			// 
			// bCancel
			// 
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(127, 58);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(75, 23);
			this.bCancel.TabIndex = 5;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// CreateMapDialog
			// 
			this.AcceptButton = this.bCreate;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(214, 86);
			this.Controls.Add(this.bCancel);
			this.Controls.Add(this.bCreate);
			this.Controls.Add(this.tbMapSize);
			this.Controls.Add(this.lblMapSize);
			this.Controls.Add(this.tbGridSnapSize);
			this.Controls.Add(this.lblGridSnapSize);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CreateMapDialog";
			this.Text = "New Map...";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblGridSnapSize;
		private System.Windows.Forms.TextBox tbGridSnapSize;
		private System.Windows.Forms.Label lblMapSize;
		private System.Windows.Forms.TextBox tbMapSize;
		private System.Windows.Forms.Button bCreate;
		private System.Windows.Forms.Button bCancel;
	}
}