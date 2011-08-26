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
// Name:      ServiceLogic.cs
// 
// Created:   27-01-2007 SharedCache.com, rschuetz
// Modified:  27-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  09-04-2007 SharedCache.com, rschuetz : distribute object over network per UDP
// Modified:  16-12-2007 SharedCache.com, rschuetz : added regions and updatd comments
// Modified:  23-12-2007 SharedCache.com, rschuetz : added an instance of COM.CacheCleanup, this is used for cleanups
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated consistency of logging calls
// Modified:  31-12-2007 SharedCache.com, rschuetz : removed everything which was depend on UDP.
// Modified:  06-01-2008 SharedCache.com, rschuetz : enabled configuration for Thread-Pool threads.
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.IO;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.WinService
{
	/// <summary>
	/// <b>Contains all needed Shared Service Logic</b>
	/// </summary>
	public class ServiceLogic
	{
		#region Variables
		/// <summary>
		/// Populates a list with installations of SharedCache
		/// </summary>
		public static ArrayList ServerFamily = new ArrayList();
		/// <summary>
		/// Populats the options to replicate data over network
		/// </summary>
		public static TcpServerReplication NetworkDistribution = new TcpServerReplication();
		/// <summary>
		/// Worker thread for TCP handling
		/// </summary>
		Thread workerTcp = null;
		/// <summary>
		/// Worker thread for cache expiration handling
		/// </summary>
		Thread workerCacheExpire = null;
		/// <summary>
		/// an instance of <see cref="TcpServer"/>
		/// </summary>
		TcpServer tcpInstance = null;
		/// <summary>
		/// an instance of <see cref="CacheExpire"/>
		/// </summary>
		CacheExpire cacheExpireInstance = null;
		/// <summary>
		/// reading Family mode from config file.
		/// </summary>
		private bool enableServiceFamilyMode = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceFamilyMode == 1 ? true : false;
		#endregion Variables

		#region Ctor
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ServiceLogic"/> class.
		/// </summary>
		public ServiceLogic()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			AssemblyInfo ai = new AssemblyInfo();
			COM.Handler.LogHandler.Force(@"= = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =");
			COM.Handler.LogHandler.Force("AsmFQName: " + ai.AsmFQName);
			COM.Handler.LogHandler.Force("AsmName: " + ai.AsmName);
			COM.Handler.LogHandler.Force("CodeBase: " + ai.CodeBase);
			COM.Handler.LogHandler.Force("Company: " + ai.Company);
			COM.Handler.LogHandler.Force("Copyright: " + ai.Copyright);
			COM.Handler.LogHandler.Force("Description: " + ai.Description);
			COM.Handler.LogHandler.Force("Product: " + ai.Product);
			COM.Handler.LogHandler.Force("Title: " + ai.Title);
			COM.Handler.LogHandler.Force("Version: " + ai.Version);
		}
		#endregion Ctor

		#region Dtor
		/// <summary>
		/// Shutdown Server.
		/// </summary>
		public void ShutDown()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			COM.Handler.LogHandler.Info("Abort Thread Tcp");
			COM.Handler.LogHandler.Info("Abort Thread Timer UDP Broadcast and family searcher");
			COM.Handler.LogHandler.Info("Abort Thread CacheExpire");			
			
			// do whatever necessary to shutdown
			this.cacheExpireInstance.Dispose();			
			this.tcpInstance.Dispose();
			
			COM.Handler.LogHandler.Info("CleanUp Thread aborted!" + COM.Enums.LogCategory.ServiceCleanUpStop.ToString());
		}
		#endregion Dtor

		private void PrintSettings()
		{
			try
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendFormat(@"CacheAmountFillFactorInPercentage: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountFillFactorInPercentage);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"CacheAmountOfObjects: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"LoggingEnable: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"ServiceCacheCleanup: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanup);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"ServiceCacheCleanupThreadJob: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanupThreadJob);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"ServiceCacheIpAddress: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheIpAddress);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"ServiceCacheIpPort: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheIpPort);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"ServiceFamilyMode: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceFamilyMode);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"SharedCacheVersionNumber: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.SharedCacheVersionNumber);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"TcpServerMaxThreadToSet: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.TcpServerMaxThreadToSet);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"TcpServerMinThreadToSet: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.TcpServerMinThreadToSet);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"SocketPoolTimeout: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.SocketPoolTimeout);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"SocketPoolValidationInterval: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.SocketPoolValidationInterval);
				sb.Append(Environment.NewLine);
				sb.AppendFormat(@"SocketPoolMinAvailableSize: {0}", COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.SocketPoolMinAvailableSize);
