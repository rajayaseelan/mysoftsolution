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
// Name:      TcpServer.cs
// 
// Created:   18-07-2007 SharedCache.com, rschuetz
// Modified:  18-07-2007 SharedCache.com, rschuetz : Creation
// Modified:  18-12-2007 SharedCache.com, rschuetz : since SharedCache works internally with byte[] instead of objects almoast all needed code has been adapted
// Modified:  18-12-2007 SharedCache.com, rschuetz : introduction of CacheAmountOfObjects in MB
// Modified:  23-12-2007 SharedCache.com, rschuetz : deleted all checks if value is an byte[]
// Modified:  23-12-2007 SharedCache.com, rschuetz : implemneted logic CacheCleanup
// Modified:  31-12-2007 SharedCache.com, rschuetz : make usage of: COM.Ports.PortTcp() instead of a direct call.
// Modified:  31-12-2007 SharedCache.com, rschuetz : removed option NetworkInstallationsAvailable() and used instead the new boolean ReplicationEnabled on the NetworkDistribution instance
// Modified:  03-01-2008 SharedCache.com, rschuetz : updated server IP endpoint to receive data from IPAddress.Any instead
// Modified:  03-01-2008 SharedCache.com, rschuetz : added new case to delete all cache items at once: CASE COM.IndexusMessage.ActionValue.RemoveAll
// Modified:  06-01-2008 SharedCache.com, rschuetz : make usage of the .net default ThreadPool - ThreadPool.QueueUserWorkItem()
// Modified:  06-01-2008 SharedCache.com, rschuetz : enabled configuration for Thread-Pool threads.
// Modified:  11-02-2008 SharedCache.com, rschuetz : takeover prototype async code [checkout: http://netrsc.blogspot.com/2008/02/threaded-asynchronous-tcp-server-with.html] for more information
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.WinService
{
	/// <summary>
	/// All needed TCP logic upon Win32 start and stop;
	/// </summary>
	public class TcpServer : COM.Extenders.IInit
	{

		#region Cache Handling
		/// <summary>
		/// Provides a local variable of <see cref="SharedCache.WinServiceCommon.Cache"/> do not work directly with this 
		/// instance, use the public property <see cref="LocalCache"/> which ensures the runtime
		/// </summary>
		private static SharedCache.WinServiceCommon.Cache _cache;
		/// <summary>
		/// Gets the local cache.
		/// </summary>
		/// <value>The local cache.</value>
		public static SharedCache.WinServiceCommon.Cache LocalCache
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				EnsureHttpRuntime();
				return _cache;
			}
		}
		/// <summary>
		/// Provides a local variable of <see cref="SharedCache.WinServiceCommon.CacheCleanup"/> do not work directly with this 
		/// instance, use the public property <see cref="CacheCleanup"/> which ensures the runtime
		/// </summary>
		private static SharedCache.WinServiceCommon.CacheCleanup _cacheCleanup;
		/// <summary>
		/// Gets the local cache.
		/// </summary>
		/// <value>The local cache.</value>
		public static SharedCache.WinServiceCommon.CacheCleanup CacheCleanup
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				EnsureCleanupRuntime();
				return _cacheCleanup;
			}
		}

		/// <summary>
		/// Ensures the cleanup runtime.
		/// </summary>
		[System.Diagnostics.DebuggerStepThrough]
		private static void EnsureCleanupRuntime()
		{
			if (null == _cacheCleanup)
			{
				try
				{
					Monitor.Enter(typeof(Indexus));
					if (null == _cacheCleanup)
					{
						// Create an Cache Object which give us access.
						_cacheCleanup = new SharedCache.WinServiceCommon.CacheCleanup();
					}
				}
				finally
				{
					Monitor.Exit(typeof(Indexus));
				}
			}
		}

		/// <summary>
		/// Ensures the HTTP runtime.
		/// </summary>
		[System.Diagnostics.DebuggerStepThrough]
		private static void EnsureHttpRuntime()
		{
			if (null == _cache)
			{
				try
				{
					Monitor.Enter(typeof(Indexus));
					if (null == _cache)
					{
						// Create an Cache Object which give us access.
						_cache = new SharedCache.WinServiceCommon.Cache();
					}
				}
				finally
				{
					Monitor.Exit(typeof(Indexus));
				}
			}
		}
		#endregion Cache Handling

		#region Properties
		/// <summary>
		/// needed for lock(){} operations 
		/// </summary>
		private static object bulkObject = new object();
		/// <summary>
		/// write Statistics
		/// </summary>
		static bool writeStats = false;
		/// <summary>
		/// reading IP Address from config file.
		/// </summary>
		private string cacheIpAdress = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheIpAddress;
		/// <summary>
		/// reading Tcp Port from config file.
		/// </summary>
		private int cacheIpPort = COM.Ports.ServerDefaultPortTcp();
		/// <summary>
		/// a value to set upper thread pool boundry
		/// </summary>
		private int maxThreadToSet = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.TcpServerMaxThreadToSet;
		/// <summary>
		/// a value to set lower thread pool boundry
		/// </summary>
		private int minThreadToSet = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.TcpServerMinThreadToSet;
		/// <summary>
		/// an Instance of <see cref="CacheExpire"/>
		/// </summary>
		private CacheExpire expire = null;
		/// <summary>
		/// The maximum size of the objects in the Cache in MB.
		/// </summary>
		private long cacheAmountOfObjects = -1;
		/// <summary>
		/// If the cache received the fillfactor it will start to throw away unused items from the cache suggested is a value between: 85% - 95%
		/// </summary>
		private long cacheAmountFillFactorInPercentage = 90;
		/// <summary>
		/// Delegate definition to handle received client data and proceed it.
		/// </summary>
		/// <param name="state">A <see cref="COM.Sockets.SharedCacheStateObject"/> object.</param>
		delegate void HandleClientMessageDelegate(COM.Sockets.SharedCacheStateObject state);
		/// <summary>
		/// Define maximum amount of sockets
		/// </summary>
		private int maxSockets;
		/// <summary>
		/// actual amount of sockets
		/// </summary>
		private int socketCount;
		/// <summary>
		/// Timer of type <see cref="Timer"/> to evaluate connected sockets
		/// </summary>
		private Timer lostTimer;
		/// <summary>
		/// Default amount of Threads
		/// </summary>
		private const int numberOfThreads = 1;
		/// <summary>
		/// Used for Timer to evaluate connected clients
		/// </summary>
		private const int timerTimeout = 300000;
		/// <summary>
		/// Used for Timer to evaluate connected clients
		/// </summary>
		private const int timeoutMinutes = 3;
		/// <summary>
		/// Define status if server is in shutdown mode
		/// </summary>
		private bool ShuttingDownServer = false;
		/// <summary>
		/// A <see cref="Hashtable"/> which contains connected clients
		/// </summary>
		private Hashtable conntectedHt = new Hashtable();
		/// <summary>
		/// A container for state objects
		/// </summary>
		private ArrayList conntectedSockets;
		private ManualResetEvent allDone = new ManualResetEvent(false);
		private Thread[] serverThread = new Thread[numberOfThreads];
		private AutoResetEvent[] threadEnd = new AutoResetEvent[numberOfThreads];
		/// <summary>
		/// Custom Thread pool <see cref="COM.Threading.SharedCacheThreadPool"/>
		/// </summary>
		private COM.Threading.SharedCacheThreadPool threadPool;
		/// <summary>
		/// <see cref="WaitCallback"/> after client connected.
		/// </summary>
		private WaitCallback AcceptConnection;
		#endregion Properties

		#region CTor
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Tcp"/> class.
		/// </summary>
		public TcpServer()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			#region validate first if provided configuration is a numeric value - ConfigCacheAmountOfObjects

			if (!string.IsNullOrEmpty(COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects.ToString()) &&
					long.TryParse(
						COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects.ToString(),
						out this.cacheAmountOfObjects) &&
						COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects > 0
				)
			{
				// calculate the correct size for max cache size
				this.cacheAmountOfObjects =
					long.Parse(COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects.ToString())
					* (1024 * 1024);
			}
			else
			{
				this.cacheAmountOfObjects = -1;
			}

			#endregion validate first if provided configuration is a numeric value - ConfigCacheAmountOfObjects

			#region validate first if provided configuration is a numeric value - ConfigCacheAmountFillFactorInPercentage
			if (!string.IsNullOrEmpty(
				COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountFillFactorInPercentage.ToString()) &&
				long.TryParse(
					COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountFillFactorInPercentage.ToString(),
					out this.cacheAmountFillFactorInPercentage)
				)
			{
				// this defines the purge factor for the dictonary
				this.cacheAmountFillFactorInPercentage = long.Parse(
					COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountFillFactorInPercentage.ToString());
			}
			else
			{
				this.cacheAmountFillFactorInPercentage = 90;
			}

			#endregion validate first if provided configuration is a numeric value - ConfigCacheAmountFillFactorInPercentage

			this.maxSockets = 30000;
			this.conntectedSockets = new ArrayList(this.maxSockets);

			#region Thread Pool Configuration
			int minThreadsInPool = 3;
			int maxThreadsInPool = 25;
			// validate min threads
			if (minThreadToSet > minThreadsInPool)
			{
				minThreadsInPool = minThreadToSet;
			}
			// validate max threads
			if (maxThreadToSet > maxThreadsInPool)
			{
				maxThreadsInPool = maxThreadToSet;
			}
			// recheck that min. amount is smaller then max. - set default values if this happend
			if (minThreadsInPool >= maxThreadsInPool)
			{
				minThreadsInPool = 3;
				maxThreadsInPool = 25;
			}
			#endregion Thread Pool Configuration
			
			try
			{
				// Init thread pool 
				this.threadPool = new COM.Threading.SharedCacheThreadPool(minThreadsInPool, maxThreadsInPool, "TcpServer");
				COM.Handler.LogHandler.Info(string.Format("Configuration: Thread Pool initalized with min:{0} and max:{1} Threads", minThreadsInPool, maxThreadsInPool));
				Console.WriteLine("Configuration: Thread Pool initalized with min:{0} and max:{1} Threads", minThreadsInPool, maxThreadsInPool);
			}
			catch (Exception)
			{
				this.threadPool = null;
				COM.Handler.LogHandler.Error("Configuration Error: Thread Pool initalized with min: 3 and max: 25 Threads");
			}
			finally
			{
				if (this.threadPool == null)
				{
					minThreadsInPool = 3;
					maxThreadsInPool = 25;
					this.threadPool = new COM.Threading.SharedCacheThreadPool(minThreadsInPool, maxThreadsInPool, "TcpServer");
					COM.Handler.LogHandler.Error("Configuration Error: Thread Pool initalized with min: 3 and max: 25 Threads");
				}
			}

			this.threadPool.Priority = ThreadPriority.AboveNormal;
			this.threadPool.NewThreadTrigger = 1;
			this.threadPool.DynamicThreadDecay = 5000;
			this.threadPool.Start();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TcpServer"/> class.
		/// </summary>
		/// <param name="expire">The expire.</param>
		public TcpServer(CacheExpire expire)
			: this()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (expire != null && expire.Expire != null && expire.Expire.Enable)
			{
				this.expire = expire;
			}
			else
			{
				this.expire = null;
				expire.Dispose();
				if (expire != null)
					expire = null;
			}
		}
		#endregion CTor

		#region IInit Members
		/// <summary>
		/// Inits this instance.
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

			for (int i = 0; i < numberOfThreads; ++i)
			{
				this.threadEnd[i] = new AutoResetEvent(false);
			}

			ThreadStart serverListener = new ThreadStart(this.StartListening);
			serverThread[0] = new Thread(serverListener);
			serverThread[0].IsBackground = true;
			serverThread[0].Start();

			TimerCallback timerDelegate = new TimerCallback(this.CheckSockets);
			this.lostTimer = new Timer(timerDelegate, null, TcpServer.timerTimeout, TcpServer.timeoutMinutes);

			COM.Handler.LogHandler.Force("Listener Started" + COM.Enums.LogCategory.ServiceStart.ToString());
			Console.WriteLine("Listener Started");
		}
		/// <summary>
		/// Gets the name.
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
			// send shutdown over network if family mode is enabled.
			COM.Handler.LogHandler.Force(string.Format(@"Thread abort's Id: {0}; Name: {0}; ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
			this.Stop();
		}


		/// <summary>
		/// Stops this instance.
		/// </summary>
		public void Stop()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			int lcv;
			lostTimer.Dispose();
			lostTimer = null;

			for (lcv = 0; lcv < numberOfThreads; lcv++)
			{
				if (!serverThread[lcv].IsAlive)
					threadEnd[lcv].Set();	// Set event if thread is already dead
			}
			this.ShuttingDownServer = true;
			try
			{
				// Create a connection to the port to unblock the listener thread
				Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, this.cacheIpPort);
				sock.Connect(endPoint);
				//sock.Close();
				sock = null;
			}
			catch (Exception)
			{
				// do nothing ... 
			}
			// Check thread end events and wait for up to 5 seconds.
			for (lcv = 0; lcv < numberOfThreads; lcv++)
				threadEnd[lcv].WaitOne(500, false);
		}



		#endregion

		#region Async TCP Connection Handling
		#region Private Methods
		private void StartListening()
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, this.cacheIpPort);
			// fix for issue: http://sharedcache.codeplex.com/WorkItem/View.aspx?WorkItemId=9370
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				try
				{
					socket.Bind(endPoint);
					socket.Listen(this.maxSockets);

					string msg = string.Format(@"TCP - Listener started on: {0}:{1}", this.cacheIpAdress, this.cacheIpPort);
					COM.Handler.LogHandler.Force(msg);
					COM.Handler.LogHandler.Traffic(msg);
					Console.WriteLine(msg);

					AcceptConnection = new WaitCallback(AcceptConnection_Handler);

					while (!this.ShuttingDownServer)
					{
						this.allDone.Reset();
						// start async socket listener for connections
						// socket.BeginAccept(new AsyncCallback(this.AccecptCallback), socket);

						this.threadPool.PostRequest(this.AcceptConnection, new object[] { socket });

						//ThreadPool.QueueUserWorkItem(AcceptConnection, socket);
						// wait until the connection is established befor we continue
						this.allDone.WaitOne();
					}
				}
				catch (SocketException sEx)
				{
					#region Socket exception handling
					threadEnd[0].Set();

					COM.Handler.LogHandler.Fatal(string.Format("Could not bind enpoint {0}:{1} - Socket Excpetion No: {2}", endPoint.Address, endPoint.Port, sEx.ErrorCode));
					COM.Handler.LogHandler.Error(string.Format("Could not bind enpoint {0}:{1} - Socket Excpetion No: {2}", endPoint.Address, endPoint.Port, sEx.ErrorCode));
					Console.WriteLine("");
					Console.WriteLine(@"FATAL ERROR:");
					Console.WriteLine("");
					Console.WriteLine("Server cannot start to listen because it could not bind your requested enpoint {0}:{1} - Socket Excpetion No: {2}", endPoint.Address, endPoint.Port, sEx.ErrorCode);
					Console.WriteLine("use the command: 'netstat -a' to evaluate which application takes usage of port: {0}", endPoint.Port);
					#endregion Socket exception handling
				}
				catch (Exception ex)
				{
					#region Regular Exception Handling
					threadEnd[0].Set();
					COM.Handler.LogHandler.Fatal(string.Format("Could not bind enpoint {0}:{1} - Excpetion Message: {2}", endPoint.Address, endPoint.Port, ex.Message));
					COM.Handler.LogHandler.Error(string.Format("Could not bind enpoint {0}:{1} - Excpetion Message: {2}", endPoint.Address, endPoint.Port, ex.Message));
					Console.WriteLine("");
					Console.WriteLine(@"FATAL ERROR:");
					Console.WriteLine("");
					Console.WriteLine("Could not bind enpoint {0}:{1} - Excpetion Message: {2}", endPoint.Address, endPoint.Port, ex.Message);
					#endregion Regular Exception Handling
				}
			}			
		}


		/// <summary>
		/// Accepts connection handler.
		/// </summary>
		/// <param name="s">The a new socket</param>
		private void AcceptConnection_Handler(object s)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Socket socket = s as Socket;
			// start async socket listener for connections
			socket.BeginAccept(new AsyncCallback(this.AccecptCallback), socket);
		}

		/// <summary>
		/// Accecpts the callback.
		/// </summary>
		/// <param name="ar">The ar.</param>
		private void AccecptCallback(IAsyncResult ar)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			// signal main thread to continue
			this.allDone.Set();

			Socket clientListener = ar.AsyncState as Socket;

			if (clientListener != null)
			{
				Socket handler = clientListener.EndAccept(ar);

				// after accepted Client is available create a new state object.
				COM.Sockets.SharedCacheStateObject state = new COM.Sockets.SharedCacheStateObject();

				// assign the socket to the worker sockets
				state.WorkSocket = handler;

				// keep state alive
				state.AliveTimeStamp = DateTime.Now;

				try
				{
					// increment the amount of connected sockets
					Interlocked.Increment(ref this.socketCount);

					if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
					{
#if DEBUG
						Console.WriteLine(@"Connected by client: {0}; current amount of connected clients: {1}", handler.RemoteEndPoint.ToString(), this.socketCount);
#endif
						COM.Handler.LogHandler.Info(string.Format(@"Connected by client: {0}; current amount of connected clients: {1}", handler.RemoteEndPoint.ToString(), this.socketCount));
					}
#if DEBUG
					if (writeStats)
					{
						Console.WriteLine(@"SocketCount: {0}", this.socketCount);
					}
#endif
					lock (this.conntectedSockets)
					{
						this.conntectedSockets.Add(state);
					}

					// ready to start with receiving data.
					handler.BeginReceive(state.Buffer, 0,
						COM.Sockets.SharedCacheStateObject.BufferSize, 0,
						new AsyncCallback(this.ReadCallback), state);

					// if the server has to much connected clients as configured the state will be removed.
					if (this.socketCount > this.maxSockets)
					{
						RemoveSocket(state);
						handler = null;
						state = null;
					}
				}
				catch (SocketException sEx)
				{
					Console.WriteLine("Socket Exception: {0} \n{1}", sEx.ErrorCode, sEx.Message);
					COM.Handler.LogHandler.Error(string.Format("Socket Exception: {0} \n{1}", sEx.ErrorCode, sEx.Message));
					RemoveSocket(state);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception: {0}", ex.Message);
					COM.Handler.LogHandler.Error(string.Format("Exception: {0}", ex.Message));
					RemoveSocket(state);
				}
			}
		}

		/// <summary>
		/// Reads the callback.
		/// </summary>
		/// <param name="ar">The ar.</param>
		private void ReadCallback(IAsyncResult ar)
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			COM.Sockets.SharedCacheStateObject state = ar.AsyncState as COM.Sockets.SharedCacheStateObject;
			if (state != null)
			{
				Socket handler = state.WorkSocket;
				if (handler != null)
				{
					try
					{
						// read data from the client sockets
						int read = handler.EndReceive(ar);
						if (read > 0)
						{
							Monitor.Enter(state);
							// save 2.5 seconds while i copy data to local variables and i do not access the property
							byte[] localBuffer;
							localBuffer = state.Buffer;
							byte[] tmp = new byte[read];
							// copy all buffer data into DataBuffer which is list and contains all data until we get the whole message.
							Array.Copy(localBuffer, tmp, tmp.Length);

							// localList.AddRange(state.DataBuffer);
							// localList.AddRange(tmp);
							if (state.DataBigBuffer == null)
								state.DataBigBuffer = new byte[0];

							byte[] localBuffer1 = state.DataBigBuffer;

							byte[] tmp2 = new byte[tmp.Length + localBuffer1.Length];
							Array.Copy(localBuffer1, tmp2, localBuffer1.Length);
							Array.Copy(tmp, 0, tmp2, localBuffer1.Length, tmp.Length);

							state.DataBigBuffer = tmp2;
							Monitor.Exit(state);


							// check for header
							if (state.ReadHeader)
							{
								// message header are long values for this issue we need to add 8 bytes to length
								state.MessageLength = BitConverter.ToInt64(state.Buffer, 0) + 8;
								// upon next read the header will not be readed again.
								state.ReadHeader = false;
							}
							state.AlreadyRead += read;

							// check for received message length
							if (state.AlreadyRead == state.MessageLength)
							{
								// keep state alive with a new timestamp
								// state.AliveTimeStamp = DateTime.Now;

								// manipulate message - we need to remove the header before we can handle it
								byte[] r = new byte[state.DataBigBuffer.Length - 8];
								Array.Copy(state.DataBigBuffer, 8, r, 0, r.Length);
								state.DataBigBuffer = r;

								// HandleClientMessage(state);

								HandleClientMessageDelegate cb = new HandleClientMessageDelegate(this.HandleClientMessage);
								object[] args = { state };
								this.threadPool.PostRequest(cb, args);
							}
							else
							{
								// Thread.Sleep(1);
								// not all data received ... get more data
								handler.BeginReceive(state.Buffer, 0, COM.Sockets.SharedCacheStateObject.BufferSize, 0,
									new AsyncCallback(this.ReadCallback), state);
							}
						}
						else
						{
							this.RemoveSocket(state);
						}
					}
					catch (SocketException sEx)
					{
						#region
						//Console.WriteLine(
						//  @"Socket Error Code: {0}" + Environment.NewLine +
						//  "Exception Title: {1}" + Environment.NewLine +
						//  "Exception Stacktrace: ", sEx.ErrorCode, sEx.Message, sEx.StackTrace);
						switch (sEx.SocketErrorCode)
						{
							// 10054
							case SocketError.ConnectionReset:
								{
									Console.WriteLine(@"Client Disconnected: {0}; {1}", sEx.ErrorCode, state.WorkSocket.RemoteEndPoint.ToString());
									COM.Handler.LogHandler.Traffic(string.Format(@"Client Disconnected: {0}; {1}", sEx.ErrorCode, state.WorkSocket.RemoteEndPoint.ToString()));
									RemoveSocket(state);
									break;
								}
							default:
								{
									RemoveSocket(state);
									Console.WriteLine(@"Socket Exception {0} Code: {1}", sEx.SocketErrorCode, sEx.ErrorCode);
									break;
								}
						}
						#endregion
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
			}
		}

		/// <summary>
		/// Handles the client message.
		/// </summary>
		/// <param name="state">The state.</param>
		private void HandleClientMessage(COM.Sockets.SharedCacheStateObject state)
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (state != null)
			{
				COM.IndexusMessage msg = null;

				try
				{
					System.IO.MemoryStream stream = new System.IO.MemoryStream(state.DataBigBuffer);
					stream.Seek(0, 0);
					msg = new COM.IndexusMessage(stream);
				}
				catch (Exception ex)
				{
					#region Error Handling
					COM.Handler.LogHandler.Fatal(string.Format("[{0}] Could not create IndexusMessage from received data", state.WorkSocket.RemoteEndPoint.ToString()), ex);
					COM.Handler.LogHandler.Error(string.Format("[{0}] Could not create IndexusMessage from received data", state.WorkSocket.RemoteEndPoint.ToString()), ex);
					COM.Handler.LogHandler.Traffic(string.Format("[{0}] Could not create IndexusMessage from received data", state.WorkSocket.RemoteEndPoint.ToString()));
					Console.WriteLine(string.Format("[{0}] Could not create IndexusMessage from received data", state.WorkSocket.RemoteEndPoint.ToString()));

					using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
					{
						resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
						resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
						resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
							new COM.CacheException("Could not convert MemoryStream to indeXusMessage", msg.Action, msg.Status, ex)
						);

						this.SendResponse(state, resultMsg);
					}
					return;
					#endregion Error Handling
				}

				if (msg != null)
				{
					// check status first [Request || Response]
					switch (msg.Status)
					{
						case COM.IndexusMessage.StatusValue.Request:
						case COM.IndexusMessage.StatusValue.ReplicationRequest:
							{
								try
								{
									#region Logging
#if DEBUG
									if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
									{
										// Add request to log
										COM.Handler.LogHandler.Traffic(
											string.Format(COM.Constants.TRAFFICLOG,
												state.WorkSocket.RemoteEndPoint,
												System.Threading.Thread.CurrentThread.ManagedThreadId,
												msg.Id,
												msg.Action.ToString(),
												msg.Status.ToString(),
												state.DataBigBuffer.LongLength
												)
											);
									}
#else
								if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
								{
									// Add request to log
									COM.Handler.LogHandler.Traffic(
										string.Format(COM.Constants.TRAFFICLOG,
											state.WorkSocket.RemoteEndPoint,
											System.Threading.Thread.CurrentThread.ManagedThreadId,
											msg.Id,
											msg.Action.ToString(),
											msg.Status.ToString(),
											state.DataBigBuffer.LongLength
											)
										);
								}
#endif
									#endregion
									#region request case
									COM.IndexusMessage replicationMessage = null;

									// if replication Request arrives as status value a different server node is the holder of this object
									if (ServiceLogic.NetworkDistribution.ReplicationEnabled && msg.Status != COM.IndexusMessage.StatusValue.ReplicationRequest)
									{
										// create a new object of received msg. to broadcast on servers.
										replicationMessage = new SharedCache.WinServiceCommon.IndexusMessage();
										replicationMessage.Action = msg.Action;
										replicationMessage.Status = COM.IndexusMessage.StatusValue.ReplicationRequest;
										replicationMessage.Key = msg.Key;
										replicationMessage.Payload = msg.Payload;
									}

									// switch over cache actions
									switch (msg.Action)
									{
										case SharedCache.WinServiceCommon.IndexusMessage.ActionValue.VersionNumberSharedCache:
											{
												#region VersionNumberSharedCache
												msg.Action = COM.IndexusMessage.ActionValue.Successful;
												msg.Payload = COM.Formatters.Serialization.BinarySerialize(
														new AssemblyInfo().Version
													);
												this.SendResponse(state, msg);
												break;
												#endregion VersionNumberSharedCache
											}
										case COM.IndexusMessage.ActionValue.VersionNumberClr:
											{
												#region VersionNumberClr
												msg.Action = COM.IndexusMessage.ActionValue.Successful;
												msg.Payload = COM.Formatters.Serialization.BinarySerialize(Environment.Version.ToString());
												this.SendResponse(state, msg);
												break;
												#endregion VersionNumberClr
											}
										case COM.IndexusMessage.ActionValue.Ping:
											{
												#region Ping
												msg.Action = COM.IndexusMessage.ActionValue.Successful;
												msg.Payload = null;
												this.SendResponse(state, msg);
												break;
												#endregion Ping
											}
										case COM.IndexusMessage.ActionValue.Add:
											{
												#region Add Case
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine(@"Adding new Item with Key: {0}", msg.Key);
													}
#endif
													LocalCache.Add(msg.Key, msg.Payload);
													// if the given msg expires is not MaxValue
													// it will be listed sub-process which clean
													// up in iterations the cache.
													// QuickFix
													if (msg.Expires != DateTime.MaxValue)
													{
														if (this.expire != null)
														{
															this.expire.Expire.DumpCacheItemAt(msg.Key, msg.Expires);
														}
													}

													// update cleanup list with new object
													CacheCleanup.Update(msg);


													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Payload = null;
													msg.Status = COM.IndexusMessage.StatusValue.Response;

													// send object back throug the connection
													this.SendResponse(state, msg);

													#region rschuetz: MODIFIED: 21-07-2007: distribute object over wire to other installations
													// Question is if the client needs to wait until this happens, 
													// or should the client first get an answer and just then it 
													// will distribute it.
													if (ServiceLogic.NetworkDistribution.ReplicationEnabled && msg.Status != COM.IndexusMessage.StatusValue.ReplicationRequest)
													{
														ServiceLogic.NetworkDistribution.Replicate(replicationMessage);
													}
													#endregion
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												finally
												{
													// handle max size and purge issues
													if (this.cacheAmountOfObjects != -1 && this.cacheAmountOfObjects <= LocalCache.CalculatedCacheSize && !CacheCleanup.PurgeIsRunning)
													{
#if DEBUG
														if (writeStats)
														{
															Console.WriteLine(@"Current Size of Cache: {0} ; {1} ", LocalCache.CalculatedCacheSize, LocalCache.CalculatedCacheSize <= 0 ? 0 : LocalCache.CalculatedCacheSize / (1024 * 1024));
														}
#endif
														List<string> remove = CacheCleanup.Purge(LocalCache.CalculatedCacheSize);
														if (remove != null)
														{
															lock (bulkObject)
															{
																foreach (string s in remove)
																{
																	LocalCache.Remove(s);
																}
															}
														}
													}
												}
												break;
												#endregion Add Case
											}
										case COM.IndexusMessage.ActionValue.ExtendTtl:
											{
												#region Extend Time To Live
												// workitem: http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=6129
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Search for Object with Key: {0}, Current Cache Size: {1}", msg.Key, LocalCache.Amount());
													}
#endif
													CacheCleanup.Update(msg);
													this.expire.Expire.DumpCacheItemAt(msg.Key, msg.Expires);

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = null;
													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												break;
												#endregion Extend Time To Live												
											}
										case COM.IndexusMessage.ActionValue.Get:
											{
												#region Receive object from cache
												byte[] objectFromCache = new byte[1] { 0 };
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Search for Object with Key: {0}, Current Cache Size: {1}", msg.Key, LocalCache.Amount());
													}
#endif
													objectFromCache = LocalCache.Get(msg.Key);
													// send new object back to client within same socket which contains the cache object.
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Action = COM.IndexusMessage.ActionValue.Successful;

													msg.Payload = objectFromCache;
													// update cleanup list
													CacheCleanup.Update(msg);

													// bugfix: http://www.codeplex.com/WorkItem/View.aspx?ProjectName=SharedCache&WorkItemId=6167
													// return null if item is already expried
													if (this.expire.Expire.CheckExpired(msg.Key))
													{
														this.expire.Expire.Remove(msg.Key);
														LocalCache.Remove(msg.Key);
														CacheCleanup.Remove(msg.Key);

#if DEBUG
														if (writeStats)
														{
															Console.WriteLine("Cleared Object with Key: {0}, Current Cache Size: {1}", msg.Key, LocalCache.Amount());
														}
#endif
														if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
														{
															COM.Handler.LogHandler.Info(string.Format("Item with Key:'{0}' has been removed and Server node {1} return NULL", msg.Key, Environment.MachineName));
														}

														msg.Payload = null;
													}
													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												break;
												#endregion
											}
										case COM.IndexusMessage.ActionValue.Remove:
											{
												#region Remove Case
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Remove Object with Key: {0}, current Cache amount: {1}", msg.Key, LocalCache.Amount());
													}
#endif
													// remove it from the cache.
													LocalCache.Remove(msg.Key);
													if (this.expire != null)
													{
														// remove it from the cache expiry job.
														this.expire.Expire.Remove(msg.Key);
													}
													// update cleanup list
													CacheCleanup.Remove(msg.Key);

													#region rschuetz: MODIFIED: 21-07-2007: distribute object over wire to other installations
													// Question is if the client needs to wait until this happens, 
													// or should the client first get an answer and just then it 
													// will distribute it.
													if (ServiceLogic.NetworkDistribution.ReplicationEnabled && msg.Status != COM.IndexusMessage.StatusValue.ReplicationRequest)
													{
														ServiceLogic.NetworkDistribution.Replicate(replicationMessage);
													}
													#endregion

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = null;
													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												break;
												#endregion Remove Case
											}
										case COM.IndexusMessage.ActionValue.RegexGet:
											{
												#region Receiving data based on regular expression pattern
												try
												{
													#region
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine(@"RegexGet Operation");
													}
#endif
													#endregion
													IDictionary<string, byte[]> result = new Dictionary<string, byte[]>();

													string regularExpressionPattern = Encoding.UTF8.GetString(msg.Payload);
													Regex regex = new Regex(regularExpressionPattern, RegexOptions.CultureInvariant);

													List<string> actualData = LocalCache.GetAllKeys();

													foreach (string n in actualData)
													{
														if (regex.IsMatch(n))
														{
															result.Add(n, LocalCache.Get(n));
														}
													}

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = COM.Formatters.Serialization.BinarySerialize(result);

													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													#region Exception Handling
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);
														this.SendResponse(state, resultMsg);
													}
													#endregion Exception Handling
												}
												break;
												#endregion Receiving data based on regular expression pattern
											}
										case COM.IndexusMessage.ActionValue.RegexRemove:
											{
												#region Regex Remove
												try
												{
													Regex regex = new Regex(Encoding.UTF8.GetString(msg.Payload), RegexOptions.CultureInvariant);

													#region local logging
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Regex Remove Keys which match pattern: {0}", regex.ToString());
													}
#endif
													#endregion local logging
													List<string> keysToRemove = new List<string>();
													List<string> actualData = LocalCache.GetAllKeys();
													foreach (string n in actualData)
													{
														if (regex.IsMatch(n))
															keysToRemove.Add(n);
													}

													foreach (string key in keysToRemove)
													{
														// remove it from the cache.
														LocalCache.Remove(key);
														if (this.expire != null)
														{
															// remove it from the cache expiry job.
															this.expire.Expire.Remove(key);
														}
														// update cleanup list
														CacheCleanup.Remove(key);
													}


													#region rschuetz: MODIFIED: 21-07-2007: distribute object over wire to other installations
													// Question is if the client needs to wait until this happens, 
													// or should the client first get an answer and just then it 
													// will distribute it.
													if (ServiceLogic.NetworkDistribution.ReplicationEnabled && msg.Status != COM.IndexusMessage.StatusValue.ReplicationRequest)
													{
														ServiceLogic.NetworkDistribution.Replicate(replicationMessage);
													}
													#endregion

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = null;
													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												break;
												#endregion Regex Remove
											}
										case COM.IndexusMessage.ActionValue.RemoveAll:
											{
												#region Clear all cache items!
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Clear all cache items!");
													}
