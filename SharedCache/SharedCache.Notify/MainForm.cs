#region Copyright (c) Roni Schuetz - All Rights Reserved
// * --------------------------------------------------------------------- *
// *                              Roni Schuetz                             *
// *              Copyright (c) 2008 All Rights reserved                   *
// *                                                                       *
// * Shared Cache high-performance, distributed caching and    *
// * replicated caching system, generic in nature, but intended to         *
// * speeding up dynamic web and / or win applications by alleviating      *
// * database load.                                                        *
// *                                                                       *
// * This Software is written by Roni Schuetz (schuetz AT gmail DOT com)   *
// *                                                                       *
// * This library is free software; you can redistribute it and/or         *
// * modify it under the terms of the GNU Lesser General Public License    *
// * as published by the Free Software Foundation; either version 2.1      *
// * of the License, or (at your option) any later version.                *
// *                                                                       *
// * This library is distributed in the hope that it will be useful,       *
// * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU      *
// * Lesser General Public License for more details.                       *
// *                                                                       *
// * You should have received a copy of the GNU Lesser General Public      *
// * License along with this library; if not, write to the Free            *
// * Software Foundation, Inc., 59 Temple Place, Suite 330,                *
// * Boston, MA 02111-1307 USA                                             *
// *                                                                       *
// *       THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.        *
// * --------------------------------------------------------------------- *
#endregion 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.Notify
{
	/// <summary>
	/// main window.
	/// </summary>
	public partial class MainForm : Form
	{
		CustomUIControls.TaskbarNotifier not = new CustomUIControls.TaskbarNotifier();
		System.Threading.Thread worker = null;
		#region Property: ServiceController
		private Network network;

		/// <summary>
		/// Gets/sets the NetworkOverview 
		/// </summary>
		public Network Network
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.network == null)
					this.network = new Network();
				if (this.network != null && this.network.IsDisposed)
				{
					this.network = null;
					this.network = new Network();
				}

				return this.network;
			}
		}
		#endregion

		#region Property: ServiceController
		private WinServiceController serviceController;

		/// <summary>
		/// Gets/sets the NetworkOverview 
		/// </summary>
		public WinServiceController ServiceController
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.serviceController == null)
					this.serviceController = new WinServiceController();
				if (this.serviceController != null && this.serviceController.IsDisposed)
				{
					this.serviceController = null;
					this.serviceController = new WinServiceController();
				}

				return this.serviceController;
			}
		}
		#endregion

		#region Property: WinAbout
		private About winAbout;
		
		/// <summary>
		/// Gets/sets the WinAbout
		/// </summary>
		public About WinAbout
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.winAbout == null)
					this.winAbout = new About();
				if (this.winAbout != null && this.winAbout.IsDisposed)
				{
					this.winAbout = null;
					this.winAbout = new About();
				}

				return this.winAbout;
			}
		}
		#endregion

		private CheckServerNodeVersion winCheckServerNodeVersion;
		public CheckServerNodeVersion WinCheckServerNodeVersion
		{
			[System.Diagnostics.DebuggerStepThrough]
			get 
			{
				if (this.winCheckServerNodeVersion == null)
					this.winCheckServerNodeVersion = new CheckServerNodeVersion();
				if (this.winCheckServerNodeVersion != null && this.winCheckServerNodeVersion.IsDisposed)
				{
					this.winCheckServerNodeVersion = null;
					this.winCheckServerNodeVersion = new CheckServerNodeVersion();
				}
				return this.winCheckServerNodeVersion;
			}
		}

		private CheckHashCode winCheckHashCode;
		public CheckHashCode WinCheckHashCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.winCheckHashCode == null)
					this.winCheckHashCode = new CheckHashCode();
				if (this.winCheckHashCode != null && this.winCheckHashCode.IsDisposed)
				{
					this.winCheckHashCode = null;
					this.winCheckHashCode = new CheckHashCode();
				}
				return this.winCheckHashCode;
			}
		}

		private CheckServerNodeClr winCheckServerNodeClr;
		public CheckServerNodeClr WinCheckServerNodeClr
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.winCheckServerNodeClr == null)
					this.winCheckServerNodeClr = new CheckServerNodeClr();
				if (this.winCheckServerNodeClr != null && this.winCheckServerNodeClr.IsDisposed)
				{
					this.winCheckServerNodeClr = null;
					this.winCheckServerNodeClr = new CheckServerNodeClr();
				}
				return this.winCheckServerNodeClr;
			}
		}
		
		#region Constructor 
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Form1"/> class.
		/// </summary>
		public MainForm()
		{
			AssemblyInfo ai = new AssemblyInfo();
			COM.Handler.LogHandler.Info("AsmFQName: " + ai.AsmFQName);
			COM.Handler.LogHandler.Info("AsmName: " + ai.AsmName);
			COM.Handler.LogHandler.Info("CodeBase: " + ai.CodeBase);
			COM.Handler.LogHandler.Info("Company: " + ai.Company);
			COM.Handler.LogHandler.Info("Copyright: " + ai.Copyright);
			COM.Handler.LogHandler.Info("Description: " + ai.Description);
			COM.Handler.LogHandler.Info("Product: " + ai.Product);
			COM.Handler.LogHandler.Info("Title: " + ai.Title);
			COM.Handler.LogHandler.Info("Version: " + ai.Version);

			InitializeComponent();
		}

		#endregion Constructor

		/// <summary>
		/// Handles the Load event of the Form1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void MainForm_Load(object sender, EventArgs e)
		{

			System.Drawing.Size s = new Size(14, 14);
			System.Drawing.Icon i = new Icon(Resource.shared_cache, s);
			this.notifyIcon1.Icon = i;

			// enable all options
			if (this.GetRegistryValue())
			{
				foreach (ToolStripItem item in this.contextMenuStrip1.Items)
				{
					if (item.Enabled == false)
					{
						item.Enabled = true;
					}

					if (item.Text.Equals("Register", StringComparison.InvariantCultureIgnoreCase))
					{
						item.Enabled = false;
						item.Text = "Registration Done!";
					}
				}
			}

			this.worker = new System.Threading.Thread(new System.Threading.ThreadStart(this.ShowNoty));
			this.worker.Start();
			// this.ShowNoty();
		}

		private delegate void DelegateShowNotify();

		/// <summary>
		/// Shows the notify window.
		/// </summary>
		private void ShowNoty()
		{
			if (this.InvokeRequired)
			{
				DelegateShowNotify inv = new DelegateShowNotify(this.ShowNoty);
				this.Invoke(inv, new object [] { } );
			}
			else
			{
				not.Hide();
				not.SetBackgroundBitmap(Resource.skin3, Color.FromArgb(255, 0, 255));
				not.SetCloseBitmap(Resource.close, Color.FromArgb(255, 0, 255), new Point(280, 57));
				not.TitleRectangle = new Rectangle(150, 57, 125, 28);
				not.ContentRectangle = new Rectangle(75, 92, 215, 55);
				not.TitleClick += new EventHandler(TitleClick);
				not.ContentClick += new EventHandler(ContentClick);
				not.CloseClick += new EventHandler(CloseClick);
				not.CloseClickable = true;
				not.TitleClickable = false;
				not.ContentClickable = false;
				not.KeepVisibleOnMousOver = true;
				not.ReShowOnMouseOver = true;

				string result = Common.VersionCheck();
				if (!string.IsNullOrEmpty(result))
				{
					string current = Config.GetStringValueFromConfigByKey(@"SharedCacheVersionNumber");
					if(current.Equals(result))
						not.Show("Version Check", "Latest version is used: " + result, 250, 15000, 500);					
					else
						not.Show("Version Check", "New version is available: " + result, 250, 15000, 500);
				}

				//COM.IndexusStatistic stat = COM.Provider.Cache.IndexusDistributionCache.SharedCache.GetStats();

				//if (stat != null)
				//{
				//  not.Show("Shared Cache Status", stat.ToNotify(), 250, 30000, 500);
				//}
				//else
				//{
				//  not.Show("Info", "No Data Available - check if service is running", 500, 3000, 500);
				//}
			}			
		}
		
		#region Events

		#region Events Notify
		/// <summary>
		/// Closes the click.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="ea">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void CloseClick(object obj, EventArgs ea)
		{
			// MessageBox.Show("Closed was Clicked");
			not.Hide();
		}

		/// <summary>
		/// Titles the click.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="ea">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void TitleClick(object obj, EventArgs ea)
		{
			// MessageBox.Show("Title was Clicked");
		}

		/// <summary>
		/// Contents the click.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="ea">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void ContentClick(object obj, EventArgs ea)
		{
			// MessageBox.Show("Content was Clicked");
		}

		#endregion Events Notify

		#region Events Menu Strip

		#endregion Events Menu Strip

		/// <summary>
		/// Handles the Click event of the Restart menu item.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Restart_Click(object sender, EventArgs e)
		{
			Common.RestartService("127.0.0.1");
		}

		private void NetworkFamily_Click(object sender, EventArgs e)
		{
			this.Network.Show();
		}

		/// <summary>
		/// Handles the Click event of the ServiceController menu item.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ServiceController_Click(object sender, EventArgs e)
		{
			this.ServiceController.Show();
		}

		/// <summary>
		/// Handles the Click event of the Status menu item.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Status_Click(object sender, EventArgs e)
		{
			this.ShowNoty();
		}

		/// <summary>
		/// Handles the Click event of the About menu item.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void About_Click(object sender, EventArgs e)
		{
			this.WinAbout.Show();
		}

		/// <summary>
		/// Handles the Click event of the Exit menu item.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Exit_Click(object sender, EventArgs e)
		{
			this.Close();
			this.Dispose(true);
		}

		#endregion Events

		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			
			SharedCache.Registration.Register reg = new SharedCache.Registration.Register();
			reg.Show();
		}

		public bool GetRegistryValue()
		{   
			try 
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\SharedCache.com", true);
				if (key == null)
					return false;

				return Convert.ToBoolean(key.GetValue("RegisterForSharedCache.com", "False"));
			}
			catch (Exception ex)
			{
				return false;
			}			
		}

		private void checkSharedCacheVersionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.WinCheckServerNodeVersion.Show();
		}

		private void checkUsageOfHashCodeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.WinCheckHashCode.Show();
		}

		private void commonRuntimeLanguageCheckToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.WinCheckServerNodeClr.Show();
		}
		
	}
}