#if DEBUG
				Console.WriteLine(sb.ToString());
#endif
				COM.Handler.LogHandler.Info(sb.ToString());
			}
			catch (Exception ex)
			{
				COM.Handler.LogHandler.Info("Could not write PrintSettings, Exception: " + ex.Message + Environment.NewLine + ex.StackTrace);
			}
			
		}

		#region Methods
		/// <summary>
		/// Inits this instance. This method used at startup to initialize 
		/// all required server components
		/// </summary>
		public void Init()
		{
			#region Access Log
			#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log
			
			COM.Handler.LogHandler.Force("Initializing Settings" + COM.Enums.LogCategory.ServiceStart.ToString());
#if DEBUG			
			Console.WriteLine(@"Shared Cache Configuration");
			Console.WriteLine();
#endif
			this.PrintSettings();
			
			// needs to be instantiated before TCP, it needs an instance of CachExpire
			cacheExpireInstance = new CacheExpire();
			
			// TCP needs an instance of CacheExpire
			tcpInstance = new TcpServer(cacheExpireInstance);

			COM.Handler.LogHandler.Force("Init and Start Thread Tcp");
			COM.Handler.LogHandler.Force("Init and Start Thread CacheExpire");
			
			// Init all extenders
			// an extender is a class which initializes its own logic around 
			// specific issue within its own thread;
			///////////////////////////////////////////////////////////////////
			this.workerTcp = new Thread(this.tcpInstance.Init);
			this.workerTcp.Name = "TCP Handler";
			this.workerTcp.IsBackground = true;
			this.workerTcp.Priority = ThreadPriority.Normal;
			///////////////////////////////////////////////////////////////////
			this.workerCacheExpire = new Thread(this.cacheExpireInstance.Init);
			this.workerCacheExpire.Name = "Cache Expire Handler";
			this.workerCacheExpire.IsBackground = true;
			this.workerCacheExpire.Priority = ThreadPriority.Lowest;
			///////////////////////////////////////////////////////////////////
			this.workerCacheExpire.Start();
			this.workerTcp.Start();

			// enable the search of replicaiton servers
			if (this.enableServiceFamilyMode)
			{
				NetworkDistribution.Init();
			}

			string msgThreadInfo = Environment.NewLine + 
				"Main Thread Id: " + Thread.CurrentThread.ManagedThreadId.ToString() + Environment.NewLine +
				"this.workerTcp: " + this.workerTcp.ManagedThreadId.ToString() + Environment.NewLine +
				/*"this.workerTimer: " + this.workerTimer.ManagedThreadId.ToString() + Environment.NewLine +*/
				"this.workerCacheExpire: " + this.workerCacheExpire.ManagedThreadId.ToString();

#if DEBUG
			Console.WriteLine(msgThreadInfo + Environment.NewLine);
			Console.WriteLine("+ + + + + + + + + + + + + + + + + + + + + + + + + + + + ");
			Console.WriteLine("server is ready to receive data.");
#endif

			COM.Handler.LogHandler.Force("Service Started " + COM.Enums.LogCategory.ServiceStart.ToString());
		}
		#endregion Methods
	}
}
