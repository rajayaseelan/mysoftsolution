using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using COM = SharedCache.WinServiceCommon;
using CACHE = SharedCache.WinServiceCommon.Provider.Cache.IndexusDistributionCache;

namespace SharedCache.Notify
{
	/// <summary>
	/// A form to display all installed versions of shared cache
	/// </summary>
	public partial class CheckServerNodeVersion : Form
	{
		// private thread to execute the logic and user able to see the form imidiatly
		private Thread worker = null;

		/// <summary>
		/// Over this form clients see all configured shared cache versions
		/// </summary>
		public CheckServerNodeVersion()
		{
			InitializeComponent();
			
			System.Drawing.Size s = new Size(50, 50);
			this.pictureBox1.Image = new Bitmap(Resource.SanscastleDocLogo, s);

			this.Shown += new EventHandler(CheckServerNodeVersion_Shown);			
		}

		/// <summary>
		/// Loading data within different thread then UI thread.
		/// </summary>
		private void LoadData()
		{
			// specific stuff
			this.SetNotifyVersion();
			this.CheckServerVersions();
		}

		/// <summary>
		/// Start worker thread to load data for this form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void CheckServerNodeVersion_Shown(object sender, EventArgs e)
		{
			this.worker = new Thread(new ThreadStart(this.LoadData));
			this.worker.Start();
		}

		/// <summary>
		/// A delegate method to invoke a method and prevent threading concurrent access
		/// </summary>
		private delegate void DelegateCheckServerVersions();
		/// <summary>
		/// A delegate method to invoke a method and prevent threading concurrent access
		/// </summary>
		private delegate void DelegateSetNotifyVersion();
		
		/// <summary>
		/// Set Version Number of notfiere on frontend label.
		/// </summary>
		private void CheckServerVersions()
		{
			if (this.InvokeRequired)
			{
				DelegateCheckServerVersions inv = new DelegateCheckServerVersions(this.CheckServerVersions);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				this.Cursor = Cursors.WaitCursor;
				
				bool sameVersion = true;

				IDictionary<string, string> result = CACHE.SharedCache.ServerNodeVersionSharedCache();
				string notifyVersion = new AssemblyInfo().Version;
				int number = 1;

				foreach (var item in CACHE.SharedCache.Servers)
				{
					if(result.ContainsKey(item))
					{
						string name = string.Format("{0}. Server Name: {1}; Shared Cache Version: {2}",
							number++,
							item,
							result[item]);

						this.LbServerNodes.Items.Add(new Common.ComboBoxItem(name, 0));

						if (!notifyVersion.Equals(result[item]))
						{
							sameVersion = false;
						}
					}
				}
				// sort all items
				this.LbServerNodes.Sorted = true;

				// only display OK / NOK in case we can connect to server
				if (result.Count > 0)
				{
					if (sameVersion)
					{
						this.pictureBox1.Image = new Bitmap(Resource.OK);
					}
					else
					{
						this.pictureBox1.Image = new Bitmap(Resource.NOK);
					}
					// 
					this.LblMessage.Text = string.Empty;
				}
				else
				{
					System.Drawing.Size s = new Size(50,50);
					this.pictureBox1.Image = new Bitmap(Resource.SanscastleDocLogo, s);
					this.LblMessage.Text = "Could not find any Server - check your 'notify config'!";
				}
				
				this.Cursor = Cursors.Default;
			}
		}
		
		/// <summary>
		/// Set Version Number of notfiere on frontend label.
		/// </summary>
		private void SetNotifyVersion()
		{
			if (this.InvokeRequired)
			{
				DelegateSetNotifyVersion inv = new DelegateSetNotifyVersion(this.SetNotifyVersion);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				this.LblVersion.Text = new AssemblyInfo().Version;
			}
		}

		/// <summary>
		/// Close the form
		/// </summary>
		/// <param name="sender">A <see cref="object"/> object</param>
		/// <param name="e"></param>
		private void BtnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

	}
}
