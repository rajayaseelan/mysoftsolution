namespace SharedCache.Registration
{
	partial class Register
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Register));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.BtnClose = new System.Windows.Forms.Button();
			this.cmbInfo = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cmbUsage = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtEmail = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtFullName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtCompanyName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ckbUpdate = new System.Windows.Forms.CheckBox();
			this.BtnRegister = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.BtnClose);
			this.groupBox1.Controls.Add(this.cmbInfo);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.cmbUsage);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.txtEmail);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.txtFullName);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.txtCompanyName);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.ckbUpdate);
			this.groupBox1.Controls.Add(this.BtnRegister);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(380, 183);
			this.groupBox1.TabIndex = 100;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Your Registration Details : ";
			// 
			// BtnClose
			// 
			this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnClose.Location = new System.Drawing.Point(262, 148);
			this.BtnClose.Name = "BtnClose";
			this.BtnClose.Size = new System.Drawing.Size(112, 23);
			this.BtnClose.TabIndex = 7;
			this.BtnClose.Text = "No Thanks!";
			this.BtnClose.UseVisualStyleBackColor = true;
			this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// cmbInfo
			// 
			this.cmbInfo.FormattingEnabled = true;
			this.cmbInfo.Items.AddRange(new object[] {
            "Internet - Search Engine",
            "Internet - Blogs / Forums",
            "Printed Media ",
            "Other"});
			this.cmbInfo.Location = new System.Drawing.Point(107, 121);
			this.cmbInfo.Name = "cmbInfo";
			this.cmbInfo.Size = new System.Drawing.Size(267, 21);
			this.cmbInfo.TabIndex = 4;
			this.cmbInfo.Text = "How did you hear about SharedCache.com?";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(7, 123);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(34, 13);
			this.label6.TabIndex = 100;
			this.label6.Text = "Info : ";
			// 
			// cmbUsage
			// 
			this.cmbUsage.FormattingEnabled = true;
			this.cmbUsage.Items.AddRange(new object[] {
            "Distribution Mode - Single Server",
            "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -" +
                " - ",
            "Distribution Mode - Multi Server - (02 - 04 Servers)",
            "Distribution Mode - Multi Server - (05 - 10 Servers)",
            "Distribution Mode - Multi Server - (more 10 Servers)",
            "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -" +
                " - ",
            "Replication Mode - Multi Server - (02 - 04 Servers)",
            "Replication Mode - Multi Server - (05 - 10 Servers)",
            "Replication Mode - Multi Server - (more 10 Servers)",
            "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -" +
                " - "});
			this.cmbUsage.Location = new System.Drawing.Point(107, 94);
			this.cmbUsage.Name = "cmbUsage";
			this.cmbUsage.Size = new System.Drawing.Size(267, 21);
			this.cmbUsage.TabIndex = 3;
			this.cmbUsage.Text = "How do you use SharedCache.com?";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(7, 148);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(99, 13);
			this.label5.TabIndex = 100;
			this.label5.Text = "Send me updates : ";
			// 
			// txtEmail
			// 
			this.txtEmail.Location = new System.Drawing.Point(107, 68);
			this.txtEmail.Name = "txtEmail";
			this.txtEmail.Size = new System.Drawing.Size(267, 20);
			this.txtEmail.TabIndex = 2;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 71);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(66, 13);
			this.label4.TabIndex = 100;
			this.label4.Text = "Your Email : ";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 96);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(47, 13);
			this.label3.TabIndex = 100;
			this.label3.Text = "Usage : ";
			// 
			// txtFullName
			// 
			this.txtFullName.Location = new System.Drawing.Point(107, 43);
			this.txtFullName.Name = "txtFullName";
			this.txtFullName.Size = new System.Drawing.Size(267, 20);
			this.txtFullName.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 46);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 100;
			this.label2.Text = "Full Name : ";
			// 
			// txtCompanyName
			// 
			this.txtCompanyName.Location = new System.Drawing.Point(107, 17);
			this.txtCompanyName.Name = "txtCompanyName";
			this.txtCompanyName.Size = new System.Drawing.Size(267, 20);
			this.txtCompanyName.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 100;
			this.label1.Text = "Company Name : ";
			// 
			// ckbUpdate
			// 
			this.ckbUpdate.AutoSize = true;
			this.ckbUpdate.Checked = true;
			this.ckbUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ckbUpdate.Location = new System.Drawing.Point(107, 148);
			this.ckbUpdate.Name = "ckbUpdate";
			this.ckbUpdate.Size = new System.Drawing.Size(15, 14);
			this.ckbUpdate.TabIndex = 5;
			this.ckbUpdate.UseVisualStyleBackColor = true;
			// 
			// BtnRegister
			// 
			this.BtnRegister.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnRegister.Location = new System.Drawing.Point(128, 148);
			this.BtnRegister.Name = "BtnRegister";
			this.BtnRegister.Size = new System.Drawing.Size(128, 23);
			this.BtnRegister.TabIndex = 6;
			this.BtnRegister.Text = "Register";
			this.BtnRegister.UseVisualStyleBackColor = true;
			this.BtnRegister.Click += new System.EventHandler(this.BtnRegister_Click);
			// 
			// Register
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(408, 208);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Register";
			this.Text = " Register your installation for SharedCache.com";
			this.TopMost = true;
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox ckbUpdate;
		private System.Windows.Forms.Button BtnRegister;
		private System.Windows.Forms.ComboBox cmbInfo;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cmbUsage;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtEmail;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtFullName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtCompanyName;
		private System.Windows.Forms.Button BtnClose;
	}
}

