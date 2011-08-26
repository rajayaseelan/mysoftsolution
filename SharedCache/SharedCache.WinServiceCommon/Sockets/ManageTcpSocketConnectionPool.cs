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
// Name:      ManageTcpSocketConnectionPool.cs
// 
// Created:   10-02-2008 SharedCache.com, rschuetz
// Modified:  10-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Net;
using System.Timers;
using System.Diagnostics;


namespace SharedCache.WinServiceCommon.Sockets
{
	/// <summary>
	/// Managing the various Connection Pools to each server node within the client
	/// </summary>
	public class ManageTcpSocketConnectionPool
	{
		/// <summary>
		/// lock down pool upon add / receive shared cache sockets
		/// </summary>
		private static object bulkObject = new object();
		/// <summary>
		/// contains the configured host in web.config / app.config
		/// </summary>
		private Dictionary<string, int> configuredHosts = new Dictionary<string, int>();
		/// <summary>
		/// a dictonary with the relevant pool for each server node
		/// </summary>
		private Dictionary<string, TcpSocketConnectionPool> pools = new Dictionary<string, TcpSocketConnectionPool>();

		/// <summary>
		/// a Timer which validat created pools. 
		/// </summary>
		/// Bugfix for: http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=5847
		System.Timers.Timer validatePoolTimer = null;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ManageTcpSocketConnectionPool"/> class.
		/// </summary>
		public ManageTcpSocketConnectionPool(bool instanceClientPools)
		{

			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
#if DEBUG
			Debug.WriteLine(string.Format("socket connection pool created for {0}", instanceClientPools ? "client" : "server"));
#endif
			#endregion Access Log

			lock (bulkObject)
			{
				if (this.configuredHosts.Count == 0)
				{
					if (instanceClientPools)
					{
						#region reading provider data from the config file
						foreach (Configuration.Client.IndexusServerSetting configEntry in Provider.Cache.IndexusDistributionCache.SharedCache.ServersList)
						{
							// default port!
							int port = 48888;
							int.TryParse(configEntry.Port, out port);
							this.configuredHosts.Add(configEntry.IpAddress, port);
						}
						#endregion

						#region upon replication the system creates also a pool for backup replication server nodes
						foreach (Configuration.Client.IndexusServerSetting configEntry in Provider.Cache.IndexusDistributionCache.SharedCache.ReplicatedServersList)
						{
							int port = 48888;
							int.TryParse(configEntry.Port, out port);
							this.configuredHosts.Add(configEntry.IpAddress, port);
						}
						#endregion
					}
					else
					{
						#region ensure client does not configure the instance itself!!!
						IPAddress ip = Handler.Network.GetFirstIPAddress();

						foreach (Configuration.Server.IndexusServerSetting configEntry in Provider.Server.IndexusServerReplicationCache.CurrentProvider.ServersList)
						{
							// validate server does not add itself.
							if (!ip.ToString().Equals(configEntry.IpAddress))
							{
								this.configuredHosts.Add(configEntry.IpAddress, configEntry.Port);
							}
						}
						#endregion
					}
					double doubleMs = 180000;
					if (instanceClientPools)
					{
						doubleMs = Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolValidationInterval.TotalMilliseconds;
					}
					else
					{
						doubleMs = Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.SocketPoolValidationInterval.TotalMilliseconds;
					}

#if DEBUG
					Debug.WriteLine("SocketPoolValidationInterval amount:" + doubleMs.ToString());
#endif

					this.validatePoolTimer = new System.Timers.Timer();
					this.validatePoolTimer.Elapsed += new ElapsedEventHandler(ValidateConnectionPools);
					this.validatePoolTimer.Interval = doubleMs;
					this.validatePoolTimer.Start();
				}
				// init each for each entry a pool
				foreach (KeyValuePair<string, int> host in this.configuredHosts)
				{
					TcpSocketConnectionPool newPool = new TcpSocketConnectionPool();
					newPool.Host = host.Key;
					newPool.Port = host.Value;
					pools.Add(host.Key, newPool);
					
					int defaultPoolSize = 5;
					if (instanceClientPools)
					{
						defaultPoolSize = Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolMinAvailableSize;
					}
					else
					{
						defaultPoolSize = Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.SocketPoolMinAvailableSize;
					}
#if DEBUG
					Debug.WriteLine(string.Format("Pool size is {0} for host {1}", defaultPoolSize, host.Key));
#endif
					// set tcp pool size
					pools[host.Key].PoolSize = defaultPoolSize;
					// enable pool
					pools[host.Key].Enable();
				}
			}
		}

