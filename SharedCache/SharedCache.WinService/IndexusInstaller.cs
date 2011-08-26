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
// Name:      IndexusInstaller.cs
// 
// Created:   21-01-2007 SharedCache.com, rschuetz
// Modified:  21-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  25-12-2007 SharedCache.com, rschuetz : updated installer procedure, no additional SetupHelper Project is needed anymore
// Modified:  25-12-2007 SharedCache.com, rschuetz : checkeout http://www.simple-talk.com/content/print.aspx?article=192 for more information
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Specialized;
using System.Net;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.WinService
{
	[RunInstaller(true)]
	public partial class IndexusInstaller : Installer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:IndexusInstaller"/> class.
		/// </summary>
		public IndexusInstaller()
		{
			ServiceProcessInstaller spi = new ServiceProcessInstaller();

			spi.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			spi.Password = null;
			spi.Username = null;

			ServiceInstaller si = new ServiceInstaller();
			// renamed service from to SharedCache.com
			si.ServiceName = "SharedCache";
			si.Description = "Shared Cache is a high performance distributed caching system which is in nature generic and it intends to reduce database load in every used application. For more information visit http://www.sharedcache.com.";
			si.StartType = ServiceStartMode.Automatic;
			// adding				
			this.Installers.Add(spi);
			this.Installers.Add(si);

			InitializeComponent();
		}

		protected override void OnBeforeUninstall(IDictionary savedState)
		{
			base.OnBeforeUninstall(savedState);

			Process proc = new Process();
			proc.StartInfo.FileName = @"job_stopService.bat";
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			proc.StartInfo.ErrorDialog = true;
			proc.StartInfo.WorkingDirectory = this.Context.Parameters["target"];
			proc.Start();
			proc.WaitForExit();
		}

		protected override void OnAfterInstall(IDictionary savedState)
		{
			base.OnAfterInstall(savedState);
			
			Process proc = new Process();
			proc.StartInfo.FileName = @"job_startService.bat"; // args[0].ToString(); // @"RestartService.cmd";
			// proc.StartInfo.Arguments = @"net start indexus.net";
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal; // ProcessWindowStyle.Hidden;
			proc.StartInfo.ErrorDialog = true;
			proc.StartInfo.WorkingDirectory = this.Context.Parameters["target"];
			proc.Start();
			proc.WaitForExit();
		}
		
		// http://msdn2.microsoft.com/en-us/library/aa984464(VS.71).aspx
		/// <summary>
		/// When overridden in a derived class, performs the installation.
		/// </summary>
		/// <param name="stateSaver">An <see cref="T:System.Collections.IDictionary"></see> used to save information needed to perform a commit, rollback, or uninstall operation.</param>
		/// <exception cref="T:System.ArgumentException">The stateSaver parameter is null. </exception>
		/// <exception cref="T:System.Exception">An exception occurred in the <see cref="E:System.Configuration.Install.Installer.BeforeInstall"></see> event handler of one of the installers in the collection.-or- An exception occurred in the <see cref="E:System.Configuration.Install.Installer.AfterInstall"></see> event handler of one of the installers in the collection. </exception>
		public override void Install(System.Collections.IDictionary stateSaver)
		{
			base.Install(stateSaver);
			
			UpdateConfigFiles(this.Context, this.Context.Parameters["action"], this.Context.Parameters["target"]);
			
			#region commented
			//ArrayList li = new ArrayList();
			//// li.Add(this.Context.Parameters["target"]);

			//this.Context.LogMessage(@"my message");
			
			//StringDictionary myStringDictionary = this.Context.Parameters;

			//foreach (DictionaryEntry de in this.Context.Parameters)
			//{ 
			//  li.Add( string.Format("Key: {0} - Value: {1}", de.Key, de.Value ) );
			//}

			//CreateReadMeFile(li);
			//System.Windows.Forms.MessageBox.Show("Hallo!");
			//string action = this.Context.Parameters["action"];

			//if (action == "install")
			//{
			//  string path = this.Context.Parameters["target"] + "MailServiceWin.exe.config";
			//  XmlDocument doc = new XmlDocument();
			//  doc.Load(path);
			//  XmlNode node = doc.SelectSingleNode("/configuration/appSettings/add[@key=\"RecoveryPath\"]");
			//  node.Attributes["value"].Value = path;
			//  doc.Save(path);
			//}
			#endregion commented
		}

		/// <summary>
		/// When overridden in a derived class, removes an installation.
		/// </summary>
		/// <param name="savedState">An <see cref="T:System.Collections.IDictionary"></see> that contains the state of the computer after the installation was complete.</param>
		/// <exception cref="T:System.ArgumentException">The saved-state <see cref="T:System.Collections.IDictionary"></see> might have been corrupted. </exception>
		/// <exception cref="T:System.Configuration.Install.InstallException">An exception occurred while uninstalling. This exception is ignored and the uninstall continues. However, the application might not be fully uninstalled after the uninstallation completes. </exception>
		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			if ("uninstall".Equals(this.Context.Parameters["action"]))
			{
				if (File.Exists(@"C:\backupLog.txt"))
				{
					File.Delete(@"C:\backupLog.txt");
				}
			}			
		}

		/// <summary>
		/// Updates the config files.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="action">The action.</param>
		/// <param name="target">The target.</param>
		private static void UpdateConfigFiles(InstallContext context, string action, string target)
		{
			ArrayList li = new ArrayList();

			//context.LogMessage("Enter Method UpdateConfigFiles with: " + (string.IsNullOrEmpty(action) ? "action is null or empty" : action));
			//if (!string.IsNullOrEmpty(action) && "install".Equals(action) && !string.IsNullOrEmpty(target))
			//{
			//  DirectoryInfo dir = new DirectoryInfo(target);

			//  foreach (FileInfo fi in dir.GetFiles("*.config"))
			//  {
			//    li.Add(string.Format("Name: {0} - FullName: {1}", fi.Name, fi.FullName));
			//    string path = target + fi.Name;
			//    XmlDocument doc = new XmlDocument();
			//    doc.Load(path);
					
			//    XmlNodeList nodes = doc.SelectNodes("/configuration/replicatedSharedCache/serverSetting/@ServiceCacheIpAddress");
			//    if (nodes != null)
			//    {
			//      nodes[0].Value = COM.Handler.Network.GetFirstIPAddress().ToString();
			//      doc.Save(path);
			//    }
			//  }
			//}

			//context.LogMessage("Exit Method UpdateConfigFiles");

			//CreateTextFileWithAdaptedConfigEntries(li);
		}

		/// <summary>
		/// Creates text file with adapted config entries.
		/// <remarks>
		/// Location is set fix to: C:\backupLog.txt
		/// </remarks>
		/// </summary>
		/// <param name="li">A list of <see cref="string"/> with adapted files</param>
		private static void CreateTextFileWithAdaptedConfigEntries(ArrayList li)
		{
			// create a writer and open the file
			using (TextWriter tw = new StreamWriter(@"C:\backupLog.txt", true))
			{
				tw.WriteLine(DateTime.Now.ToString());
				tw.WriteLine(@"- - - - - - - - - - - - - - - - -");
				if (li == null || li.Count == 0)
				{
					tw.WriteLine(@"	- no config files to adapted");
				}
				foreach (string n in li)
				{
					// write a line of text to the file
					tw.WriteLine(@"	- " + n);
				}
				tw.WriteLine(@"- - - - - - - - - - - - - - - - -");

				// close the stream
				tw.Close();
			}
		}

	}
}