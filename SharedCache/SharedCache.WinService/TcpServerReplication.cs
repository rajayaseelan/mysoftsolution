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
// Name:      TcpServerReplication.cs
// 
// Created:   24-09-2007 SharedCache.com, rschuetz
// Modified:  24-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated consistency of logging calls
// Modified:  31-12-2007 SharedCache.com, rschuetz : make usage of: COM.Ports.PortTcp() instead of a direct call.
// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - protocol changed - added string key and byte[] payload instead KeyValuePair
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
	/// While Familiy Mode is enabled this Thread task is responsible to 
	/// replicate data to other servers within the familiy.
	/// </summary>
	public class TcpServerReplication : COM.Extenders.IInit
	{

		#region Properties
		/// <summary>
		/// needed for lock(){} operations 
		/// </summary>
		private object bulkObject = new object();
		/// <summary>
		/// A Queue of type <see cref="COM.IndexusMessage"/> which is responsible to replicate data
		/// to differnt servers.
		/// </summary>
		private Queue<COM.IndexusMessage> replicationQueue;
		/// <summary>
		/// reading Family mode from config file.
		/// </summary>
		private bool enableServiceFamilyMode = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceFamilyMode == 1 ? true : false;
		/// <summary>
		/// reading IP Address from config file.
		/// </summary>
		private string cacheIpAdress = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheIpAddress;
		/// <summary>
		/// reading Tcp Port from config file.
		/// </summary>
		private int cacheIpPort = COM.Ports.ServerDefaultPortTcp();
		#region Property: ReplicationEnabled
		private bool replicationEnabled = false;

		/// <summary>
		/// Gets/sets the ReplicationEnabled
		/// </summary>
		public bool ReplicationEnabled
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.replicationEnabled; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.replicationEnabled = value; }
		}
		#endregion

		COM.Threading.SharedCacheThreadPool threadPool;
		delegate void DistributeMessageToFamilyDelegate(object state);
		private ManualResetEvent allDone = new ManualResetEvent(false);
		Dictionary<long, ResendControlState> resendCount = new Dictionary<long, ResendControlState>();
		private int maxResend = 5;

		class ResendControlState
		{
			public ResendControlState(int count)
			{
				#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
				#endregion Access Log

				this.count = count;
			}

			public DateTime lastTry;
			public int count = 0;
		}

		#endregion Properties

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="TcpServerReplication"/> class.
		/// </summary>
		public TcpServerReplication()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Thread.CurrentThread.Name = "TcpServerReplication";
			this.replicationQueue = new Queue<SharedCache.WinServiceCommon.IndexusMessage>(10000);

			this.threadPool = new COM.Threading.SharedCacheThreadPool(3, 25, "TcpReplication");

			if (this.enableServiceFamilyMode)
			{
				foreach (string n in COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.Servers)
				{
					string msg = "Defined server by provider: " + n;
#if DEBUG
					Console.WriteLine(msg);
#endif
					COM.Handler.LogHandler.Info(msg);
				}

				Console.WriteLine(@"- - - - - - - - -");
				Console.WriteLine();
			}
		}
		#endregion Constructor

		#region Public Methods
		/// <summary>
		/// Replicates the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		public void Replicate(COM.IndexusMessage msg)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			try
			{
				lock (bulkObject)
				{
					this.replicationQueue.Enqueue(msg);
				}
				if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
				{
					COM.Handler.LogHandler.SyncInfo(
							string.Format(COM.Constants.SYNCENQUEUELOG,
								msg.Id,
								msg.Action,
								msg.Status
							)
						);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				COM.Handler.LogHandler.Error(ex);
			}
		}
		#region IInit Members

		/// <summary>
		/// Exposes the name of current thread which is running and it's ID
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return string.Format(@"Thread Id: {0}; Name: {0}; ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name);
			}
		}

		/// <summary>
		/// Inits this instance, precondition to init the instance is that the 
		/// configuration mode of: ServiceFamilyMode if this is set to 1 it searches
		/// the provider for configured instances. 
		/// <remarks>
		/// if ServiceFamilyMode is off [0] it does not search for configured providers
		/// while if ServiceFamilyMode is on [1] loading the configured data within 
		/// the provider section: replicatedSharedCache
		/// </remarks>
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

			if (this.enableServiceFamilyMode)
			{
				foreach (string s in COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.Servers)
				{
					// validate server does not add itself.
					if (COM.Handler.Network.EvaluateLocalIPAddress(s))
					{
						ServiceLogic.ServerFamily.Add(s);
					}
				}
				if (ServiceLogic.ServerFamily.Count > 0)
				{
					this.replicationEnabled = true;
				}
				else
				{
					COM.Handler.LogHandler.Force("ServerFamily Count is 0!! Replication has been disabled automatically by system.");
					this.replicationEnabled = false;
				}
			}

			if (this.replicationEnabled)
			{
				// start replication threadpool
				this.threadPool.Priority = ThreadPriority.Normal;
				this.threadPool.NewThreadTrigger = 1;
				this.threadPool.DynamicThreadDecay = 5000;
				this.threadPool.Start();

				Thread t = new Thread(this.ProcessQueue);
				t.Start();

				// import data from parent nodes upon startup
				Thread tt = new Thread(this.PollDataFromReplicatedServers);
				tt.Start();

			}
		}

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		public void Dispose()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			this.replicationQueue.Clear();
		}

		#endregion
		#endregion Public Methods

		#region Private Methods

		private void ProcessQueue()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			DistributeMessageToFamilyDelegate callback = null;
			int cnt = 0;
			do
			{
				Thread.Sleep(25);

				do
				{
					this.allDone.Reset();
					cnt = this.replicationQueue.Count;

					if (cnt > 0)
					{
						#region Handle paralell replication logic

						COM.IndexusMessage msg = null;
						long id = -1;

						#region enqueue it here and send msg to distribute as argument
						lock (bulkObject)
						{
							msg = this.replicationQueue.Dequeue();
						}
						#endregion

						if (msg != null)
						{
							if (!resendCount.ContainsKey(msg.Id))
							{
								resendCount.Add(msg.Id, new ResendControlState(0));
							}
							else
							{
								ResendControlState state = null;
								lock (bulkObject)
								{
									state = resendCount[msg.Id];
								}
								if (state != null)
								{
									TimeSpan sp = DateTime.Now.Subtract(state.lastTry);
									if (sp.TotalMilliseconds < 250)
									{
										// Console.WriteLine("Not ready to resend!!! wait more time!!");
										lock (bulkObject)
										{
											this.replicationQueue.Enqueue(msg);
											break;
										}
									}
								}
							}

							if (ServiceLogic.ServerFamily != null && ServiceLogic.ServerFamily.Count > 0)
							{
								// Loop through all available server
								foreach (string host in ServiceLogic.ServerFamily)
								{
									try
									{
										#region replicate to server nodes / familiy members
										if (msg.Status == SharedCache.WinServiceCommon.IndexusMessage.StatusValue.ReplicationRequest)
										{
											msg.ClientContext = false;
											msg.Hostname = host;

											callback = new DistributeMessageToFamilyDelegate(this.DistributeMessageToFamily);
											object[] args = new object[1] { msg };
											this.threadPool.PostRequest(callback, args);
										}
										#endregion
									}
									catch (Exception ex)
									{
										// ???
									}
								}
							}
							else
							{
								#region
								string msgNoServerAvailable = string.Format("could not call object distribution - Family Mode: {0}; Configured installation amount within network: {1}; IP:{2}:{3};", this.enableServiceFamilyMode, ServiceLogic.ServerFamily.Count, this.cacheIpAdress, this.cacheIpPort);
								COM.Handler.LogHandler.Error(msgNoServerAvailable);
#if DEBUG
								Console.WriteLine(msgNoServerAvailable);
#endif
								#endregion
							}
						}
						#endregion

						this.allDone.WaitOne();
					}
				} while (cnt > 0);
			} while (true);
		}
		
		/// <summary>
		/// Distributes the message.
		/// <remarks>
		/// handling requested action of the object within the network. This been executed within its 
		/// own thread and not with a delegate, delegates get blocked in previous implementations.
		/// </remarks>
		/// </summary>
		/// <param name="state">The MSG. A <see cref="T:SharedCache.WinServiceCommon.IndexusMessage"/> Object.</param>
		private void DistributeMessageToFamily(object state)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			COM.IndexusMessage msg = state as COM.IndexusMessage;

			// signal main thread to continue
			this.allDone.Set();

			#region Logging
			if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
			{
				COM.Handler.LogHandler.SyncInfo(
						string.Format(
							COM.Constants.SYNCDEQUEUELOG,
							msg.Id,
							msg.Action,
							msg.Status
						)
					);
			}
			#endregion Logging

			try
			{
				switch (msg.Action)
				{
					case COM.IndexusMessage.ActionValue.Add:
					case COM.IndexusMessage.ActionValue.Remove:
						{
							COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.Distribute(msg);
							break;
						}
				}
			}
			catch (Exception ex)
			{
				#region exception logic
				string errorMsg = @"Distribution were not successfully. " + ex.Message;
				COM.Handler.LogHandler.Error(errorMsg);
				COM.Handler.LogHandler.SyncInfo(errorMsg);
				Console.WriteLine(errorMsg);

				lock (bulkObject)
				{
					int counter = 0;
					if (resendCount.ContainsKey(msg.Id))
					{
						// first increment and then assign.
						counter = ++resendCount[msg.Id].count;
						resendCount[msg.Id].lastTry = DateTime.Now;
					}
					if (counter <= maxResend)
					{
						// reassign msg to queue
						this.replicationQueue.Enqueue(msg);
					}
					else
					{
						// after x-times to try to resend we remove it from the loop otherwise we run into an endless loop
						resendCount.Remove(msg.Id);
						COM.Handler.LogHandler.SyncFatalException(string.Format(@"The message {0} could not be replicated after {1} tries", msg.Id, maxResend), ex);
						#region Logging
#if DEBUG
						Console.WriteLine(@"The message ID - {0} could not be replicated after {1} tries", msg.Id, maxResend);
#endif
						#endregion
					}
				}
				#endregion exception logic
			}


		}

		private void PollDataFromReplicatedServers()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Dictionary<string, List<string>> dataToImport = new Dictionary<string, List<string>>();
			List<string> consolidatedData = new List<string>();

			foreach (string s in COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.Servers)
			{
				// validate server does not add itself.
				if (COM.Handler.Network.EvaluateLocalIPAddress(s))
				{
					if (COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.Ping(s))
					{
						List<string> parentServerNodeKeyList = COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.GetAllKeys(s);
						if (parentServerNodeKeyList != null)
						{
							dataToImport.Add(s, parentServerNodeKeyList);
						}
					}
					else
					{
						if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
						{ 
							COM.Handler.LogHandler.Error(string.Format("Configured replicated server is not available: {0}", s));
						}
						#if DEBUG
						Console.WriteLine("Configured replicated server is not available: {0}", s);
						#endif
					}
				}
			}
			
			foreach (KeyValuePair<string, List<string>> item in dataToImport)
			{
				#if DEBUG
				Console.WriteLine(item.Key.ToString() + " contains " + item.Value.Count + " items to import!");
				#endif

				List<string> keys = item.Value;
				foreach(string n in keys)
				{
					if (!consolidatedData.Contains(n))
					{
						consolidatedData.Add(n);
					}
				}
			}

			foreach (string s in COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.Servers)
			{
				IDictionary<string, byte[]> data = COM.Provider.Server.IndexusServerReplicationCache.CurrentProvider.MultiGet(consolidatedData, s);
				if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
				{
					if(data != null)
					{
						COM.Handler.LogHandler.Info(string.Format("importing {0} keys from server node {1} ", data.Count, s));
					}					
				}
				// adding each key / value pair to local cache
				foreach(KeyValuePair<string, byte[]> item in data)
				{
					// no need to check expiration because the item will removed on the other instance
					TcpServer.LocalCache.Add(item.Key, item.Value);					
				}				
			}
		}

		#endregion Private Methods

	}
}
