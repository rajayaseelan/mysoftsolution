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
// Name:      TcpSocketConnectionPool.cs
// 
// Created:   10-02-2008 SharedCache.com, rschuetz
// Modified:  10-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon.Sockets
{
	/// <summary>
	/// Represents a pool of connections for a specific server node.
	/// <example>
	/// Server: 192.168.0.1 Port: 48888
	/// </example>
	/// </summary>
	internal class TcpSocketConnectionPool
	{
		private static object bulkObject = new object();

		#region Property: Host
		private string host;

		/// <summary>
		/// Gets/sets the Host
		/// </summary>
		public string Host
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.host; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.host = value; }
		}
		#endregion
		#region Property: Port
		private int port;

		/// <summary>
		/// Gets/sets the Port
		/// </summary>
		public int Port
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.port; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.port = value; }
		}
		#endregion
		
		
		/// <summary>Queue of available socket connections.</summary>
		private Queue<SharedCacheTcpClient> availableSockets = new Queue<SharedCacheTcpClient>();
		
		/// <summary>The maximum size of the connection pool.</summary>
		internal int PoolSize { get; set; }
		
		/// <summary>
		/// Identify if this pool is available or not
		/// </summary>
		public bool PoolAvailable { get; private set; }

		internal void Disable()
		{
			this.PoolAvailable = false;	
		}

		internal void Enable()
		{
			this.PoolAvailable = true;
		}

		/// <summary>
		/// Validates this instance.
		/// </summary>
		internal void Validate()
		{			
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			if (this.PoolAvailable)
			{
				#region validate all open connections when they used last time
				for (int i = 0; i < this.availableSockets.Count; i++)
				{
					SharedCacheTcpClient client = null;
					lock (bulkObject)
					{
						client = this.availableSockets.Dequeue();
					}
					TimeSpan sp = DateTime.Now.Subtract(client.LastUsed);
					// Console.WriteLine(@"last used: {0}m {1}s {2}ms", sp.Minutes, sp.Seconds, sp.Milliseconds);
					if (sp.Minutes >= 2)
					{
						client.Dispose();
					}
					else if (client != null && !client.Connected)
					{
						// this will close the socket in case we have to much open sockets.
						this.PutSocket(client);
					}
					else if (client != null)
					{
						lock (bulkObject)
						{
							this.availableSockets.Enqueue(client);
						}
					}
				}
				#endregion
			}
			else
			{
				#region try to enable pool in case its disabled
				if (CacheUtil.Ping(this.Host))
				{
					this.Enable();
					#region Logging
					string msg = "Client could reconnect to host {0} and it enables this node.";
					#if TRACE
					Console.WriteLine(msg, this.Host);	
					#endif
					#if DEBUG
					Console.WriteLine(msg, this.Host);
					#endif
					Handler.LogHandler.Fatal(string.Format(msg, this.Host));					
					#endregion
				}
				else
				{
					#region Logging
					string msg = "Client could NOT reconnect to host {0} and keeps this node disabled";
					#if TRACE
					Console.WriteLine(msg,this.Host);	
					#endif
					#if DEBUG
					Console.WriteLine(msg, this.Host);	
					#endif
					Handler.LogHandler.Fatal(string.Format(msg, this.Host));
					#endregion
				}
				#endregion
			}
		}

		/// <summary>
		/// Get an open socket from the connection pool.
		/// </summary>
		/// <returns>Socket returned from the pool or new socket
		/// opened.</returns>
		public SharedCacheTcpClient GetSocket()
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			if (this.availableSockets.Count > 0)
			{
				SharedCacheTcpClient socket = null;
				while (this.availableSockets.Count > 0)
				{
					lock (bulkObject)
					{
						socket = this.availableSockets.Dequeue();
					}
					if (socket.Connected)
					{
						TimeSpan sp = DateTime.Now.Subtract(socket.LastUsed);
						//TODO: read data from config file: SocketPoolTimeout
						if (sp.Minutes >= 2)
						{
							socket.Close();
							return this.OpenSocket();
						}
						else
							return socket;
					}
					else
					{
						if (socket != null)
						{
							// MAYBE we should consider to reconnect it instead to close it?
							socket.Close();
						}
					}
				}
			}
			return this.OpenSocket();
		}
		
		/// <summary>
		/// Return the given socket back to the socket pool.
		/// </summary>
		/// <param name="socket">Socket connection to return.</param>
		public void PutSocket(SharedCacheTcpClient socket)
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log
			//TODO: Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolMinAvailableSize
			// if (this.availableSockets.Count < Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolMinAvailableSize)
			if (this.availableSockets.Count <= this.PoolSize)
			// if (this.availableSockets.Count < TcpSocketConnectionPool.POOL_SIZE)
			{
				if (socket != null)
				{
					if (socket.Connected)
					{
						// Set the socket back to blocking and enqueue
						socket.SetBlockingMode(true);
						socket.LastUsed = DateTime.Now;
						lock (bulkObject)
						{
							this.availableSockets.Enqueue(socket);
						}						
					}
					else
					{
						socket.Close();
					}					
				}
			}
			else
			{
				// Number of sockets is above the pool size, so just close it.
				socket.Close();
			}
		}

		/// <summary>
		/// Open a new socket connection.
		/// </summary>
		/// <returns>Newly opened socket connection.</returns>
		private SharedCacheTcpClient OpenSocket()
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			return new SharedCacheTcpClient(this.host, this.port);
		}
	}
}
