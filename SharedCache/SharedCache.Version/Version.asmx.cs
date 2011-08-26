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
// Name:      Version.cs
// 
// Created:   21-07-2007 SharedCache.com, rschuetz
// Modified:  21-07-2007 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Reflection;
using System.Web.Configuration;
using System.Configuration;

using COM = SharedCache.WinServiceCommon;
using System.EnterpriseServices;

namespace SharedCache.Version
{
	/// <summary>
	/// Summary description for Service1
	/// </summary>
	[WebService(Namespace = "http://sharedcache.indeXus.Net/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	public class Version : System.Web.Services.WebService
	{
		[WebMethod]
		[COM.Attributes.SharedCacheSoapExtension(CacheInSecond=60000)]
		public string HelloWorld()
		{
			
			return DateTime.Now.ToString("dd.MM.yyyy - hh:mm:ss");			
		}

		/// <summary>
		/// Gets the version of the public available release.
		/// </summary>
		/// <returns></returns>
		[WebMethod]
		[COM.Attributes.SharedCacheSoapExtension(CacheInSecond=60000)]
		public string GetVersion()
		{
			COM.Handler.LogHandler.Info("****************************************************** " + DateTime.Now.ToString() + " ********************************************************");

			COM.Handler.LogHandler.Info("Assembly Info:");
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

			COM.Handler.LogHandler.Info("Client Info:");
			COM.Handler.LogHandler.Info("*************************************************");
			if (this.Context != null)
			{
				if (this.Context.Request != null)
				{
					if (this.Context.Request.UserLanguages != null)
					{
						foreach (string s in this.Context.Request.UserLanguages)
						{
							COM.Handler.LogHandler.Info(s);
						}
					}
					if (this.Context.Request.UserHostName != null)
						COM.Handler.LogHandler.Info(this.Context.Request.UserHostName);
					if (this.Context.Request.UserHostAddress != null)
						COM.Handler.LogHandler.Info(this.Context.Request.UserHostAddress);
					if (this.Context.Request.UserAgent != null)
						COM.Handler.LogHandler.Info(this.Context.Request.UserAgent);
					if (this.Context.Request.UrlReferrer != null)
						COM.Handler.LogHandler.Info(this.Context.Request.UrlReferrer.ToString());
					if (this.Context.Request.ServerVariables != null)
					{
						COM.Handler.LogHandler.Info("ServerVariables:");
						COM.Handler.LogHandler.Info("*************************************************");
						foreach (string s in this.Context.Request.ServerVariables.AllKeys)
						{
							COM.Handler.LogHandler.Info(string.Format("   {0,-10} {1}", s, this.Context.Request.ServerVariables[s]));
						}

					}
					if (this.Context.Request.Headers != null)
					{
						COM.Handler.LogHandler.Info("Headers:");
						COM.Handler.LogHandler.Info("*************************************************");
						foreach (string s in this.Context.Request.Headers.AllKeys)
						{
							COM.Handler.LogHandler.Info(string.Format("   {0,-10} {1}", s, this.Context.Request.Headers[s]));
						}
					}
				}
			}


			return Config.GetStringValueFromConfigByKey("SharedCacheVersionNumber") + " - " + DateTime.Now.ToString("hh:mm:ss");

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

		///// <summary>
		///// Displays the app.config settings.
		///// </summary>
		///// <returns></returns>
		//public static string DisplayAppSettings()
		//{
		//  StringBuilder sb = new StringBuilder();
		//  NameValueCollection appSettings = ConfigurationManager.AppSettings;
		//  string[] keys = appSettings.AllKeys;
		//  sb.Append(string.Empty + Environment.NewLine);
		//  sb.Append("Application appSettings:" + Environment.NewLine);
		//  sb.Append(string.Empty + Environment.NewLine);
		//  // Loop to get key/value pairs.
		//  for (int i = 0; i < appSettings.Count; i++)
		//  {
		//    sb.AppendFormat("#{0} Name: {1} - Value: {2}" + Environment.NewLine, i, keys[i], appSettings[i]);
		//  }
		//  return sb.ToString();
		//}
		#endregion
	}
}
