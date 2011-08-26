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
using System.Threading;
using System.Windows.Forms;
using System.ServiceProcess;

namespace SharedCache.Notify
{
	/// <summary>
	/// <b>Maintain Windows Service, possibility to start / re-start / stop shared cache windows service</b>
	/// </summary>
	public partial class WinServiceController : Form
	{

		private ServiceControllerStatus actualStatus;
		private Cursor cursor;
		
		#region Property: IndeXusNetService
		private ServiceController indeXusNetService;
		
		/// <summary>
		/// Gets the IndeXusNetService
		/// </summary>
		public ServiceController IndeXusNetService
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  {
				if (this.indeXusNetService == null)
				{
					this.indeXusNetService = new ServiceController("SharedCache", Environment.MachineName);	
				}
				return this.indeXusNetService;  
			}
		}
		#endregion
		#region CTor
		/// <summary>
		/// Initializes a new instance of the <see cref="WinServiceController"/> class.
		/// </summary>
		public WinServiceController()
		{
			InitializeComponent();

			try
			{
				// if service is not installed, this call forces an exception.
				string n = this.IndeXusNetService.Status.ToString();
				
				this.LblServiceNotAvailable.Visible = false;
				this.BtnStart.Enabled = false;
				this.BtnStop.Enabled = false;

				this.Display();
			}
			catch (InvalidOperationException ex)
			{
				// disable buttons to start / restart / stop
				this.GrpBoxServiceAvailable.Visible = false;

				// inform customer that no service is installed on this machine.
				this.LblServiceNotAvailable.Visible = true;
				this.LblServiceNotAvailable.Top = 13;
				this.LblServiceNotAvailable.Left = 13;				
				this.LblServiceNotAvailable.Text = "SharedCache.com Windows Service is not installed on this Server: " + Environment.MachineName.ToString();
			}
		}
		#endregion CTor

		/// <summary>
		/// Displays this instance.
		/// </summary>
		private void Display()
		{
			this.BtnStop.Enabled = (this.IndeXusNetService.Status == ServiceControllerStatus.Running) && (this.IndeXusNetService.CanStop == true);
			this.BtnStart.Enabled = (this.IndeXusNetService.Status == ServiceControllerStatus.Stopped);

			this.LblStatus.Text = this.IndeXusNetService.Status.ToString();
		}

		/// <summary>
		/// UI Delegate to update Service Status Text
		/// </summary>
		private delegate void UpdateStatusLabelDelegate(string text);
		/// <summary>
		/// Updates the status label, if InvokeRequired is enabled, its
		/// uses an delegate call
		/// </summary>
		/// <param name="text">The text, as new service status</param>
		private void UpdateStatusLabel(string text)
		{
			if (this.InvokeRequired)
			{
				UpdateStatusLabelDelegate utvd = new UpdateStatusLabelDelegate(this.UpdateStatusLabel);
				this.Invoke(utvd, new object[] { text });
			}
			else
			{
				this.LblStatus.Text = text;
			}
		}

		/// <summary>
		/// Waiting for service status changes
		/// </summary>
		/// <param name="status">The status.</param>
		private void UpdateStatus(ServiceControllerStatus status)
		{
			// it will wait until the status has been changed.
			this.IndeXusNetService.WaitForStatus(status);
			// update text on UI
			this.LblStatus.Text = this.IndeXusNetService.Status.ToString();
		}

		#region Cursor Handling
		/// <summary>
		/// Set Wait Cursor on begin of an action
		/// </summary>
		private void StartSwitchCurser()
		{
			cursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
		}

		/// <summary>
		/// Set previous cursor on end of an action
		/// </summary>
		private void EndSwitchCurser()
		{
			Cursor.Current = cursor;
		}
		#endregion Cursor

		#region User Actions / Event Handling

		/// <summary>
		/// Handles the Click event of the BtnStart control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BtnStart_Click(object sender, EventArgs e)
		{
			this.StartSwitchCurser();
			actualStatus = this.IndeXusNetService.Status;
			this.IndeXusNetService.Start();
			this.UpdateStatus(ServiceControllerStatus.Running);
			this.Display();
			this.EndSwitchCurser();
		}

		/// <summary>
		/// Handles the Click event of the BtnStop control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BtnStop_Click(object sender, EventArgs e)
		{
			this.StartSwitchCurser();
			actualStatus = this.IndeXusNetService.Status;
			this.IndeXusNetService.Stop();
			this.UpdateStatus(ServiceControllerStatus.Stopped);
			this.Display();
			this.EndSwitchCurser();
		}

		/// <summary>
		/// Handles the Click event of the BtnCloseWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BtnCloseWindow_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		#endregion 
	}
}