#endif
													lock (bulkObject)
													{
														// remove all items from cache
														LocalCache.Clear();

														if (this.expire != null)
														{
															// remove all items from the cache expiry job.
															this.expire.Expire.Clear();
														}

														// remove all items from cleanup
														CacheCleanup.Clear();
													}

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = null;
													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}


												}
												break;
												#endregion
											}
										case COM.IndexusMessage.ActionValue.GetAllKeys:
											{
												#region Get All Keys
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Get All available Key's!");
													}
#endif
													List<string> keys = LocalCache.GetAllKeys();

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = COM.Formatters.Serialization.BinarySerialize(keys);

													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);
														this.SendResponse(state, resultMsg);
													}
												}
												break;
												#endregion
											}
										case COM.IndexusMessage.ActionValue.Statistic:
											{
												#region Statistic

												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("Message Action: {0}", msg.Action.ToString());
													}
#endif
													COM.IndexusStatistic stats;
													// previous clients are sending a stats object, newer clients are 
													// sending nothing to reduce interaction on the wire.
													if (msg.Payload != null)
														stats = COM.Formatters.Serialization.BinaryDeSerialize<COM.IndexusStatistic>(msg.Payload);
													else
														stats = new COM.IndexusStatistic();

													// receive cache attributes and add them to cache object
													stats.ServiceAmountOfObjects = LocalCache.Amount();
													stats.ServiceTotalSize = LocalCache.Size();
													stats.ServiceUsageList = CacheCleanup.GetAccessStatList();

													// adding results to return message and set all needed attributes.
													msg.Payload = COM.Formatters.Serialization.BinarySerialize(stats);
													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;

													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);
														this.SendResponse(state, resultMsg);
													}
												}
												break;
												#endregion
											}
										case COM.IndexusMessage.ActionValue.GetAbsoluteTimeExpiration:
											{
												// As requested: http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=6170
												#region Collecting absolut expiration DateTime for provided keys
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine(@"GetAbsoluteTimeExpiration Operation: {0}", msg.Key);
													}
