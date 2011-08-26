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

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.Notify
{
	/// <summary>
	/// <b>About window</b>
	/// </summary>
	public partial class About : Form
	{
		CustomUIControls.TaskbarNotifier not = new CustomUIControls.TaskbarNotifier();
		/// <summary>
		/// Initializes a new instance of the <see cref="About"/> class.
		/// </summary>
		public About()
		{

			InitializeComponent();
			this.ShowInTaskbar = true;
			
			this.copyright.Text = string.Format("Copyright {0} Roni Schuetz, Switzerland; Use at your own risk!", DateTime.Now.Year.ToString());
			string latestVersionNumber = "Could not get Version Number, try again later.";
			
			
			this.installedVersionNumber.Text = Config.GetStringValueFromConfigByKey(@"SharedCacheVersionNumber");
			this.VersionNumber.Text = latestVersionNumber;


			string result = Common.VersionCheck();
			if (!string.IsNullOrEmpty(result))
			{
				this.VersionNumber.Text = result;
				this.ShowNoty("Version Check", "New version is available - " + result);
			}
			
			this.Show();
		}

		/// <summary>
		/// Shows the notify window.
		/// </summary>
		private void ShowNoty(string title, string msg)
		{
			not.Hide();
			not.SetBackgroundBitmap(Resource.skin3, Color.FromArgb(255, 0, 255));
			//not.SetBackgroundBitmap(Resource.Copy_of_skin2, Color.FromArgb(255, 0, 255));			
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

			not.Show(title, msg, 250, 30000, 500);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void ManageLink(string url, bool isEmail)
		{
			if(!string.IsNullOrEmpty(url))
			{
				string tmp = (isEmail == true ? "mailto:" : "") + url;
				System.Diagnostics.Process.Start(tmp);
			}
			
		}

		private void About_Load(object sender, EventArgs e)
		{
			
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.ManageLink(((System.Windows.Forms.LinkLabel)(sender)).Text, false);
		}

		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.ManageLink(((System.Windows.Forms.LinkLabel)(sender)).Text, true);
		}

		private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.ManageLink(((System.Windows.Forms.LinkLabel)(sender)).Text, false);
		}

		private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.ManageLink(((System.Windows.Forms.LinkLabel)(sender)).Text, false);
		}

		private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.ManageLink(((System.Windows.Forms.LinkLabel)(sender)).Text, false);
		}

		private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.ManageLink(((System.Windows.Forms.LinkLabel)(sender)).Text, false);
		}

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

	}
}