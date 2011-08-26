namespace SharedCache.Notify
{
	partial class WinServiceController
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinServiceController));
			this.GrpBoxServiceAvailable = new System.Windows.Forms.GroupBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.LblStatus = new System.Windows.Forms.Label();
			this.BtnStop = new System.Windows.Forms.Button();
			this.BtnStart = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.BtnCloseWindow = new System.Windows.Forms.Button();
			this.LblServiceNotAvailable = new System.Windows.Forms.Label();
			this.GrpBoxServiceAvailable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// GrpBoxServiceAvailable
			// 
			this.GrpBoxServiceAvailable.Controls.Add(this.pictureBox1);
			this.GrpBoxServiceAvailable.Controls.Add(this.LblStatus);
			this.GrpBoxServiceAvailable.Controls.Add(this.BtnStop);
			this.GrpBoxServiceAvailable.Controls.Add(this.BtnStart);
			this.GrpBoxServiceAvailable.Controls.Add(this.label1);
			this.GrpBoxServiceAvailable.Location = new System.Drawing.Point(13, 13);
			this.GrpBoxServiceAvailable.Name = "GrpBoxServiceAvailable";
			this.GrpBoxServiceAvailable.Size = new System.Drawing.Size(450, 134);
			this.GrpBoxServiceAvailable.TabIndex = 0;
			this.GrpBoxServiceAvailable.TabStop = false;
			this.GrpBoxServiceAvailable.Text = "Windows Service";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::SharedCache.Notify.Resource.SanscastleDocLogo;
			this.pictureBox1.Location = new System.Drawing.Point(20, 25);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(62, 65);
			this.pictureBox1.TabIndex = 3;
			this.pictureBox1.TabStop = false;
			// 
			// LblStatus
			// 
			this.LblStatus.AutoSize = true;
			this.LblStatus.Location = new System.Drawing.Point(171, 25);
			this.LblStatus.Name = "LblStatus";
			this.LblStatus.Size = new System.Drawing.Size(35, 13);
			this.LblStatus.TabIndex = 4;
			this.LblStatus.Text = "label2";
			// 
			// BtnStop
			// 
			this.BtnStop.Location = new System.Drawing.Point(228, 105);
			this.BtnStop.Name = "BtnStop";
			this.BtnStop.Size = new System.Drawing.Size(216, 23);
			this.BtnStop.TabIndex = 3;
			this.BtnStop.Text = "Stop";
			this.BtnStop.UseVisualStyleBackColor = true;
			this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
			// 
			// BtnStart
			// 
			this.BtnStart.Location = new System.Drawing.Point(7, 105);
			this.BtnStart.Name = "BtnStart";
			this.BtnStart.Size = new System.Drawing.Size(216, 23);
			this.BtnStart.TabIndex = 1;
			this.BtnStart.Text = "Start";
			this.BtnStart.UseVisualStyleBackColor = true;
			this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(88, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Current Status:";
			// 
			// BtnCloseWindow
			// 
			this.BtnCloseWindow.Location = new System.Drawing.Point(318, 153);
			this.BtnCloseWindow.Name = "BtnCloseWindow";
			this.BtnCloseWindow.Size = new System.Drawing.Size(139, 23);
			this.BtnCloseWindow.TabIndex = 1;
			this.BtnCloseWindow.Text = "Close";
			this.BtnCloseWindow.UseVisualStyleBackColor = true;
			this.BtnCloseWindow.Click += new System.EventHandler(this.BtnCloseWindow_Click);
			// 
			// LblServiceNotAvailable
			// 
			this.LblServiceNotAvailable.AutoSize = true;
			this.LblServiceNotAvailable.Location = new System.Drawing.Point(17, 158);
			this.LblServiceNotAvailable.Name = "LblServiceNotAvailable";
			this.LblServiceNotAvailable.Size = new System.Drawing.Size(188, 13);
			this.LblServiceNotAvailable.TabIndex = 2;
			this.LblServiceNotAvailable.Text = "text in case that no service is installed.";
			// 
			// WinServiceController
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(478, 180);
			this.Controls.Add(this.LblServiceNotAvailable);
			this.Controls.Add(this.BtnCloseWindow);
			this.Controls.Add(this.GrpBoxServiceAvailable);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WinServiceController";
			this.Text = "Shared Cache Windows Service Controller";
			this.GrpBoxServiceAvailable.ResumeLayout(false);
			this.GrpBoxServiceAvailable.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox GrpBoxServiceAvailable;
		private System.Windows.Forms.Button BtnStop;
		private System.Windows.Forms.Button BtnStart;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button BtnCloseWindow;
		private System.Windows.Forms.Label LblStatus;
		private System.Windows.Forms.Label LblServiceNotAvailable;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}