#endif
													IDictionary<string, DateTime> result = new Dictionary<string, DateTime>();
													List<string> requstedData = COM.Formatters.Serialization.BinaryDeSerialize<List<string>>(msg.Payload);
													if (requstedData != null)
													{
														foreach (string key in requstedData)
														{
															bool addItem = true;

															if (this.expire.Expire.CheckExpired(key))
															{
																result.Add(key, DateTime.MinValue);
															}
															else
															{
																result.Add(key, this.expire.Expire.GetExpireDateTime(key));
															}
														}

														msg.Action = COM.IndexusMessage.ActionValue.Successful;
														msg.Status = COM.IndexusMessage.StatusValue.Response;
														msg.Payload = COM.Formatters.Serialization.BinarySerialize(result);

														this.SendResponse(state, msg);
													}
													else
													{
														throw new Exception("Payload could not be deserialized into a list of strings!");
													}
												}
												catch (Exception ex)
												{
													#region Exception Handling
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);
														this.SendResponse(state, resultMsg);
													}
													#endregion Exception Handling
												}
												break;
												#endregion Collecting absolut expiration DateTime for provided keys
											}
										case COM.IndexusMessage.ActionValue.MultiGet:
											{
												#region Receiving a list of single items from the cache
												try
												{
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine(@"MultiGet Operation: {0}", msg.Key);
													}
#endif
													IDictionary<string, byte[]> result = new Dictionary<string, byte[]>();
													List<string> requstedData = COM.Formatters.Serialization.BinaryDeSerialize<List<string>>(msg.Payload);
													if (requstedData != null)
													{
														foreach (string key in requstedData)
														{
															bool addItem = true;
															// bugfix: http://www.codeplex.com/WorkItem/View.aspx?ProjectName=SharedCache&WorkItemId=6167
															// return null if item is already expried
															if (this.expire.Expire.CheckExpired(key))
															{
																this.expire.Expire.Remove(key);
																LocalCache.Remove(key);
																CacheCleanup.Remove(key);
#if DEBUG
																if (writeStats)
																{
																	Console.WriteLine("Cleared Object with Key: {0}, Current Cache Size: {1}", msg.Key, LocalCache.Amount());
																}
#endif
																if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
																{
																	COM.Handler.LogHandler.Info(string.Format("Item with Key:'{0}' has been removed and Server node {1} return NULL", msg.Key, Environment.MachineName));
																}

																addItem = false;
															}

															if (addItem)
															{
																result.Add(key, LocalCache.Get(key));
															}
														}

														msg.Action = COM.IndexusMessage.ActionValue.Successful;
														msg.Status = COM.IndexusMessage.StatusValue.Response;
														msg.Payload = COM.Formatters.Serialization.BinarySerialize(result);

														this.SendResponse(state, msg);
													}
													else
													{
														throw new Exception("Payload could not be deserialized into a list of strings!");
													}
												}
												catch (Exception ex)
												{
													#region Exception Handling
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);
														this.SendResponse(state, resultMsg);
													}
													#endregion Exception Handling
												}
												break;
												#endregion Receiving a list of single items from the cache
											}
										case COM.IndexusMessage.ActionValue.MultiAdd:
											{
												#region Adding a bunch of data
												try
												{
													#region Stats
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine(@"Adding a bunch of data - MultiAdd");
													}
#endif
													#endregion Stats

													IDictionary<string, byte[]> data =
														COM.Formatters.Serialization.BinaryDeSerialize<IDictionary<string, byte[]>>(msg.Payload);

													foreach (KeyValuePair<string, byte[]> item in data)
													{
														LocalCache.Add(item.Key, item.Value);
														// if the given msg expires is not MaxValue
														// it will be listed sub-process which clean
														// up in iterations the cache.
														// QuickFix
														if (msg.Expires != DateTime.MaxValue)
														{
															if (this.expire != null)
															{
																this.expire.Expire.DumpCacheItemAt(item.Key, msg.Expires);
															}
														}

														using (COM.IndexusMessage multiMsg = new COM.IndexusMessage(
																			COM.Handler.Unique.NextUniqueId(), msg.Status, msg.Action, msg.ItemPriority,
																			msg.Hostname, msg.Expires, item.Key, item.Value
																		)
																	)
														{
															// update cleanup list with new object
															CacheCleanup.Update(multiMsg);
														}
													}

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Payload = null;
													msg.Status = COM.IndexusMessage.StatusValue.Response;

													// send object back throug the connection
													this.SendResponse(state, msg);

													#region rschuetz: MODIFIED: 21-07-2007: distribute object over wire to other installations
													// Question is if the client needs to wait until this happens, 
													// or should the client first get an answer and just then it 
													// will distribute it.
													if (ServiceLogic.NetworkDistribution.ReplicationEnabled && msg.Status != COM.IndexusMessage.StatusValue.ReplicationRequest)
													{
														ServiceLogic.NetworkDistribution.Replicate(replicationMessage);
													}
													#endregion
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												finally
												{
													// handle max size and purge issues
													if (this.cacheAmountOfObjects != -1 && this.cacheAmountOfObjects <= LocalCache.CalculatedCacheSize && !CacheCleanup.PurgeIsRunning)
													{
#if DEBUG
														if (writeStats)
														{
															Console.WriteLine(@"Current Size of Cache: {0} ; {1} ", LocalCache.CalculatedCacheSize, LocalCache.CalculatedCacheSize <= 0 ? 0 : LocalCache.CalculatedCacheSize / (1024 * 1024));
														}
#endif
														List<string> remove = CacheCleanup.Purge(LocalCache.CalculatedCacheSize);
														if (remove != null)
														{
															lock (bulkObject)
															{
																foreach (string s in remove)
																{
																	LocalCache.Remove(s);
																}
															}
														}
													}
												}
												break;
												#endregion Adding a bunch of data
											}
										case COM.IndexusMessage.ActionValue.MultiDelete:
											{
												#region Remove a bunch of data
												try
												{
													#region Stats
#if DEBUG
													if (writeStats)
													{
														Console.WriteLine("MultiDelete Object with Key: {0}, current Cache amount: {1}", msg.Key, LocalCache.Amount());
													}
#endif
													#endregion Stats

													List<string> requstedDataToDelete = COM.Formatters.Serialization.BinaryDeSerialize<List<string>>(msg.Payload);

													foreach (string key in requstedDataToDelete)
													{
														// remove it from the cache.
														LocalCache.Remove(key);
														if (this.expire != null)
														{
															// remove it from the cache expiry job.
															this.expire.Expire.Remove(key);
														}
														// update cleanup list
														CacheCleanup.Remove(key);
													}

													msg.Action = COM.IndexusMessage.ActionValue.Successful;
													msg.Status = COM.IndexusMessage.StatusValue.Response;
													msg.Payload = null;
													this.SendResponse(state, msg);
												}
												catch (Exception ex)
												{
													using (COM.IndexusMessage resultMsg = new COM.IndexusMessage())
													{
														resultMsg.Action = COM.IndexusMessage.ActionValue.Error;
														resultMsg.Status = COM.IndexusMessage.StatusValue.Response;
														resultMsg.Payload = COM.Formatters.Serialization.BinarySerialize(
															new COM.CacheException(msg.Action.ToString(), msg.Action, msg.Status, ex)
														);

														this.SendResponse(state, resultMsg);
													}
												}
												break;

												#endregion Remove a bunch of data
											}

									}
									break;
									#endregion request case
								}
								finally
								{
									// msg is not needed anymore and since its implmente IDisposable it can be disposed at this place
									if (msg != null)
									{
										msg.Dispose();
									}
								}


							}
						case COM.IndexusMessage.StatusValue.Response:
							{
								#region response case
#if DEBUG
								if (writeStats)
								{
									Console.WriteLine("Received Action: {0}", msg.Status.ToString());
								}
#endif
								// Server never should receive a Response from the client!
								msg.Action = COM.IndexusMessage.ActionValue.Error;
								string info = string.Format("[{0}]Error, the message should have the Status Request!", state.WorkSocket.RemoteEndPoint);
								Console.WriteLine(info);
								COM.Handler.LogHandler.Traffic(info);
								COM.Handler.LogHandler.Error(info);
								break;
								#endregion response case
							}
					}
				}
			}
			else
			{
				COM.Handler.LogHandler.Error("Cannot handle state because state is NULL");
				Console.WriteLine("Cannot handle state because state is NULL");
			}
		}

		/// <summary>
		/// Sends the response.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <param name="response">The response.</param>
		private void SendResponse(COM.Sockets.SharedCacheStateObject state, COM.IndexusMessage response)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			// adding the protocol header !!!! very important
			byte[] arr = COM.Handler.UtilByte.CreateMessageHeader(response.GetBytes());

			// manage first state object values before sending back data;
			state.DataBigBuffer = arr;
			state.MessageLength = arr.LongLength;

			// start async send
			this.Send(state);
		}

		/// <summary>
		/// Sends the specified state.
		/// </summary>
		/// <param name="state">The state.</param>
		private void Send(COM.Sockets.SharedCacheStateObject state)
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (state != null)
			{
				if (state.WorkSocket != null)
				{
					Socket socket = state.WorkSocket;
					if (state.DataBigBuffer != null && state.DataBigBuffer.Length > 0)
					{
						// Thread.Sleep(1);

						// todo: count is not good to use since its a int and not long value!!!
						socket.BeginSend(state.DataBigBuffer, 0, state.DataBigBuffer.Length, 0,
							new AsyncCallback(this.SendCallback), state);
					}
					else
					{
						// handler is now ready to receive more data from client
						//TimeSpan sp = DateTime.Now.Subtract(state.AliveTimeStamp);
						//Console.WriteLine("Send:" + sp.TotalMilliseconds);

						state.ReadHeader = true;
						state.AliveTimeStamp = DateTime.Now;
						state.AlreadySent = 0;
						state.DataBigBuffer = null;// new byte[COM.Sockets.SharedCacheStateObject.BufferSize * 2];
						state.MessageLength = 0;
						state.AlreadyRead = 0;

						socket.BeginReceive(state.Buffer, 0, COM.Sockets.SharedCacheStateObject.BufferSize, 0,
							new AsyncCallback(this.ReadCallback), state);
					}
				}
				else
				{
					Console.WriteLine(@"State worker socket is NULL");
					COM.Handler.LogHandler.Error(@"State worker socket is NULL");
				}
			}
			else
			{
				Console.WriteLine("Cannot handle state because state is NULL");
				COM.Handler.LogHandler.Error("Cannot handle state because state is NULL");
			}
		}

		/// <summary>
		/// Sends the callback.
		/// </summary>
		/// <param name="ar">The ar.</param>
		private void SendCallback(IAsyncResult ar)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			COM.Sockets.SharedCacheStateObject state = ar.AsyncState as COM.Sockets.SharedCacheStateObject;
			if (state != null)
			{
				Socket handler = state.WorkSocket;
				if (handler != null)
				{
					try
					{

						int sent = handler.EndSend(ar);
						// Console.WriteLine(@"Sent: {0}", sent);
						state.AlreadySent += (long)sent;
						if (state.AlreadySent < state.MessageLength)
						{
							// send the next bunch of data until
							handler.BeginSend(state.DataBigBuffer, (int)state.AlreadySent,
								state.DataBigBuffer.Length, 0, new AsyncCallback(this.SendCallback), state);
						}
						else
						{
							//TimeSpan sp = DateTime.Now.Subtract(state.AliveTimeStamp);
							//Console.WriteLine("SendCallback:" + sp.TotalMilliseconds);
							// handler is now ready to receive more data from client
							state.ReadHeader = true;
							state.AliveTimeStamp = DateTime.Now;
							state.AlreadySent = 0;
							state.DataBigBuffer = null; // new byte[COM.Sockets.SharedCacheStateObject.BufferSize * 2];
							state.MessageLength = 0;
							state.AlreadyRead = 0;

							handler.BeginReceive(state.Buffer, 0, COM.Sockets.SharedCacheStateObject.BufferSize, 0,
								new AsyncCallback(this.ReadCallback), state);
						}
					}
					catch (SocketException sEx)
					{
						COM.Handler.LogHandler.Force(string.Format("[{0}] Socket ErrorCode:{1}\n SocketErrorCode:{2}\nMessage: {3}", state.WorkSocket.RemoteEndPoint.ToString(), sEx.ErrorCode, sEx.SocketErrorCode.ToString(), sEx.Message));
					}
					catch (Exception ex)
					{
						COM.Handler.LogHandler.Error(ex);
						Console.WriteLine("Exception: {0}", ex.Message);
					}
				}
				else
				{
					COM.Handler.LogHandler.Force(@"Socket is NULL and cannot send data to client back!!!");
				}
			}
			else
			{
				COM.Handler.LogHandler.Force(@"State is NULL!!!");
				Console.WriteLine(@"State is NULL!!!");
			}
		}

		/// <summary>
		/// Checks connected sockets.
		/// </summary>
		/// <param name="eventState">State of the event.</param>
		private void CheckSockets(object eventState)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.lostTimer.Change(Timeout.Infinite, Timeout.Infinite);
			try
			{
				ArrayList arr = new ArrayList(this.conntectedSockets);

				foreach (COM.Sockets.SharedCacheStateObject state in arr)
				{
					if (state.WorkSocket == null)
					{
						// remove invalid state object
						try
						{
							lock (this.conntectedSockets)
							{
								if (this.conntectedSockets.Contains(state))
								{
									this.conntectedSockets.Remove(state);
									Interlocked.Decrement(ref socketCount);
								}
							}
						}
						catch (Exception ex)
						{
							COM.Handler.LogHandler.Error(@"Problem to cleanup", ex);
						}
					}
					else
					{
						if (DateTime.Now.AddTicks(-state.AliveTimeStamp.Ticks).Minute > timeoutMinutes)
						{
							this.RemoveSocket(state);
						}
					}
				}
			}
			catch (Exception ex)
			{
				COM.Handler.LogHandler.Error(@"Problem to cleanup", ex);
			}
			finally
			{
				lostTimer.Change(TcpServer.timerTimeout, TcpServer.timerTimeout);
			}
		}

		/// <summary>
		/// Removes socket
		/// </summary>
		/// <param name="state">The state.</param>
		private void RemoveSocket(COM.Sockets.SharedCacheStateObject state)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Socket sock = state.WorkSocket;
			try
			{
				lock (this.conntectedSockets)
				{
					if (this.conntectedSockets.Contains(state))
					{
						this.conntectedSockets.Remove(state);
						Interlocked.Decrement(ref this.socketCount);
					}
				}

#if DEBUG
				Console.WriteLine("Connection Count {0}", this.socketCount);
#endif

			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine("Excpetion: " + ex);
#endif
				if (1 == COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
				{
					COM.Handler.LogHandler.Error("Exception in RemoveSocket", ex);
				}
			}

			try
			{
				lock (this.conntectedHt)
				{
					if (sock != null && (this.conntectedHt.ContainsKey(sock)))
					{
						object socketTemp = this.conntectedHt[sock];
						if (this.conntectedHt.ContainsKey(socketTemp))
						{
							if (this.conntectedHt.ContainsKey(this.conntectedHt[socketTemp]))
							{
								this.conntectedHt.Remove(sock);
								if (sock.Equals(this.conntectedHt[socketTemp]))
								{
									this.conntectedHt.Remove(socketTemp);
								}
								else
								{
									object value, key = socketTemp;
									while (true)
									{
										value = this.conntectedHt[key];
										if (sock.Equals(value))
										{
											this.conntectedHt[key] = socketTemp;
											break;
										}
										else if (this.conntectedHt.ContainsKey(value))
										{
											key = value;
										}
										else
										{
											// chain is broken.
											break;
										}
									}
								}
							}
							else
							{
								COM.Handler.LogHandler.Info("Socket is not in connected Hash table");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Excpetion: " + ex);
			}

			try
			{
				if (sock != null)
				{
					sock.Shutdown(SocketShutdown.Both);
					// sock.Close();
					if (sock != null)
						sock = null;
					// shutdown the state
					state = null;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Excpetion: " + ex);
			}
		}
		#endregion Private Methods
		#endregion Async TCP Connection Handling

		#region Override Methods
		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			StringBuilder sb = new StringBuilder();


			#region Override ToString() default with reflection
			Type t = this.GetType();
			PropertyInfo[] pis = t.GetProperties();
			for (int i = 0; i < pis.Length; i++)
			{
				try
				{
					PropertyInfo pi = (PropertyInfo)pis.GetValue(i);
					COM.Handler.LogHandler.Info(
					string.Format(
					"{0}: {1}",
					pi.Name,
					pi.GetValue(this, new object[] { })
					)
					);
					sb.AppendFormat("{0}: {1}" + Environment.NewLine, pi.Name, pi.GetValue(this, new object[] { }));
				}
				catch (Exception ex)
				{
					COM.Handler.LogHandler.Error("Could not log property. Ex. Message: " + ex.Message);
				}
			}
			#endregion Override ToString() default with reflection

			return sb.ToString();
		}

		#endregion Override Methods
	}
}
