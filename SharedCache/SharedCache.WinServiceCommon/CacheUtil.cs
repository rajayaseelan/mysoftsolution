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
// Name:      CacheUtil.cs
// 
// Created:   25-01-2007 SharedCache.com, rschuetz
// Modified:  25-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-09-2007 SharedCache.com, rschuetz : added Methods to set Hosts while adding or removing data from Cache
//																										 this is mainly used for internal or distributed cache.
// Modified:  30-09-2007 SharedCache.com, rschuetz : enabled back the commented methods (Add, Remove, Get)
// Modified:  30-09-2007 SharedCache.com, rschuetz : added an additional overload for Add " public static void Add(string key, object obj, string host)" so its similar with the provider
// Modified:  18-12-2007 SharedCache.com, rschuetz : since SharedCache works internally with byte[] instead of objects almoast all needed code has been adapted
// Modified:  04-01-2008 SharedCache.com, rschuetz : introduction on cache provider to clear() all cache data with one single call instead to iterate over all key's.
// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - protocol changed - added string key and byte[] payload instead KeyValuePair
// Modified:  12-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - added compression and decompression
// Modified:  12-02-2008 SharedCache.com, rschuetz : added HitRate to stats
// Modified:  23-02-2008 SharedCache.com, rschuetz : refactored - rewritten almaost every method
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// Modified:  29-02-2008 SharedCache.com, rschuetz : failover for replication as clientConext. if main server node is not available send message to replicated Server
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using SharedCache.WinServiceCommon.Configuration.Client;
using SharedCache.WinServiceCommon.Provider.Cache;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// CacheUtil represents an internal API to communicate with
	/// Shared Cache and it maintains statistical information
	/// about various actions. This API is used in client as in server mode.
	/// Upon replication modus its maintain clients to resend messages to 
	/// various server nodes based on configuration. The list with 
	/// configured servers will be used linear.
	/// </summary>
	internal static class CacheUtil
	{
		public static bool ClientContext = true;

		#region Properties
		/// <summary>
		/// read from configuration if compression is enabled
		/// </summary>
		private static bool compressionEnabled = false;

		/// <summary>
		/// read from configuration the minimum size of objects which have to be compressed.
		/// </summary>
		private static int compressionMinSize = 1;
		#endregion Properties

		#region counters
		/// <summary>Counts the amount of total added objects per instance</summary>
		private static long counterAdd = 0;
		/// <summary>Counts the amount of total received objects per instance</summary>
		private static long counterGet = 0;
		/// <summary>Counts the amount of total removed objects per instance</summary>
		private static long counterRemove = 0;
		/// <summary>Counts the amount of statistics calls per instance</summary>
		private static long counterStatistic = 0;
		/// <summary>count the success get operatiosn</summary>
		private static long counterSuccess = 0;
		/// <summary>Counts the amount of statistics calls per instance</summary>
		private static long counterFailed = 0;
		/// <summary>counts hit which are available on the server.</summary>
		private static long counterHitSuccess = 0;
		/// <summary>counts failed ret. values when the key is not available anymore.</summary>
		private static long counterHitFailed = 0;
		/// <summary>Parameter counts the amount of calls to disabled server nodes</summary>
		private static long counterFailedNodeNotAvailable = 0;
		#endregion

		/// <summary>
		/// Handle each protocol message - its like single point of access for every provided possibility
		/// </summary>
		/// <param name="msg">A <see cref="IndexusMessage"/> object.</param>
		/// <param name="lastUsedServerNodeIp">The last used server node ip, this is mainly used in case of replication configuration.</param>
		/// <returns>
		/// the status if communication where sucessfully or if it failed.
		/// </returns>
		private static bool HandleProtocol(IndexusMessage msg, string lastUsedServerNodeIp)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			try
			{
				bool hostAvailable = false;
				if (ClientContext)
				{
					//new: check if host is available first bevor we send to prevent timeouts!!!
					hostAvailable = Sockets.ManageClientTcpSocketConnectionPoolFactory.CheckPool(msg.Hostname);
				}
				else
				{
					hostAvailable = Sockets.ManageServerTcpSocketConnectionPoolFactory.CheckPool(msg.Hostname);
				}
				if (hostAvailable || msg.Action == IndexusMessage.ActionValue.Ping)
				{
					if (msg.Send())
					{
						Interlocked.Increment(ref counterSuccess);
						return true;
					}
					else
					{
						Interlocked.Increment(ref counterFailed);
						return false;
					}
				}
				else
				{
					// handling replicated caching in case primary node is not available
					if (Provider.Cache.IndexusDistributionCache.SharedCache.ReplicatedServers.Length > 0)
					{
						string current = msg.Hostname;
						bool take = false;
						foreach (IndexusServerSetting item in IndexusDistributionCache.ProviderSection.ReplicatedServers)
						{
							// handling first chain in list
							if(string.IsNullOrEmpty(lastUsedServerNodeIp))
								take = true;
							if (item.IpAddress.Equals(lastUsedServerNodeIp))
							{
								take = true;
								continue;
							}
							if (take)
							{
								msg.Hostname = item.IpAddress;
								bool result = HandleProtocol(msg, item.IpAddress);
								if(!result)
									Interlocked.Increment(ref counterFailed);
								else
									Interlocked.Increment(ref counterSuccess);

								return result;
							}
						}
					}
					Interlocked.Increment(ref counterFailedNodeNotAvailable);
					return false;
				}
			}
			catch (SocketException sEx)
			{
				// upon clientContext and the user defined replication mode the
				// system try to send the message to a different server node
				// Console.WriteLine(sEx.Message + " " + sEx.SocketErrorCode + " " + sEx.ErrorCode);
				if (ClientContext)
				{
					// doesn't matter if we are using replicated caching or distributed caching specfic server node need to be 
					// disabled until the client enable it automatically again.
					Sockets.ManageClientTcpSocketConnectionPoolFactory.DisablePool(msg.Hostname);

					// in case first node of replicated servers is not available we receive an exception which 
					// will force the program to move to next available node until it found one which is available
					if (Provider.Cache.IndexusDistributionCache.SharedCache.ReplicatedServers.Length > 0)
					{
						string current = msg.Hostname;
						bool take = false;
						foreach (IndexusServerSetting item in IndexusDistributionCache.ProviderSection.ReplicatedServers)
						{
							// handling first chain in list
							if (string.IsNullOrEmpty(lastUsedServerNodeIp))
								take = true;
							if (item.IpAddress.Equals(lastUsedServerNodeIp))
							{
								take = true;
								continue;
							}
							if (take)
							{
								msg.Hostname = item.IpAddress;
								bool result = HandleProtocol(msg, item.IpAddress);
								if (!result)
									Interlocked.Increment(ref counterFailed);
								else
									Interlocked.Increment(ref counterSuccess);

								return result;
							}
						}
					}
					else
					{
						Interlocked.Increment(ref counterFailed);
						return false;
					}
				}
				else
				{
					Sockets.ManageServerTcpSocketConnectionPoolFactory.DisablePool(msg.Hostname);					
				}
				return false;
			}
			catch (Exception ex)
			{
				Interlocked.Increment(ref counterFailed);
				//if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
				//{
					Handler.LogHandler.Fatal(string.Format("Unhandled Exception appears: {0} \n{1}", ex.Message, ex.StackTrace));
					Console.WriteLine(string.Format("Unhandled Exception appears: {0} \n{1}", ex.Message, ex.StackTrace));
				//}
				return false;
			}
		}

		/// <summary>
		/// Pings the specified server node based on provided host.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>the status if communication where sucessfully or if it failed.</returns>
		internal static bool Ping(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			using (IndexusMessage msg = new IndexusMessage(Handler.Unique.NextUniqueId(), IndexusMessage.ActionValue.Ping))
			{
				if (!string.IsNullOrEmpty(host))
				{
					msg.Hostname = host;
				}
				if (!ClientContext)
				{
					msg.ClientContext = false;
					msg.Status = IndexusMessage.StatusValue.ReplicationRequest;
				}
				return HandleProtocol(msg, string.Empty);
			}
		}

		/// <summary>
		/// Clear cache on provided server node.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>the status if communication where sucessfully or if it failed.</returns>
		internal static bool Clear(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			using (IndexusMessage msg = new IndexusMessage(Handler.Unique.NextUniqueId(), IndexusMessage.ActionValue.RemoveAll))
			{
				if (!string.IsNullOrEmpty(host))
				{
					msg.Hostname = host;
				}
				return HandleProtocol(msg, string.Empty);
			}
		}

		/// <summary>
		/// Retrieve a List of <see cref="string"/> of all available key's on specific server node.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>
		/// the status if communication where sucessfully or if it failed.
		/// </returns>
		internal static List<string> GetAllKeys(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			using (IndexusMessage msg = new IndexusMessage(Handler.Unique.NextUniqueId(), IndexusMessage.ActionValue.GetAllKeys))
			{
				if (!string.IsNullOrEmpty(host))
				{
					msg.Hostname = host;
				}

				if (!ClientContext)
				{
					msg.ClientContext = false;
					msg.Status = IndexusMessage.StatusValue.ReplicationRequest;
				}

				HandleProtocol(msg, string.Empty);
				return msg.Payload != null ? Formatters.Serialization.BinaryDeSerialize<List<string>>(msg.Payload) : new List<string>();
			}
		}

		/// <summary>
		/// Adding an item to cache.
		/// </summary>
		/// <param name="msg">The protocol message as <see cref="IndexusMessage"/> object.</param>
		/// <returns>the status if communication where sucessfully or if it failed.</returns>
		internal static bool Add(IndexusMessage msg)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Interlocked.Increment(ref counterAdd);

			// handling compression
			if (compressionEnabled)
			{
				if (compressionMinSize > -1 && msg.Payload != null && msg.Payload.Length >= compressionMinSize)
				{
					msg.Payload = Formatters.Compression.Compress(msg.Payload);
				}
			}

			return HandleProtocol(msg, string.Empty);
		}

		/// <summary>
		/// Removes the specified MSG.
		/// </summary>
		/// <param name="msg">The protocol message as <see cref="IndexusMessage"/> object.</param>
		/// <returns>the status if communication where sucessfully or if it failed.</returns>
		internal static bool Remove(IndexusMessage msg)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Interlocked.Increment(ref counterRemove);
			return HandleProtocol(msg, string.Empty);
		}

		/// <summary>
		/// Gets the specified MSG.
		/// </summary>
		/// <param name="msg">The protocol message as <see cref="IndexusMessage"/> object.</param>
		/// <returns>the status if communication where sucessfully or if it failed.</returns>
		internal static bool Get(IndexusMessage msg)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Interlocked.Increment(ref counterGet);

			if (!ClientContext)
			{
				msg.ClientContext = false;
				msg.Status = IndexusMessage.StatusValue.ReplicationRequest;
			}

			if (HandleProtocol(msg, string.Empty))
			{
				if (msg.Payload != null)
				{
					Interlocked.Increment(ref counterHitSuccess);
				}
				else
				{
					Interlocked.Increment(ref counterHitFailed);
				}


				// handling compression
				if (compressionEnabled)
				{
					if (compressionMinSize > -1 && Formatters.Compression.CheckHeader(msg.Payload))
					{
						msg.Payload = Formatters.Compression.Decompress(msg.Payload);
					}
				}
				return true;
			}
			else
			{
				Interlocked.Increment(ref counterHitFailed);
				if (!ClientContext)
					return false;
				return true;
			}
		}

		/// <summary>
		/// Receive statistics from server node based on provided host.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>an object of type <see cref="IndexusStatistic"/></returns>
		internal static IndexusStatistic Statistic(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Interlocked.Increment(ref counterStatistic);

			IndexusStatistic result = new IndexusStatistic();
			IndexusMessage msg = new IndexusMessage(Handler.Unique.NextUniqueId(), IndexusMessage.ActionValue.Statistic);

			if (!string.IsNullOrEmpty(host))
			{
				msg.Hostname = host;
			}

			if (HandleProtocol(msg, string.Empty))
			{
				IndexusStatistic serverNodeStatistic = Formatters.Serialization.BinaryDeSerialize<IndexusStatistic>(msg.Payload);
				result.ServiceTotalSize = serverNodeStatistic.ServiceTotalSize;
				result.ServiceAmountOfObjects = serverNodeStatistic.ServiceAmountOfObjects;
				result.ServiceUsageList = serverNodeStatistic.ServiceUsageList;
				result.apiCounterAdd = counterAdd;
				result.apiCounterGet = counterGet;
				result.apiCounterRemove = counterRemove;
				result.apiCounterStatistic = counterStatistic;
				result.apiCounterSuccess = counterSuccess;
				result.apiCounterFailed = counterFailed;
				result.apiHitSuccess = counterHitSuccess;
				result.apiHitFailed = counterHitFailed;
				result.apiCounterFailedNodeNotAvailable = counterFailedNodeNotAvailable;

			}
			else
			{
				return new IndexusStatistic(
					counterAdd,
					counterGet,
					counterRemove,
					counterStatistic,
					-1,
					-1,
					-1,
					-1
				);
			}

			return result;
		}

		/// <summary>
		/// Set configuration options for client transfers. In case 
		/// the Cacheutil runs in Client Context it's responsible to
		/// compress and decompress messages.
		/// </summary>
		/// <param name="p">if set to <c>true</c> [p].</param>
		internal static void SetContext(bool p)
		{
			#region Access Log
#if TRACE
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheUtil).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (p)
			{
				compressionEnabled = Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.CompressionEnabled == 1 ? true : false;
				compressionMinSize = Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.CompressionMinSize >= 0 ?
					Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.CompressionMinSize : -1;
			}
			else
			{
				ClientContext = false;
				// server context!! - the server use the cache util
				compressionEnabled = false;
				compressionMinSize = int.MaxValue;
			}
		}
	}
}
