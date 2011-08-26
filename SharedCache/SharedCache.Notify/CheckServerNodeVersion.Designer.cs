namespace SharedCache.Notify
{
	partial class CheckServerNodeVersion
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckServerNodeVersion));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.LblMessage = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.LbServerNodes = new System.Windows.Forms.ListBox();
			this.LblVersion = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.BtnClose = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.LblMessage);
			this.groupBox1.Controls.Add(this.pictureBox1);
			this.groupBox1.Controls.Add(this.LbServerNodes);
			this.groupBox1.Controls.Add(this.LblVersion);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.BtnClose);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(13, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(417, 248);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Shared Cache Server Nodes";
			// 
			// LblMessage
			// 
			this.LblMessage.AutoSize = true;
			this.LblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LblMessage.ForeColor = System.Drawing.Color.Red;
			this.LblMessage.Location = new System.Drawing.Point(9, 220);
			this.LblMessage.Name = "LblMessage";
			this.LblMessage.Size = new System.Drawing.Size(41, 13);
			this.LblMessage.TabIndex = 8;
			this.LblMessage.Text = "label3";
			// 
			// pictureBox1
			// 
			this.pictureBox1.ErrorImage = global::SharedCache.Notify.Resource.NOK;
			this.pictureBox1.InitialImage = global::SharedCache.Notify.Resource.SanscastleDocLogo;
			this.pictureBox1.Location = new System.Drawing.Point(361, 36);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(50, 50);
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			// 
			// LbServerNodes
			// 
			this.LbServerNodes.FormattingEnabled = true;
			this.LbServerNodes.Location = new System.Drawing.Point(9, 92);
			this.LbServerNodes.Name = "LbServerNodes";
			this.LbServerNodes.Size = new System.Drawing.Size(402, 121);
			this.LbServerNodes.TabIndex = 6;
			// 
			// LblVersion
			// 
			this.LblVersion.AutoSize = true;
			this.LblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LblVersion.Location = new System.Drawing.Point(118, 58);
			this.LblVersion.Name = "LblVersion";
			this.LblVersion.Size = new System.Drawing.Size(41, 13);
			this.LblVersion.TabIndex = 3;
			this.LblVersion.Text = "label3";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(106, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Your Notify Version : ";
			// 
			// BtnClose
			// 
			this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnClose.Location = new System.Drawing.Point(336, 219);
			this.BtnClose.Name = "BtnClose";
			this.BtnClose.Size = new System.Drawing.Size(75, 23);
			this.BtnClose.TabIndex = 1;
			this.BtnClose.Text = "Close";
			this.BtnClose.UseVisualStyleBackColor = true;
			this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 16);
			this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.label1.MaximumSize = new System.Drawing.Size(400, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(384, 26);
			this.label1.TabIndex = 0;
			this.label1.Text = "Below you see a list with all \'configured\' server nodes, ensure you\'re using on a" +
					"ll your nodes the same shared cache version number.";
			// 
			// CheckServerNodeVersion
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.BtnClose;
			this.ClientSize = new System.Drawing.Size(444, 275);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CheckServerNodeVersion";
			this.Text = "Check Shared Cache Versions";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label LblVersion;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button BtnClose;
		private System.Windows.Forms.ListBox LbServerNodes;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label LblMessage;
	}
}