		/// <summary>
		/// Determinates if specific pool is available.
		/// </summary>
		/// <param name="host">A <see cref="string"/> which defines the host.</param>
		/// <returns>A <see cref="bool"/> which represents the status.</returns>
		public bool Available(string host)
		{
			return pools[host].PoolAvailable;
		}

		/// <summary>
		/// Validates all available pools. In case a one of the pool's is disabled 
		/// it will enable it automatically after it can ping specific server node.
		/// </summary>
		void ValidateConnectionPools(object sender, ElapsedEventArgs e)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			try
			{
				foreach (KeyValuePair<string, TcpSocketConnectionPool> p in pools)
				{
					#region Logging
					//if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
					//{
					//  #if TRACE
					//  Console.WriteLine("Validate Pool: " + host);	
					//  #endif
					//  #if DEBUG
					//  Console.WriteLine("Validate Pool: " + p.Key);
					//  #endif
					//  Handler.LogHandler.Fatal("Validate Pool: " + p.Key);
					//}
					#endregion
#if DEBUG
					Debug.WriteLine(string.Format("validate host: {0}", p.Key));
#endif
					lock (bulkObject)
					{
						p.Value.Validate();
					}						
				}
			}
			catch (Exception ex)
			{
				#if DEBUG
				{
					Console.WriteLine("ValidatePools throws an Exception! Message: " + ex.Message);
				}
				#endif
				Handler.LogHandler.Fatal("ValidatePools throws an Exception!", ex);
			}
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="ManageTcpSocketConnectionPool"/> is reclaimed by garbage collection.
		/// </summary>
		~ManageTcpSocketConnectionPool()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			// Bugfix for: http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=5847
			if (this.validatePoolTimer != null)
			{
				if (this.validatePoolTimer.Enabled)
				{
					validatePoolTimer.Stop();
				}

				validatePoolTimer.Dispose();
			}

			foreach (KeyValuePair<string, int> host in this.configuredHosts)
			{
				pools[host.Key] = null;
			}
		}

		/// <summary>
		/// Gets the socket from pool.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns>A <see cref="SharedCacheTcpClient"/> object.</returns>
		public SharedCacheTcpClient GetSocketFromPool(string host)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
#if DEBUG
			Debug.WriteLine(string.Format("retrieve socket from host: {0}", host));
#endif
			TcpSocketConnectionPool pool = pools[host];
			return pool.GetSocket();
		}

		internal bool GetPoolStatus(string host)
		{
			return pools[host].PoolAvailable;
		}

		internal bool Disable(string host)
		{
#if DEBUG
			Debug.WriteLine(string.Format("disable host: {0}", host));
#endif
			try
			{
				bool actualStatus = pools[host].PoolAvailable;
				lock (bulkObject)
				{
					// need to set the attribute within the instance for enabling the pool from the validate
					// method itself.
					pools[host].Disable();
				}

				if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
				{
					string msg = string.Empty;
					if (actualStatus)
					{
						msg = "Server node is NOT available: {0}. Client tries to reconnect every {1}.";
					}
					else
					{
						msg = "Server node {0} is still NOT available.";
					}
					#if TRACE
					Console.WriteLine(msg, host, Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolValidationInterval.TotalMilliseconds);
					#endif
					#if DEBUG
					Console.WriteLine(msg, host, Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolValidationInterval.TotalMilliseconds);
					#endif
					Handler.LogHandler.Fatal(string.Format(msg, host, Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.SocketPoolValidationInterval.TotalMilliseconds));
				}				
				return true;
			}
			catch (Exception ex)
			{
				Handler.LogHandler.Fatal("Client tried to disable host " + host + ". during this step an exception appeared.",ex);
				return false;
			}

		}


		/// <summary>
		/// Puts the socket to pool.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="socket">The socket.</param>
		public void PutSocketToPool(string host, SharedCacheTcpClient socket)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
#if DEBUG
			Debug.WriteLine(string.Format("put socket to pool: {0}", host));
#endif
			#endregion Access Log

			TcpSocketConnectionPool pool = pools[host];
			pool.PutSocket(socket);
		}
	}
}
