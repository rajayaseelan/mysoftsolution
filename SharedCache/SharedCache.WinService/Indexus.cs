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
// Name:      Indexus.cs
// 
// Created:   21-01-2007 SharedCache.com, rschuetz
// Modified:  21-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated consistency of logging calls
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Serialization;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.WinService
{
		
	partial class Indexus : ServiceBase
	{
		/// <summary>
		/// running thread
		/// </summary>
		private Thread runThread;
		
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		
		#region Singleton: ServiceLogic
		private ServiceLogic serviceLogicInstance;
		/// <summary>
		/// Singleton for <see cref="ServiceLogic" />
		/// </summary>
		public ServiceLogic ServiceLogicInstance
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (serviceLogicInstance == null)
					this.serviceLogicInstance = new ServiceLogic();
		
				return this.serviceLogicInstance;
			}
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Indexus"/> class.
		/// sets the default port to 48888
		/// </summary>
		public Indexus()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			InitializeComponent();
		}

		#region Methods
		/// <summary>
		/// The main entry point for the process
		/// options: 
		///  - Install
		/// 1. /install 
		/// 2. /i
		///  - Uninstall
		/// 1. /uninstall
		/// 2. /u
		/// </summary>
		/// <param name="args">The args. A <see cref="T:System.String[]"/> Object.</param>
		public static void Main(string[] args)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + typeof(Indexus).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			string optionalArgs = string.Empty;
			if (args.Length > 0)
			{
				optionalArgs = args[0];
			}
			#region args Handling
			if (!string.IsNullOrEmpty(optionalArgs))
			{
				if("/?".Equals(optionalArgs.ToLower()))
				{
					Console.WriteLine("Help Menu");
					Console.ReadLine();
					return;
				}
				else if("/local".Equals(optionalArgs.ToLower()))
				{
					Console.Title = "Shared Cache - Server";
					Console.BackgroundColor = ConsoleColor.DarkBlue;
					Console.ForegroundColor = ConsoleColor.White;
					// running as cmd appliacation
					Indexus SrvIndexus = new Indexus();
					SrvIndexus.StartService();
					Console.ReadLine();
					SrvIndexus.StopService();
					return;
				}
				else if(@"/verbose".Equals(optionalArgs.ToLower()))
				{
					// Console.SetOut(this);
					// Console.SetIn(Console.Out);
					// Console.ReadLine();
					return;
				}
				
				TransactedInstaller ti = new TransactedInstaller();
				IndexusInstaller ii = new IndexusInstaller();
				ti.Installers.Add(ii);
				string path = string.Format("/assemblypath={0}", System.Reflection.Assembly.GetExecutingAssembly().Location);
				string[] cmd = { path };
				InstallContext context = new InstallContext(string.Empty, cmd);
				ti.Context = context;

				if ("/install".Equals(optionalArgs.ToLower()) || "/i".Equals(optionalArgs.ToLower()))
				{
					ti.Install(new Hashtable());
				}
				else if ("/uninstall".Equals(optionalArgs.ToLower()) || "/u".Equals(optionalArgs.ToLower()))
				{
					ti.Uninstall(null);
				}
				else
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(@"Your provided Argument is not available." + Environment.NewLine);
					sb.Append(@"Use one of the following once:" + Environment.NewLine);
					sb.AppendFormat(@"To Install the service '{0}': '/install' or '/i'" + Environment.NewLine, @"IndeXus.Net");
					sb.AppendFormat(@"To Un-Install the service'{0}': '/uninstall' or '/u'" + Environment.NewLine, @"IndeXus.Net");
					Console.WriteLine(sb.ToString());
				}
			}
			else
			{
				// nothing received as input argument
				ServiceBase[] servicesToRun;
				servicesToRun = new ServiceBase[] { new Indexus() };
				ServiceBase.Run(servicesToRun);
			}
			#endregion args Handling
		}

		/// <summary>
		/// Starts the service.
		/// </summary>
		private void StartService()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
            COM.Handler.LogHandler.Info("SharedCache.com Service Starting");
			this.ServiceLogicInstance.Init();
            COM.Handler.LogHandler.Info("SharedCache.com Service Started");
		}
		
		/// <summary>
		/// Stops the service.
		/// </summary>
		private void StopService()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			COM.Handler.LogHandler.Info("SharedCache.com Service Stopping" + COM.Enums.LogCategory.ServiceCleanUpStop.ToString());
			this.ServiceLogicInstance.ShutDown();
            COM.Handler.LogHandler.Info("SharedCache.com Service Stopped" + COM.Enums.LogCategory.ServiceCleanUpStop.ToString());
		}

		#endregion Methods

		#region override Methods
		/// <summary>
		/// When implemented in a derived class, executes when a Start command is sent 
		/// to the service by the Service Control Manager (SCM) or when the operating 
		/// system starts (for a service that starts automatically). Specifies actions 
		/// to take when the service starts.
		/// </summary>
		/// <param name="args">data passed by the start command.</param>
		protected override void OnStart(string[] args)
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			runThread = new Thread(new ThreadStart(StartService));
			runThread.Start();
		}

		/// <summary>
		/// When implemented in a derived class, executes when a Stop command is sent to 
		/// the service by the Service Control Manager (SCM). Specifies actions to take 
		/// when a service stops running.
		/// </summary>
		protected override void OnStop()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.StopService();
		}

		/// <summary>
		/// When implemented in a derived class, <see cref="M:System.ServiceProcess.ServiceBase.OnContinue"></see> runs when a Continue command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service resumes normal functioning after being paused.
		/// </summary>
		protected override void OnContinue()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.StopService();
			this.StartService();
			base.OnContinue();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion override Methods

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			#region Access Log
			COM.Handler.LogHandler.Tracking(
				"Access Method: " + this.GetType().ToString()+ "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;"
			);
			#endregion Access Log

			components = new System.ComponentModel.Container();
			this.CanHandlePowerEvent = false;
			this.CanPauseAndContinue = true;
			this.CanShutdown = true;
			// renamed service from to SharedCache.com
			this.ServiceName = "SharedCache";
		}

		#endregion

	}
}
