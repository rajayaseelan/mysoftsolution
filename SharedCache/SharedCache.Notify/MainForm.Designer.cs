namespace SharedCache.Notify
{
	/// <summary>
	/// <b>Main Window / notify bar</b>
	/// </summary>
	partial class MainForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MainEntryRestart = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.MainEntryNetworkFamily = new System.Windows.Forms.ToolStripMenuItem();
			this.MainEntryServiceController = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.commonRuntimeLanguageCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.checkSharedCacheVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.checkUsageOfHashCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MainEntryStatus = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.MainEntryAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MainEntryExit = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.notifyIcon1.BalloonTipText = "halli hallo whats up??";
			this.notifyIcon1.BalloonTipTitle = "just some text :-)";
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "Shared Cache Controller";
			this.notifyIcon1.Visible = true;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MainEntryRestart,
            this.toolStripSeparator3,
            this.MainEntryNetworkFamily,
            this.MainEntryServiceController,
            this.toolStripMenuItem2,
            this.MainEntryStatus,
            this.toolStripSeparator2,
            this.toolStripMenuItem1,
            this.toolStripSeparator4,
            this.MainEntryAbout,
            this.toolStripSeparator1,
            this.MainEntryExit});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(178, 226);
			// 
			// MainEntryRestart
			// 
			this.MainEntryRestart.Enabled = false;
			this.MainEntryRestart.Name = "MainEntryRestart";
			this.MainEntryRestart.Size = new System.Drawing.Size(177, 22);
			this.MainEntryRestart.Text = "Restart Service";
			this.MainEntryRestart.Click += new System.EventHandler(this.Restart_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(174, 6);
			// 
			// MainEntryNetworkFamily
			// 
			this.MainEntryNetworkFamily.Enabled = false;
			this.MainEntryNetworkFamily.Name = "MainEntryNetworkFamily";
			this.MainEntryNetworkFamily.Size = new System.Drawing.Size(177, 22);
			this.MainEntryNetworkFamily.Text = "Network Familiy";
			this.MainEntryNetworkFamily.Click += new System.EventHandler(this.NetworkFamily_Click);
			// 
			// MainEntryServiceController
			// 
			this.MainEntryServiceController.Enabled = false;
			this.MainEntryServiceController.Name = "MainEntryServiceController";
			this.MainEntryServiceController.Size = new System.Drawing.Size(177, 22);
			this.MainEntryServiceController.Text = "Service Controller";
			this.MainEntryServiceController.Click += new System.EventHandler(this.ServiceController_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commonRuntimeLanguageCheckToolStripMenuItem,
            this.checkSharedCacheVersionToolStripMenuItem,
            this.checkUsageOfHashCodeToolStripMenuItem});
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(177, 22);
			this.toolStripMenuItem2.Text = "Environment Check";
			// 
			// commonRuntimeLanguageCheckToolStripMenuItem
			// 
			this.commonRuntimeLanguageCheckToolStripMenuItem.Name = "commonRuntimeLanguageCheckToolStripMenuItem";
			this.commonRuntimeLanguageCheckToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
			this.commonRuntimeLanguageCheckToolStripMenuItem.Text = "Check - Common Language Runtime (CLR) Version";
			this.commonRuntimeLanguageCheckToolStripMenuItem.Click += new System.EventHandler(this.commonRuntimeLanguageCheckToolStripMenuItem_Click);
			// 
			// checkSharedCacheVersionToolStripMenuItem
			// 
			this.checkSharedCacheVersionToolStripMenuItem.Name = "checkSharedCacheVersionToolStripMenuItem";
			this.checkSharedCacheVersionToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
			this.checkSharedCacheVersionToolStripMenuItem.Text = "Check - Shared Cache Version";
			this.checkSharedCacheVersionToolStripMenuItem.Click += new System.EventHandler(this.checkSharedCacheVersionToolStripMenuItem_Click);
			// 
			// checkUsageOfHashCodeToolStripMenuItem
			// 
			this.checkUsageOfHashCodeToolStripMenuItem.Name = "checkUsageOfHashCodeToolStripMenuItem";
			this.checkUsageOfHashCodeToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
			this.checkUsageOfHashCodeToolStripMenuItem.Text = "Check - Usage of Hash Code";
			this.checkUsageOfHashCodeToolStripMenuItem.Click += new System.EventHandler(this.checkUsageOfHashCodeToolStripMenuItem_Click);
			// 
			// MainEntryStatus
			// 
			this.MainEntryStatus.Name = "MainEntryStatus";
			this.MainEntryStatus.Size = new System.Drawing.Size(177, 22);
			this.MainEntryStatus.Text = "Status";
			this.MainEntryStatus.Click += new System.EventHandler(this.Status_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(174, 6);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
			this.toolStripMenuItem1.Text = "Register";
			this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(174, 6);
			// 
			// MainEntryAbout
			// 
			this.MainEntryAbout.Name = "MainEntryAbout";
			this.MainEntryAbout.Size = new System.Drawing.Size(177, 22);
			this.MainEntryAbout.Text = "About";
			this.MainEntryAbout.Click += new System.EventHandler(this.About_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(174, 6);
			// 
			// MainEntryExit
			// 
			this.MainEntryExit.Name = "MainEntryExit";
			this.MainEntryExit.Size = new System.Drawing.Size(177, 22);
			this.MainEntryExit.Text = "Exit";
			this.MainEntryExit.Click += new System.EventHandler(this.Exit_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "MainForm";
			this.Opacity = 0;
			this.ShowInTaskbar = false;
			this.Text = "MainForm";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem MainEntryStatus;
		private System.Windows.Forms.ToolStripMenuItem MainEntryAbout;
		private System.Windows.Forms.ToolStripMenuItem MainEntryExit;
		private System.Windows.Forms.ToolStripMenuItem MainEntryRestart;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem MainEntryNetworkFamily;
		private System.Windows.Forms.ToolStripMenuItem MainEntryServiceController;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem commonRuntimeLanguageCheckToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem checkSharedCacheVersionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem checkUsageOfHashCodeToolStripMenuItem;
	}
}

