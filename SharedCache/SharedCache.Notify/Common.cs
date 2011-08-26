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

// *************************************************************************
//
// Name:      Common.cs
// 
// Created:   18-07-2007 SharedCache.com, rschuetz
// Modified:  18-07-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : added config method since its not available anymore in SharedCache.WinServiceCommon
// ************************************************************************* 

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
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.Notify
{
	/// <summary>
	/// <b>common library</b>
	/// </summary>
	public class Common
	{
		public class ComboBoxItem
		{
			public string Name;
			public int Value;
			public ComboBoxItem(string Name, int Value)
			{
				this.Name = Name;
				this.Value = Value;
			}

			public override string ToString()
			{
				return this.Name;
			}
		}

		/// <summary>
		/// Restarts the service.
		/// </summary>
		/// <param name="service">The service.</param>
		public static void RestartService(string service)
		{
			if(string.IsNullOrEmpty(service))
			{
				MessageBox.Show("Service IP address is missing!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
			
			string vars = " " + service;
			Process proc = new Process();
			proc.StartInfo.FileName = @"RestartService.cmd";
			proc.StartInfo.Arguments = vars;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			proc.StartInfo.ErrorDialog = false;
			proc.StartInfo.WorkingDirectory = path;
			proc.Start();
			proc.WaitForExit();
			if (proc.ExitCode != 0)
				MessageBox.Show("Error executing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}


		public static string VersionCheck()
		{
			version.Version wsVersion;
			string result = string.Empty;
			bool getWsInfo = false;
			try
			{
				wsVersion = new version.Version();
				wsVersion.Timeout = 5000;
				wsVersion.Url = Config.GetStringValueFromConfigByKey(@"VersionUrl");
				result = wsVersion.GetVersion();
				getWsInfo = true;
			}
			catch (Exception ex)
			{
				COM.Handler.LogHandler.Error("Could not get online version number", ex);
			}

			if (!string.IsNullOrEmpty(result) && !result.Equals(Config.GetStringValueFromConfigByKey(@"SharedCacheVersionNumber")))
			{
				if (getWsInfo)
				{
					return result;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Summary description for ConfigHandler.
	/// </summary>
	public class Config
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Config"/> class.
		/// </summary>
		public Config()
		{ }

		#region Methods
		/// <summary>
		/// Get the value based on the key from the app.config
		/// </summary>
		/// <param name="key"><see cref="string"/> Key-Name</param>
		/// <returns>string value of a applied key</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static string GetStringValueFromConfigByKey(string key)
		{
			try
			{
				if (ConfigurationManager.AppSettings[key] != null)
					return ConfigurationManager.AppSettings[key];
			}
			catch (FormatException e)
			{
				System.Diagnostics.Debug.WriteLine(((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine + e.ToString());
			}
			catch (ArgumentException e)
			{
				System.Diagnostics.Debug.WriteLine(((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine + e.ToString());
			}
			catch (Exception e)
			{
				// Todo: RSC: add Logging
				System.Diagnostics.Debug.WriteLine(e);
			}

			return string.Empty;
		}

		/// <summary>
		/// Get the value based on the key from the app.config
		/// </summary>
		/// <param name="key"><see cref="string"/> Key-Name</param>
		/// <returns>int value of a applied key</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static int GetIntValueFromConfigByKey(string key)
		{
			try
			{
				if (ConfigurationManager.AppSettings[key] != null)
					return int.Parse(ConfigurationManager.AppSettings[key]);
			}
			catch (FormatException e)
			{
				System.Diagnostics.Debug.WriteLine(((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine + e.ToString());
			}
			catch (ArgumentException e)
			{
				System.Diagnostics.Debug.WriteLine(((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine + e.ToString());
			}
			catch (Exception e)
			{
				// Todo: RSC: add Logging
				System.Diagnostics.Debug.WriteLine(e);
			}
			return -1;
		}

		/// <summary>
		/// Get the value based on the key from the app.config
		/// </summary>
		/// <param name="key"><see cref="string"/> Key-Name</param>
		/// <returns>int value of a applied key</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static double GetDoubleValueFromConfigByKey(string key)
		{
			try
			{
				if (ConfigurationManager.AppSettings[key] != null)
					return double.Parse(ConfigurationManager.AppSettings[key]);
			}
			catch (FormatException e)
			{
				System.Diagnostics.Debug.WriteLine(((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine + e.ToString());
			}
			catch (ArgumentException e)
			{
				System.Diagnostics.Debug.WriteLine(((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine + e.ToString());
			}
			catch (Exception e)
			{
				// Todo: RSC: add Logging
				System.Diagnostics.Debug.WriteLine(e);
			}
			return -1;
		}

		/// <summary>
		/// Gets the num of defined app settings.
		/// </summary>
		/// <returns>A <see cref="T:System.Int32"/> Object.</returns>
		[System.Diagnostics.DebuggerStepThrough]
		public static int GetNumOfDefinedAppSettings()
		{
			return ConfigurationManager.AppSettings.Count;
		}

		/// <summary>
		/// Displays the app.config settings.
		/// </summary>
		/// <returns></returns>
		public static string DisplayAppSettings()
		{
			StringBuilder sb = new StringBuilder();
			NameValueCollection appSettings = ConfigurationManager.AppSettings;
			string[] keys = appSettings.AllKeys;
			sb.Append(string.Empty + Environment.NewLine);
			sb.Append("Application appSettings:" + Environment.NewLine);
			sb.Append(string.Empty + Environment.NewLine);
			// Loop to get key/value pairs.
			for (int i = 0; i < appSettings.Count; i++)
			{
				sb.AppendFormat("#{0} Name: {1} - Value: {2}" + Environment.NewLine, i, keys[i], appSettings[i]);
			}
			return sb.ToString();
		}
		#endregion
	}

}
