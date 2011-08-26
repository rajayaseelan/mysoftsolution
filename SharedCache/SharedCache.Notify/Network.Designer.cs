namespace SharedCache.Notify
{
	partial class Network
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Network));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.LblAmount = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.BtnResetForm = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.TxtSearchKey = new System.Windows.Forms.TextBox();
			this.BtnSearch = new System.Windows.Forms.Button();
			this.BtnRegularExpressionSearch = new System.Windows.Forms.Button();
			this.TxtSearchRegEx = new System.Windows.Forms.TextBox();
			this.LbxServerNodes = new System.Windows.Forms.ListBox();
			this.LblAvailableKeys = new System.Windows.Forms.Label();
			this.LblCacheNodes = new System.Windows.Forms.Label();
			this.BtnClearSelectedKeys = new System.Windows.Forms.Button();
			this.LbxNodeKey = new System.Windows.Forms.ListBox();
			this.GrpCacheStats = new System.Windows.Forms.GroupBox();
			this.LblScVersion = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.LblClrVersion = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.LbxServerClrVersion = new System.Windows.Forms.ListBox();
			this.GvStats = new System.Windows.Forms.DataGridView();
			this.TtCacheNodes = new System.Windows.Forms.ToolTip(this.components);
			this.TtCacheNodeKeys = new System.Windows.Forms.ToolTip(this.components);
			this.TtRegularExpression = new System.Windows.Forms.ToolTip(this.components);
			this.TtSearch = new System.Windows.Forms.ToolTip(this.components);
			this.BtnClearCache = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.GrpCacheStats.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GvStats)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.GrpCacheStats);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(794, 502);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Shared Cache Overview";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.BtnClearCache);
			this.groupBox2.Controls.Add(this.LblAmount);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.BtnResetForm);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.TxtSearchKey);
			this.groupBox2.Controls.Add(this.BtnSearch);
			this.groupBox2.Controls.Add(this.BtnRegularExpressionSearch);
			this.groupBox2.Controls.Add(this.TxtSearchRegEx);
			this.groupBox2.Controls.Add(this.LbxServerNodes);
			this.groupBox2.Controls.Add(this.LblAvailableKeys);
			this.groupBox2.Controls.Add(this.LblCacheNodes);
			this.groupBox2.Controls.Add(this.BtnClearSelectedKeys);
			this.groupBox2.Controls.Add(this.LbxNodeKey);
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox2.Location = new System.Drawing.Point(6, 167);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(781, 329);
			this.groupBox2.TabIndex = 14;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Cache Overview";
			// 
			// LblAmount
			// 
			this.LblAmount.AutoSize = true;
			this.LblAmount.Location = new System.Drawing.Point(695, 132);
			this.LblAmount.Name = "LblAmount";
			this.LblAmount.Size = new System.Drawing.Size(35, 13);
			this.LblAmount.TabIndex = 21;
			this.LblAmount.Text = ". . . .";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(594, 132);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(107, 13);
			this.label3.TabIndex = 20;
			this.label3.Text = "Amount of Keys : ";
			// 
			// BtnResetForm
			// 
			this.BtnResetForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnResetForm.Location = new System.Drawing.Point(593, 258);
			this.BtnResetForm.Name = "BtnResetForm";
			this.BtnResetForm.Size = new System.Drawing.Size(180, 23);
			this.BtnResetForm.TabIndex = 19;
			this.BtnResetForm.Text = "Reset Form";
			this.BtnResetForm.UseVisualStyleBackColor = true;
			this.BtnResetForm.Click += new System.EventHandler(this.BtnResetForm_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(366, 78);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(62, 13);
			this.label2.TabIndex = 18;
			this.label2.Text = "Key Search";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(366, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(135, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "Regular Expression Search";
			// 
			// TxtSearchKey
			// 
			this.TxtSearchKey.Location = new System.Drawing.Point(369, 94);
			this.TxtSearchKey.Name = "TxtSearchKey";
			this.TxtSearchKey.Size = new System.Drawing.Size(218, 20);
			this.TxtSearchKey.TabIndex = 16;
			// 
			// BtnSearch
			// 
			this.BtnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnSearch.Location = new System.Drawing.Point(593, 92);
			this.BtnSearch.Name = "BtnSearch";
			this.BtnSearch.Size = new System.Drawing.Size(180, 23);
			this.BtnSearch.TabIndex = 15;
			this.BtnSearch.Text = "Key Search";
			this.BtnSearch.UseVisualStyleBackColor = true;
			this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
			// 
			// BtnRegularExpressionSearch
			// 
			this.BtnRegularExpressionSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnRegularExpressionSearch.Location = new System.Drawing.Point(593, 46);
			this.BtnRegularExpressionSearch.Name = "BtnRegularExpressionSearch";
			this.BtnRegularExpressionSearch.Size = new System.Drawing.Size(180, 23);
			this.BtnRegularExpressionSearch.TabIndex = 14;
			this.BtnRegularExpressionSearch.Text = "RegEx Search";
			this.BtnRegularExpressionSearch.UseVisualStyleBackColor = true;
			this.BtnRegularExpressionSearch.Click += new System.EventHandler(this.BtnRegularExpressionSearch_Click);
			// 
			// TxtSearchRegEx
			// 
			this.TxtSearchRegEx.Location = new System.Drawing.Point(369, 48);
			this.TxtSearchRegEx.Name = "TxtSearchRegEx";
			this.TxtSearchRegEx.Size = new System.Drawing.Size(218, 20);
			this.TxtSearchRegEx.TabIndex = 13;
			// 
			// LbxServerNodes
			// 
			this.LbxServerNodes.FormattingEnabled = true;
			this.LbxServerNodes.Location = new System.Drawing.Point(9, 32);
			this.LbxServerNodes.Name = "LbxServerNodes";
			this.LbxServerNodes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.LbxServerNodes.Size = new System.Drawing.Size(351, 82);
			this.LbxServerNodes.TabIndex = 11;
			this.LbxServerNodes.SelectedIndexChanged += new System.EventHandler(this.DisplayNodeSelectionResult);
			// 
			// LblAvailableKeys
			// 
			this.LblAvailableKeys.AutoSize = true;
			this.LblAvailableKeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LblAvailableKeys.Location = new System.Drawing.Point(6, 117);
			this.LblAvailableKeys.Name = "LblAvailableKeys";
			this.LblAvailableKeys.Size = new System.Drawing.Size(137, 13);
			this.LblAvailableKeys.TabIndex = 10;
			this.LblAvailableKeys.Text = "A list with all available Key\'s";
			// 
			// LblCacheNodes
			// 
			this.LblCacheNodes.AutoSize = true;
			this.LblCacheNodes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LblCacheNodes.Location = new System.Drawing.Point(6, 16);
			this.LblCacheNodes.Name = "LblCacheNodes";
			this.LblCacheNodes.Size = new System.Drawing.Size(163, 13);
			this.LblCacheNodes.TabIndex = 7;
			this.LblCacheNodes.Text = "Configured Shared Cache Nodes";
			// 
			// BtnClearSelectedKeys
			// 
			this.BtnClearSelectedKeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnClearSelectedKeys.Location = new System.Drawing.Point(593, 229);
			this.BtnClearSelectedKeys.Name = "BtnClearSelectedKeys";
			this.BtnClearSelectedKeys.Size = new System.Drawing.Size(180, 23);
			this.BtnClearSelectedKeys.TabIndex = 12;
			this.BtnClearSelectedKeys.Text = "Clear Selected Keys";
			this.BtnClearSelectedKeys.UseVisualStyleBackColor = true;
			this.BtnClearSelectedKeys.Click += new System.EventHandler(this.BtnClearSelectedKeys_Click);
			// 
			// LbxNodeKey
			// 
			this.LbxNodeKey.FormattingEnabled = true;
			this.LbxNodeKey.Location = new System.Drawing.Point(9, 132);
			this.LbxNodeKey.Name = "LbxNodeKey";
			this.LbxNodeKey.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.LbxNodeKey.Size = new System.Drawing.Size(578, 186);
			this.LbxNodeKey.TabIndex = 8;
			this.LbxNodeKey.SelectedIndexChanged += new System.EventHandler(this.SetFocusToDelete);
			// 
			// GrpCacheStats
			// 
			this.GrpCacheStats.Controls.Add(this.LblScVersion);
			this.GrpCacheStats.Controls.Add(this.label5);
			this.GrpCacheStats.Controls.Add(this.LblClrVersion);
			this.GrpCacheStats.Controls.Add(this.label4);
			this.GrpCacheStats.Controls.Add(this.LbxServerClrVersion);
			this.GrpCacheStats.Controls.Add(this.GvStats);
			this.GrpCacheStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GrpCacheStats.Location = new System.Drawing.Point(6, 19);
			this.GrpCacheStats.Name = "GrpCacheStats";
			this.GrpCacheStats.Size = new System.Drawing.Size(781, 141);
			this.GrpCacheStats.TabIndex = 13;
			this.GrpCacheStats.TabStop = false;
			this.GrpCacheStats.Text = "Cache Statistics and Server CLR Information";
			// 
			// LblScVersion
			// 
			this.LblScVersion.AutoSize = true;
			this.LblScVersion.Location = new System.Drawing.Point(654, 32);
			this.LblScVersion.Name = "LblScVersion";
			this.LblScVersion.Size = new System.Drawing.Size(81, 13);
			this.LblScVersion.TabIndex = 14;
			this.LblScVersion.Text = "LblScVersion";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(498, 32);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(153, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Your Current Notify Version is : ";
			// 
			// LblClrVersion
			// 
			this.LblClrVersion.AutoSize = true;
			this.LblClrVersion.Location = new System.Drawing.Point(654, 19);
			this.LblClrVersion.Name = "LblClrVersion";
			this.LblClrVersion.Size = new System.Drawing.Size(81, 13);
			this.LblClrVersion.TabIndex = 12;
			this.LblClrVersion.Text = "LblClrVersion";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(369, 19);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(282, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Your Local CLR (Common Language Runtime) Version is : ";
			// 
			// LbxServerClrVersion
			// 
			this.LbxServerClrVersion.FormattingEnabled = true;
			this.LbxServerClrVersion.HorizontalScrollbar = true;
			this.LbxServerClrVersion.Location = new System.Drawing.Point(369, 71);
			this.LbxServerClrVersion.Name = "LbxServerClrVersion";
			this.LbxServerClrVersion.Size = new System.Drawing.Size(404, 56);
			this.LbxServerClrVersion.TabIndex = 10;
			// 
			// GvStats
			// 
			this.GvStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.GvStats.Location = new System.Drawing.Point(6, 19);
			this.GvStats.Name = "GvStats";
			this.GvStats.Size = new System.Drawing.Size(354, 108);
			this.GvStats.TabIndex = 9;
			// 
			// BtnClearCache
			// 
			this.BtnClearCache.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnClearCache.Location = new System.Drawing.Point(593, 288);
			this.BtnClearCache.Name = "BtnClearCache";
			this.BtnClearCache.Size = new System.Drawing.Size(180, 23);
			this.BtnClearCache.TabIndex = 22;
			this.BtnClearCache.Text = "Clear all Items from Cache Nodes";
			this.BtnClearCache.UseVisualStyleBackColor = true;
			this.BtnClearCache.Click += new System.EventHandler(this.BtnClearCache_Click);
			// 
			// Network
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(818, 526);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Network";
			this.Text = "Shared Cache Network Overview";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.GrpCacheStats.ResumeLayout(false);
			this.GrpCacheStats.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GvStats)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label LblCacheNodes;
		private System.Windows.Forms.ListBox LbxNodeKey;
		private System.Windows.Forms.ToolTip TtCacheNodes;
		private System.Windows.Forms.DataGridView GvStats;
		private System.Windows.Forms.Label LblAvailableKeys;
		private System.Windows.Forms.ToolTip TtCacheNodeKeys;
		private System.Windows.Forms.ListBox LbxServerNodes;
		private System.Windows.Forms.Button BtnClearSelectedKeys;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox TxtSearchKey;
		private System.Windows.Forms.Button BtnSearch;
		private System.Windows.Forms.Button BtnRegularExpressionSearch;
		private System.Windows.Forms.TextBox TxtSearchRegEx;
		private System.Windows.Forms.GroupBox GrpCacheStats;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button BtnResetForm;
		private System.Windows.Forms.ToolTip TtRegularExpression;
		private System.Windows.Forms.ToolTip TtSearch;
		private System.Windows.Forms.Label LblAmount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ListBox LbxServerClrVersion;
		private System.Windows.Forms.Label LblClrVersion;
		private System.Windows.Forms.Label LblScVersion;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button BtnClearCache;
	}
}