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
// Name:      IndexusSharedCacheProvider.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  30-09-2007 SharedCache.com, rschuetz : removed using (using SharedCache.WinServiceCommon.Configuration.Client;) and replaced with using COM
// Modified:  30-09-2007 SharedCache.com, rschuetz : added property: public "List<string> ServersList" -> changed "public List<string> ServersList"
// Modified:  30-09-2007 SharedCache.com, rschuetz : implemented DefaultExpireTime property getter
// Modified:  30-09-2007 SharedCache.com, rschuetz : public override bool Remove(string key) -> make usage of this.XXXX
// Modified:  30-09-2007 SharedCache.com, rschuetz : implemented the additional "add method" overloads
// Modified:  31-12-2007 SharedCache.com, rschuetz : exported the hash algorithm to an external file since this is also used from the server side.
// Modified:  01-01-2008 SharedCache.com, rschuetz : implmented new methods with Prioirty
// Modified:  02-01-2008 SharedCache.com, rschuetz : implmented GetStats() & GetAllKeys();
// Modified:  02-01-2008 SharedCache.com, rschuetz : added the following Add Methods to serialize the objects outside sharedcache and pass a byte array public abstract void Add(string key, byte[] value);
// Modified:  02-01-2008 SharedCache.com, rschuetz : added the following Add Methods to serialize the objects outside sharedcache and pass a byte array public abstract void Add(string key, byte[] value, DateTime expires);
// Modified:  02-01-2008 SharedCache.com, rschuetz : added the following Add Methods to serialize the objects outside sharedcache and pass a byte array public abstract void Add(string key, byte[] value, DateTime expires, string host);
// Modified:  02-01-2008 SharedCache.com, rschuetz : added the following Add Methods to serialize the objects outside sharedcache and pass a byte array public abstract void Add(string key, byte[] value, DateTime expires, IndexusMessage.CacheItemPriority priority);
// Modified:  02-01-2008 SharedCache.com, rschuetz : added the following Add Methods to serialize the objects outside sharedcache and pass a byte array public abstract void Add(string key, byte[] value, DateTime expires, string host, IndexusMessage.CacheItemPriority priority);
// Modified:  02-01-2008 SharedCache.com, rschuetz : to receive you can simply use the generic option public abstract T Get<T>(string key);  -> no new Get method added
// Modified:  04-01-2008 SharedCache.com, rschuetz : introduction on cache provider to clear() all cache data with one single call instead to iterate over all key's.
// Modified:  13-01-2008 SharedCache.com, rschuetz : implemented Count method
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using COM = SharedCache.WinServiceCommon;
using System.Reflection;


namespace SharedCache.WinServiceCommon.Provider.Cache
{
	/// <summary>
	/// Implementing a provider for Shared Cache based on
	/// Microsofts Provider model.
	/// </summary>
	public class IndexusSharedCacheProvider : IndexusProviderBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IndexusSharedCacheProvider"/> class.
		/// </summary>
		public IndexusSharedCacheProvider()
		{
			CacheUtil.SetContext(true);
		}

		#region Variables & Properties

		/// <summary>
		/// For multi get no specific key is required so 
		/// use a constant to represent the key value.
		/// </summary>
		private const string MultiGetKey = "MG";
		private const string MultiAddKey = "MA";
		private const string MultiDelKey = "MD";
		private const string RegexRemoveKey = "RRK";
		private const string RegexGetItems = "RGI";
		private const string VersionClr = "CLR";
		private const string VersionSharedCache = "VSC";
		private const string AbsoluteTimeExpiration = "ATE";

		/// <summary>
		/// a list which represents only configured server node ip addresses.
		/// </summary>
		private static List<string> serverIp;

		private static List<string> replicatedServerIp;
		private static List<Configuration.Client.IndexusServerSetting> replicatedServerList;
		/// <summary>
		/// A list of all available server <see cref="COM.Configuration.Client.IndexusServerSetting"/> configured nodes
		/// with the provided key.
		/// </summary>
		private static List<Configuration.Client.IndexusServerSetting> serverList;
		/// <summary>
		/// Retrieve amount of configured server nodes.
		/// </summary>
		/// <value>The count.</value>
		public override long Count
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.Servers != null && this.Servers.Length > 0)
					return (long)this.Servers.Length;
				else
					return 0;

			}
		}
		/// <summary>
		/// Retrieve configured server nodes as an array of <see cref="string"/>
		/// </summary>
		/// <value>The servers.</value>
		public override string[] Servers
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (serverIp == null)
				{
					serverIp = new List<string>();
					foreach (Configuration.Client.IndexusServerSetting server in IndexusDistributionCache.ProviderSection.Servers)
					{
						serverIp.Add(server.IpAddress);
					}
				}
				return serverIp.ToArray();
			}
		}
		/// <summary>
		/// Retrieve configured server nodes configuration as a <see cref="List"/>. This
		/// is provides the Key and IPAddress of each item in the configuration section.
		/// </summary>
		/// <value>The servers list.</value>
		public override List<Configuration.Client.IndexusServerSetting> ServersList
		{
			get
			{
				if (serverList == null)
				{
					serverList = new List<COM.Configuration.Client.IndexusServerSetting>();
					foreach (COM.Configuration.Client.IndexusServerSetting server in IndexusDistributionCache.ProviderSection.Servers)
					{
						serverList.Add(server);
					}
				}
				return serverList;
			}
		}

		/// <summary>
		/// Retrieve replication server nodes configuration as an array of <see cref="string"/>. This
		/// is provides the Key and IPAddress of each item in the configuration section.
		/// </summary>
		/// <value>
		/// An array of <see cref="string"/> with all configured replicated servers.
		/// </value>
		public override string[] ReplicatedServers
		{
			get {
				if (replicatedServerIp == null)
				{
					replicatedServerIp = new List<string>();
					foreach (Configuration.Client.IndexusServerSetting server in IndexusDistributionCache.ProviderSection.ReplicatedServers)
					{
						replicatedServerIp.Add(server.IpAddress);
					}
				}
				return replicatedServerIp.ToArray();
			}
		}

		/// <summary>
		/// Retrieve replication server nodes configuration as a <see cref="List"/>. This
		/// is provides the Key and IPAddress of each item in the configuration section.
		/// </summary>
		/// <value>
		/// A List of <see cref="string"/> with all configured replicated servers.
		/// </value>
		public override List<Configuration.Client.IndexusServerSetting> ReplicatedServersList
		{
			get {
				if (replicatedServerList == null)
				{
					replicatedServerList = new List<Configuration.Client.IndexusServerSetting>();
					foreach (Configuration.Client.IndexusServerSetting server in IndexusDistributionCache.ProviderSection.ReplicatedServers)
					{
						replicatedServerList.Add(server);
					}
				}
				return replicatedServerList;
			}
		}
		
		#region Property: Hashing
		private static Enums.HashingAlgorithm hashing = Enums.HashingAlgorithm.Hashing;

		/// <summary>
		/// Gets the Hashing
		/// </summary>
		public static Enums.HashingAlgorithm Hashing
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				try
				{
					hashing = Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.HashingAlgorithm;						
				}
				catch (Exception)
				{
					Console.WriteLine(string.Format(@"Could not read configuration for Hashing , recheck your app.config / web.config"));
					Handler.LogHandler.Fatal(string.Format(@"Could not read configuration for Hashing , recheck your app.config / web.config"));
				}
				return hashing;
			}
		}
		#endregion
		#endregion
		
		#region Add
		/// <summary>
		/// Adding an item to cache with all possibility options. All overloads are using this
		/// method to add items based on various provided variables. e.g. expire date time,
		/// item priority or to a specific host
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		/// <param name="action">The action is always Add item to cache. See also <see cref="IndexusMessage.ActionValue"/> options.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		/// <param name="status">The status of the request. See also <see cref="IndexusMessage.StatusValue"/></param>
		/// <param name="host">The host, represents the specific server node.</param>
		internal override void Add(string key, byte[] payload, DateTime expires, 
			IndexusMessage.ActionValue action, 
			IndexusMessage.CacheItemPriority prio, 
			IndexusMessage.StatusValue status, 
			string host)
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			if(string.IsNullOrEmpty(host))
				throw new ArgumentException("host parameter must be defined - simply use GetServerForKey(key)", host);

			using (IndexusMessage msg = new IndexusMessage())
			{
				msg.Key = key;
				msg.Payload = payload;
				msg.Expires = expires;
				msg.Action = action;
				msg.ItemPriority = prio;
				msg.Status = status;
				msg.Hostname = host;

				CacheUtil.Add(msg);
			}			
		}

		#region Public Add Overloads
		#region basic add
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and
		/// DateTime.MaxValue. Items are only get cleared from cache in case
		/// max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		public override void Add(string key, object payload)
		{			
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			byte[] array = Formatters.Serialization.BinarySerialize(payload);
			this.Add(key, array);
		}

		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and
		/// DateTime.MaxValue. Items are only get cleared from cache in case
		/// max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		public override void Add(string key, byte[] payload)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.Add(key, payload, DateTime.MaxValue, IndexusMessage.ActionValue.Add, IndexusMessage.CacheItemPriority.Normal, IndexusMessage.StatusValue.Request, this.GetServerForKey(key));
		}
		#endregion basic add

		#region overlaod with item expiration
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items
		/// with provided <see cref="DateTime"/>.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		public override void Add(string key, object payload, DateTime expires)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			byte[] array = Formatters.Serialization.BinarySerialize(payload);
			this.Add(key, array, expires);
		}
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items
		/// with provided <see cref="DateTime"/>.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		public override void Add(string key, byte[] payload, DateTime expires)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.Add(key, payload, expires, IndexusMessage.ActionValue.Add, IndexusMessage.CacheItemPriority.Normal, IndexusMessage.StatusValue.Request, this.GetServerForKey(key));
		}
		#endregion overlaod with item expiration

		#region overlaod with priority
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// DateTime.MaxValue. Items are only get cleared from cache in case max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public override void Add(string key, object payload, IndexusMessage.CacheItemPriority prio)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			byte[] array = Formatters.Serialization.BinarySerialize(payload);
			this.Add(key, array, prio);
		}
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// DateTime.MaxValue. Items are only get cleared from cache in case max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public override void Add(string key, byte[] payload, IndexusMessage.CacheItemPriority prio)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.Add(key, payload, DateTime.MaxValue, IndexusMessage.ActionValue.Add, prio, IndexusMessage.StatusValue.Request, this.GetServerForKey(key));
		}
		#endregion overlaod with priority

		#region overlaod specific host
		/// <summary>
		/// Adding an item to specific cache node. It let user to control on which server node the item will be placed.
		/// Items are added with normal priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// DateTime.MaxValue <see cref="DateTime"/>. Items are only get cleared from cache in case max. cache factor
		/// arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="host">The host represents the ip address of a server node.</param>
		public override void Add(string key, object payload, string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			byte[] array = Formatters.Serialization.BinarySerialize(payload);
			this.Add(key, array, host);
		}
		/// <summary>
		/// Adding an item to specific cache node. It let user to control on which server node the item will be placed.
		/// Items are added with normal priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// DateTime.MaxValue <see cref="DateTime"/>. Items are only get cleared from cache in case max. cache factor
		/// arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="host">The host represents the ip address of a server node.</param>
		public override void Add(string key, byte[] payload, string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.Add(key, payload, DateTime.MaxValue, IndexusMessage.ActionValue.Add, IndexusMessage.CacheItemPriority.Normal, IndexusMessage.StatusValue.Request, host);
		}
		#endregion overlaod specific host

		#region overlaod item expiration and priority
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items with provided <see cref="DateTime"/>.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public override void Add(string key, object payload, DateTime expires, IndexusMessage.CacheItemPriority prio)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			byte[] array = Formatters.Serialization.BinarySerialize(payload);
			this.Add(key, array, expires, prio);
		}
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items with provided <see cref="DateTime"/>.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public override void Add(string key, byte[] payload, DateTime expires, IndexusMessage.CacheItemPriority prio)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.Add(key, payload, expires, IndexusMessage.ActionValue.Add, prio, IndexusMessage.StatusValue.Request, this.GetServerForKey(key));
		}
		#endregion overlaod specific host
		#endregion Public Add Overloads

		#endregion Add

		/// <summary>
		/// This Method extends item time to live.
		/// </summary>
		/// <remarks>WorkItem Request: http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=6129</remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		public override void ExtendTtl(string key, DateTime expires)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.Add(key, null, expires, IndexusMessage.ActionValue.ExtendTtl, IndexusMessage.CacheItemPriority.None, IndexusMessage.StatusValue.Request, this.GetServerForKey(key));
		}

		/// <summary>
		/// Return Servers CLR (Common Language Runtime), this is needed to decide which 
		/// Hashing codes can be used.
		/// </summary>
		/// <returns>CLR (Common Language Runtime) version number as <see cref="string"/> e.g. xxxx.xxxx.xxxx.xxxx</returns>
		public override IDictionary<string, string> ServerNodeVersionClr()
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			IDictionary<string, string> result = new Dictionary<string, string>();

			foreach (string item in this.Servers)
			{
				using (IndexusMessage msg = new IndexusMessage())
				{
					msg.Hostname = item;
					msg.Key = VersionClr;
					msg.Status = IndexusMessage.StatusValue.Request;
					msg.Action = IndexusMessage.ActionValue.VersionNumberClr;
					msg.Payload = null;
					if(CacheUtil.Get(msg) && msg.Payload != null)
					{
						string clrVersionNumber = Formatters.Serialization.BinaryDeSerialize<string>(msg.Payload);
						if (!string.IsNullOrEmpty(clrVersionNumber))
						{
							result.Add(item, clrVersionNumber);
						}
						else
						{ 
							// TODO: LOG missing data from server node!!!
						}						
					}					
				}	
			}

			return result;
		}
		/// <summary>
		/// Returns current build version of Shared Cache
		/// </summary>
		/// <returns>Shared Cache version number as <see cref="string"/> e.g. xxxx.xxxx.xxxx.xxxx</returns>
		public override IDictionary<string, string> ServerNodeVersionSharedCache()
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			IDictionary<string, string> result = new Dictionary<string, string>();

			foreach (string item in this.Servers)
			{
				using (IndexusMessage msg = new IndexusMessage())
				{
					msg.Hostname = item;
					msg.Key = VersionSharedCache;
					msg.Status = IndexusMessage.StatusValue.Request;
					msg.Action = IndexusMessage.ActionValue.VersionNumberSharedCache;
					msg.Payload = null;
					if (CacheUtil.Get(msg) && msg.Payload != null)
					{
						string clrVersionNumber = Formatters.Serialization.BinaryDeSerialize<string>(msg.Payload);
						if (!string.IsNullOrEmpty(clrVersionNumber))
						{
							result.Add(item, clrVersionNumber);
						}
						else
						{
							// TODO: LOG missing data from server node!!!
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the absolute time expiration of items within cache nodes
		/// </summary>
		/// <param name="keys">A list with keys of type <see cref="string"/></param>
		/// <returns>A IDictionary&lg;<see cref="string"/>, <see cref="DateTime"/>> were each key has its expiration absolute DateTime</returns>

		public override IDictionary<string, DateTime> GetAbsoluteTimeExpiration(List<string> keys)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			// for each server node we split up the keys
			Dictionary<string, List<string>> splitter = new Dictionary<string, List<string>>();
			Dictionary<string, DateTime> result = new Dictionary<string, DateTime>();
			foreach (string server in this.Servers)
			{
				splitter.Add(server, new List<string>());
			}

			foreach (string k in keys)
			{
				splitter[this.GetServerForKey(k)].Add(k);
			}

			foreach (string server in splitter.Keys)
			{
				// evaluate only to send messages to servers 
				// which really have items - prevent roundtrips
				if (splitter[server].Count > 0)
				{
					using (IndexusMessage msg = new IndexusMessage())
					{
						msg.Hostname = server;
						msg.Key = AbsoluteTimeExpiration;
						msg.Action = IndexusMessage.ActionValue.GetAbsoluteTimeExpiration;
						msg.Payload = Formatters.Serialization.BinarySerialize(splitter[server]);
						if (CacheUtil.Get(msg) && msg.Payload != null)
						{
							IDictionary<string, DateTime> partialResult =
								Formatters.Serialization.BinaryDeSerialize<IDictionary<string, DateTime>>(msg.Payload);

							foreach (KeyValuePair<string, DateTime> item in partialResult)
							{
								result.Add(item.Key, item.Value);
							}
						}
					}
				}
			}
			return result;
		}
		
		#region Get
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key for cache item</param>
		/// <returns>Returns received item as casted object T</returns>
		public override T Get<T>(string key)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			using (IndexusMessage msg = new IndexusMessage())
			{
				msg.Hostname = this.GetServerForKey(key);
				msg.Key = key;
				msg.Action = IndexusMessage.ActionValue.Get;
				if (CacheUtil.Get(msg) && msg.Payload != null)
				{
					return Formatters.Serialization.BinaryDeSerialize<T>(msg.Payload);
				}
				else
				{
					return default(T);
				}
			}
		}

		#region Overloads Get
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <returns>
		/// Returns received item as casted <see cref="object"/>
		/// </returns>
		public override object Get(string key)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return this.Get<object>(key);
		}
		#endregion Overloads Get		
		#endregion Get		
		
		#region Multi Get
		
		/// <summary>
		/// Based on a list of key's the client receives a dictonary with
		/// all available data depending on the keys.
		/// </summary>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys.</param>
		/// <returns>
		/// A <see cref="IDictionary"/> with <see cref="string"/> and <see cref="byte"/> array element.
		/// </returns>
		public override IDictionary<string, byte[]> MultiGet(List<string> keys)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			// for each server node we split up the keys
			Dictionary<string, List<string>> splitter = new Dictionary<string,List<string>>();
			Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();
			foreach (string server in this.Servers)
			{ 
				splitter.Add(server, new List<string>());
			}

			foreach (string k in keys)
			{
				splitter[this.GetServerForKey(k)].Add(k);
			}

			foreach (string server in splitter.Keys)
			{
				// evaluate only to send messages to servers 
				// which really have items.
				if (splitter[server].Count > 0)
				{
					using (IndexusMessage msg = new IndexusMessage())
					{
						msg.Hostname = server;
						msg.Key = MultiGetKey;
						msg.Action = IndexusMessage.ActionValue.MultiGet;
						msg.Payload = Formatters.Serialization.BinarySerialize(splitter[server]);
						if (CacheUtil.Get(msg) && msg.Payload != null)
						{
							IDictionary<string, byte[]> partialResult =
								Formatters.Serialization.BinaryDeSerialize<IDictionary<string, byte[]>>(msg.Payload);

							foreach (KeyValuePair<string, byte[]> item in partialResult)
							{
								result.Add(item.Key, item.Value);
							}
						}
					}
				}				
			}
			return result;
		}

		/// <summary>
		/// Adding a bunch of data to the cache, prevents to make several calls
		/// from the client to the server. All data is tranported with
		/// a <see cref="Dictonary"/> with a <see cref="string"/> and <see cref="byte"/>
		/// array combination.
		/// </summary>
		/// <param name="data">The data to add as a <see cref="IDictionary"/></param>
		public override void MultiAdd(IDictionary<string, byte[]> data)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Dictionary<string, IDictionary<string, byte[]>> splitter = new Dictionary<string, IDictionary<string, byte[]>>();
			
			foreach (string server in this.Servers)
			{
				splitter.Add(server, new Dictionary<string, byte[]>());
			}
			foreach (string k in data.Keys)
			{
				splitter[this.GetServerForKey(k)].Add(k, data[k]);
			}
			foreach (string server in splitter.Keys)
			{
				if (splitter[server].Count > 0)
				{
					using (IndexusMessage msg = new IndexusMessage())
					{
						msg.Action = IndexusMessage.ActionValue.MultiAdd;
						msg.Hostname = server;
						msg.Key = MultiAddKey;
						msg.Payload = Formatters.Serialization.BinarySerialize(splitter[server]);
						if (CacheUtil.Add(msg))
						{
							return;
						}
						else
						{
							// write something to log ??
						}
					}
				}
			}

		}

		/// <summary>
		/// Delete a bunch of data from the cache. This prevents several calls from
		/// the client to the server. Only one single call is done with all relevant
		/// key's for the server node.
		/// </summary>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys to delete</param>
		public override void MultiDelete(List<string> keys)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			Dictionary<string, bool> nodeResult = new Dictionary<string, bool>();
			
			Dictionary<string, List<string>> splitter = new Dictionary<string, List<string>>();
			Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();
			foreach (string server in this.Servers)
			{
				splitter.Add(server, new List<string>());
			}

			foreach (string k in keys)
			{
				splitter[this.GetServerForKey(k)].Add(k);
			}

			foreach (string server in splitter.Keys)
			{
				if (splitter[server].Count > 0)
				{
					using (IndexusMessage msg = new IndexusMessage())
					{
						msg.Action = IndexusMessage.ActionValue.MultiDelete;
						msg.Hostname = server;
						msg.Key = MultiDelKey;
						msg.Payload = Formatters.Serialization.BinarySerialize(splitter[server]);
						bool tmpResult = CacheUtil.Remove(msg);
						nodeResult.Add(msg.Hostname, tmpResult);
					}
				}
			}

			foreach (KeyValuePair<string, bool> res in nodeResult)
			{
				if (!res.Value)
				{ 
					Handler.LogHandler.Error(string.Format("MultiDelete could not be executed successfully at node: {0}", res.Key.ToString()));
				}
			}
		}

		#endregion Multi Get

		#region Remove
		/// <summary>
		/// Remove cache item with provided key.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		public override void Remove(string key)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			using (IndexusMessage msg = new IndexusMessage())
			{
				msg.Hostname = this.GetServerForKey(key);
				msg.Key = key;
				msg.Action = IndexusMessage.ActionValue.Remove;
				CacheUtil.Remove(msg);
			}
		}
		#endregion Remove

		#region Other Methods
		/// <summary>
		/// Remove Cache Items on server node based on regular expression. Each item which matches
		/// will be automatically removed from each server.
		/// </summary>
		/// <param name="regularExpression">The regular expression.</param>
		/// <returns></returns>
		public override bool RegexRemove(string regularExpression)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Dictionary<string, bool> result = new Dictionary<string, bool>();
			bool overallResult = true;

			foreach(Configuration.Client.IndexusServerSetting setting in this.ServersList)
			{
				using (IndexusMessage msg = new IndexusMessage())
				{
					msg.Hostname = setting.IpAddress;
					msg.Key = RegexRemoveKey;
					msg.Action = IndexusMessage.ActionValue.RegexRemove;
					msg.Payload = Encoding.UTF8.GetBytes(regularExpression);
					bool nodeResult = CacheUtil.Remove(msg);
					result.Add(msg.Hostname, nodeResult);
				}
			}
			
			foreach (KeyValuePair<string, bool> res in result)
			{
				if (!res.Value)
				{ 
					overallResult = false;
					Handler.LogHandler.Error(string.Format("RegexRemove could not be executed successfully at node: {0}", res.Key.ToString()));
				}
			}

			return overallResult;
		}
		/// <summary>
		/// Returns items from cache node based on provided pattern.
		/// </summary>
		/// <param name="regularExpression">The regular expression.</param>
		/// <returns>
		/// An IDictionary with <see cref="string"/> and <see cref="byte"/> array with all founded elementes
		/// </returns>
		public override IDictionary<string, byte[]> RegexGet(string regularExpression)
		{
			IDictionary<string, byte[]> result = new Dictionary<string, byte[]>();

			foreach (Configuration.Client.IndexusServerSetting setting in this.ServersList)
			{
				using (IndexusMessage msg = new IndexusMessage())
				{
					msg.Hostname = setting.IpAddress;
					msg.Key = RegexGetItems;
					msg.Action = IndexusMessage.ActionValue.RegexGet;
					msg.Payload = Encoding.UTF8.GetBytes(regularExpression);
					if (CacheUtil.Get(msg))
					{
						if (msg.Status == IndexusMessage.StatusValue.Request)
						{ 

							// no server roundtrip happend
							return result;
						}
						IDictionary<string, byte[]> nodeResult = Formatters.Serialization.BinaryDeSerialize<IDictionary<string, byte[]>>(msg.Payload);
						if (nodeResult != null)
						{
							foreach (KeyValuePair<string, byte[]> item in nodeResult)
							{
								result.Add(item);
							}
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Pings the specified host.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns>
		/// if the server is available then it returns true otherwise false.
		/// </returns>
		public override bool Ping(string host)
		{
			return CacheUtil.Ping(host);
		}
		/// <summary>
		/// Force each configured cache server node to clear the cache.
		/// </summary>
		public override void Clear()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			foreach (Configuration.Client.IndexusServerSetting setting in this.ServersList)
			{
				CacheUtil.Clear(setting.IpAddress);
			}
		}
		/// <summary>
		/// Retrieve a list with all key which are available on all cofnigured server nodes.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>
		/// A List of <see cref="string"/> with all available keys.
		/// </returns>
		public override List<string> GetAllKeys(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return CacheUtil.GetAllKeys(host);
		}
		/// <summary>
		/// Retrieve a list with all key which are available on cache.
		/// </summary>
		/// <returns>
		/// A List of <see cref="string"/> with all available keys.
		/// </returns>
		public override List<string> GetAllKeys()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			List<string> result = new List<string>();
			foreach (Configuration.Client.IndexusServerSetting setting in this.ServersList)
			{
				result.AddRange(this.GetAllKeys(setting.IpAddress));
			}
			return result;
		}
		/// <summary>
		/// Retrieve statistic information <see cref="IndexusStatistic"/> from specific
		/// server based on provided host.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>
		/// an <see cref="IndexusStatistic"/> object
		/// </returns>
		public override IndexusStatistic GetStats(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			IndexusStatistic result = new IndexusStatistic();
			#region
			// retrieve for specific host all data
			IndexusStatistic remoteStat = CacheUtil.Statistic(host);

			// read the local data, need to read them upon the last item, otherwise the amount 
			// of stats would be missing the server calls.
			result.apiCounterAdd = remoteStat.apiCounterAdd;
			result.apiCounterFailed = remoteStat.apiCounterFailed;
			result.apiCounterGet = remoteStat.apiCounterGet;
			result.apiCounterRemove = remoteStat.apiCounterRemove;
			result.apiCounterSuccess = remoteStat.apiCounterSuccess;
			result.apiCounterStatistic = remoteStat.apiCounterStatistic;
			result.apiHitSuccess = remoteStat.apiHitSuccess;
			result.apiHitFailed = remoteStat.apiHitFailed;

			// creating a node stat object
			ServerStats nodeStat = new ServerStats(
					host,
					remoteStat.ServiceAmountOfObjects,
					remoteStat.ServiceTotalSize,
					remoteStat.ServiceUsageList
				);

			// adding the node to the stat object.
			result.NodeDate.Add(nodeStat);

			// comulate the data from each node.
			result.ServiceAmountOfObjects += remoteStat.ServiceAmountOfObjects;
			result.ServiceTotalSize += remoteStat.ServiceTotalSize;

			// if the remote object is not null or empty nothing has to take over and 
			// printout gone be empty then the cache is empty
			if (remoteStat.ServiceUsageList != null && remoteStat.ServiceUsageList.Count > 0)
			{
				// init inner list of the local object, first time 
				// this is not initialized.
				if (result.ServiceUsageList == null)
					result.ServiceUsageList = new Dictionary<string, long>();

				// need to iterate over every key value pair and adding it into 
				// general list to support a general overview of top values from
				// all servers together.
				foreach (KeyValuePair<string, long> de in remoteStat.ServiceUsageList)
				{
					// upon replication the key's are available on all server
					if (!result.ServiceUsageList.ContainsKey(de.Key))
					{
						result.ServiceUsageList.Add(de.Key, de.Value);
					}

				}
				// sorting comulated data from each node.
				Handler.Generic.Util.SortDictionaryDesc(result.ServiceUsageList);
			}
			#endregion
			return result;
		}
		/// <summary>
		/// Retrieve all statistic information <see cref="IndexusStatistic"/> from each configured
		/// server as one item.
		/// </summary>
		/// <returns>
		/// an aggrigated <see cref="IndexusStatistic"/> object with all server statistics
		/// </returns>
		public override IndexusStatistic GetStats()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			IndexusStatistic result = new IndexusStatistic();
			int cntr = 0;

			foreach (COM.Configuration.Client.IndexusServerSetting setting in this.ServersList)
			{
				string configuredHost = setting.IpAddress;
				#region
				// retrieve for specific host all data
				IndexusStatistic remoteStat = COM.CacheUtil.Statistic(configuredHost);

				// read the local data, need to read them upon the last item, otherwise the amount 
				// of stats would be missing the server calls.
				if (cntr == (this.ServersList.Count - 1))
				{
					result.apiCounterAdd = remoteStat.apiCounterAdd;
					result.apiCounterFailed = remoteStat.apiCounterFailed;
					result.apiCounterGet = remoteStat.apiCounterGet;
					result.apiCounterRemove = remoteStat.apiCounterRemove;
					result.apiCounterSuccess = remoteStat.apiCounterSuccess;
					result.apiCounterStatistic = remoteStat.apiCounterStatistic;
					result.apiHitSuccess = remoteStat.apiHitSuccess;
					result.apiHitFailed = remoteStat.apiHitFailed;
					result.apiCounterFailedNodeNotAvailable = remoteStat.apiCounterFailedNodeNotAvailable;
				}
				cntr++;

				// creating a node stat object
				ServerStats nodeStat = new ServerStats(
						configuredHost,
						remoteStat.ServiceAmountOfObjects,
						remoteStat.ServiceTotalSize,
						remoteStat.ServiceUsageList
					);

				// adding the node to the stat object.
				result.NodeDate.Add(nodeStat);

				// comulate the data from each node.
				result.ServiceAmountOfObjects += remoteStat.ServiceAmountOfObjects;
				result.ServiceTotalSize += remoteStat.ServiceTotalSize;

				// if the remote object is not null or empty nothing has to take over and 
				// printout gone be empty then the cache is empty
				if (remoteStat.ServiceUsageList != null && remoteStat.ServiceUsageList.Count > 0)
				{
					// init inner list of the local object, first time 
					// this is not initialized.
					if (result.ServiceUsageList == null)
						result.ServiceUsageList = new Dictionary<string, long>();

					// need to iterate over every key value pair and adding it into 
					// general list to support a general overview of top values from
					// all servers together.
					foreach (KeyValuePair<string, long> de in remoteStat.ServiceUsageList)
					{
						// upon replication the key's are available on all server
						if (!result.ServiceUsageList.ContainsKey(de.Key))
						{
							result.ServiceUsageList.Add(de.Key, de.Value);
						}

					}
					// sorting comulated data from each node.
					Handler.Generic.Util.SortDictionaryDesc(result.ServiceUsageList);
				}
				#endregion
			}
			return result;
		}
		/// <summary>
		/// Return the ip server which handles this key by the hashcode of the key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// the specific host ip as a <see cref="string"/> object.
		/// </returns>
		public override string GetServerForKey(string key)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			switch (Hashing)
			{
				// option - yes -> 0ms -> hash = SharedCache.WinServiceCommon.Hashing.GetHashCodeWrapper.GetHashCode(values[i]);
				// option - yes -> 1ms -> hash = SharedCache.WinServiceCommon.Hashing.Ketama.Generate(values[i]);
				// option - yes -> 0ms -> hash = BitConverter.ToInt32(SharedCache.WinServiceCommon.Hashing.FnvHash32.Create().ComputeHash(Encoding.Unicode.GetBytes(values[i])), 0);
				// option - yes -> 0ms -> hash = BitConverter.ToInt32(SharedCache.WinServiceCommon.Hashing.FnvHash64.Create().ComputeHash(Encoding.Unicode.GetBytes(values[i])), 0);

				case Enums.HashingAlgorithm.Hashing:
					{
						return Servers[COM.Hashing.Hash.Generate(key, Servers.Length)];
					}
				case Enums.HashingAlgorithm.Ketama:
					{
						int srv = Math.Abs(COM.Hashing.Ketama.Generate(key) % Servers.Length);
						return Servers[srv];
					}
				case Enums.HashingAlgorithm.FvnHash32:
					{
						int srv = Math.Abs(BitConverter.ToInt32(COM.Hashing.FnvHash32.Create().ComputeHash(Encoding.UTF8.GetBytes(key)), 0) % Servers.Length);
						return Servers[srv];
					}
				case Enums.HashingAlgorithm.FvnHash64:
					{
						int srv = Math.Abs(BitConverter.ToInt32(COM.Hashing.FnvHash64.Create().ComputeHash(Encoding.UTF8.GetBytes(key)), 0) % Servers.Length);
						return Servers[srv];
					}
				default:
					return Servers[COM.Hashing.Hash.Generate(key, Servers.Length)];
			}
		}
		#endregion Other Methods


		#region DataContract Extensions
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and
		/// DateTime.MaxValue. Items are only get cleared from cache in case
		/// max. cache factor arrived or the cache get refreshed.
		/// Data get serialized for DataContract.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		public override void DataContractAdd(string key, object payload)
		{ 
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			byte[] array = Formatters.Serialization.DataContractBinarySerialize(payload);
			this.Add(key, array);
		}
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items
		/// with provided <see cref="DateTime"/>.
		/// Data get serialized for DataContract.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		public override void DataContractAdd(string key, object payload, DateTime expires)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			byte[] array = Formatters.Serialization.DataContractBinarySerialize(payload);
			this.Add(key, array, expires);
		}
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// DateTime.MaxValue. Items are only get cleared from cache in case max. cache factor arrived or the cache get refreshed.
		/// Data get serialized for DataContract.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public override void DataContractAdd(string key, object payload, IndexusMessage.CacheItemPriority prio)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			byte[] array = Formatters.Serialization.DataContractBinarySerialize(payload);
			this.Add(key, array, prio);
		}
		/// <summary>
		/// Adding an item to specific cache node. It let user to control on which server node the item will be placed.
		/// Items are added with normal priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// DateTime.MaxValue <see cref="DateTime"/>. Items are only get cleared from cache in case max. cache factor
		/// arrived or the cache get refreshed.
		/// Data get serialized for DataContract.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="host">The host represents the ip address of a server node.</param>
		public override void DataContractAdd(string key, object payload, string host)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			byte[] array = Formatters.Serialization.DataContractBinarySerialize(payload);
			this.Add(key, array, host);
		}
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items with provided <see cref="DateTime"/>.
		/// Data get serialized for DataContract.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public override void DataContractAdd(string key, object payload, DateTime expires, IndexusMessage.CacheItemPriority prio)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			byte[] array = Formatters.Serialization.DataContractBinarySerialize(payload);
			this.Add(key, array, expires, prio);
		}
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key for cache item</param>
		/// <returns>Returns received item as casted object T</returns>
		public override T DataContractGet<T>(string key)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			using (IndexusMessage msg = new IndexusMessage())
			{
				msg.Hostname = this.GetServerForKey(key);
				msg.Key = key;
				msg.Action = IndexusMessage.ActionValue.Get;
				if (CacheUtil.Get(msg) && msg.Payload != null)
				{
					return Formatters.Serialization.DataContractBinaryDeSerialize<T>(msg.Payload);
				}
				else
				{
					return default(T);
				}
			}
		}
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <returns>
		/// Returns received item as casted <see cref="object"/>
		/// </returns>
		public override object DataContractGet(string key)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			return DataContractGet<object>(key);
		}
		/// <summary>
		/// Based on a list of key's the client receives a dictonary with
		/// all available data depending on the keys.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys.</param>
		/// <returns>
		/// A <see cref="IDictionary"/> with <see cref="string"/> and <see cref="byte"/> array element.
		/// </returns>
		public override IDictionary<string, byte[]> DataContractMultiGet(List<string> keys)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			// for each server node we split up the keys
			Dictionary<string, List<string>> splitter = new Dictionary<string, List<string>>();
			Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();
			foreach (string server in this.Servers)
			{
				splitter.Add(server, new List<string>());
			}

			foreach (string k in keys)
			{
				splitter[this.GetServerForKey(k)].Add(k);
			}

			foreach (string server in splitter.Keys)
			{
				// evaluate only to send messages to servers 
				// which really have items.
				if (splitter[server].Count > 0)
				{
					using (IndexusMessage msg = new IndexusMessage())
					{
						msg.Hostname = server;
						msg.Key = MultiGetKey;
						msg.Action = IndexusMessage.ActionValue.MultiGet;
						msg.Payload = Formatters.Serialization.DataContractBinarySerialize(splitter[server]);
						if (CacheUtil.Get(msg) && msg.Payload != null)
						{
							IDictionary<string, byte[]> partialResult =
								Formatters.Serialization.BinaryDeSerialize<IDictionary<string, byte[]>>(msg.Payload);

							foreach (KeyValuePair<string, byte[]> item in partialResult)
							{
								result.Add(item.Key, item.Value);
							}
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Returns items from cache node based on provided pattern.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <param name="regularExpression">The regular expression.</param>
		/// <returns>
		/// An IDictionary with <see cref="string"/> and <see cref="byte"/> array with all founded elementes
		/// </returns>
		public override IDictionary<string, byte[]> DataContractRegexGet(string regularExpression)
		{
			#region Access Log
#if TRACE			
						{
							Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
						}
#endif
			#endregion Access Log
			IDictionary<string, byte[]> result = new Dictionary<string, byte[]>();

			foreach (Configuration.Client.IndexusServerSetting setting in this.ServersList)
			{
				using (IndexusMessage msg = new IndexusMessage())
				{
					msg.Hostname = setting.IpAddress;
					msg.Key = RegexGetItems;
					msg.Action = IndexusMessage.ActionValue.RegexGet;
					msg.Payload = Encoding.UTF8.GetBytes(regularExpression);
					if (CacheUtil.Get(msg))
					{
						if (msg.Status == IndexusMessage.StatusValue.Request)
						{

							// no server roundtrip happend
							return result;
						}
						IDictionary<string, byte[]> nodeResult = Formatters.Serialization.DataContractBinaryDeSerialize<IDictionary<string, byte[]>>(msg.Payload);
						if (nodeResult != null)
						{
							foreach (KeyValuePair<string, byte[]> item in nodeResult)
							{
								result.Add(item);
							}
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Adding a bunch of data to the cache, prevents to make several calls
		/// from the client to the server. All data is tranported with
		/// a <see cref="IDictionary"/> with a <see cref="string"/> and <see cref="byte"/>
		/// array combination.
		/// </summary>
		/// <param name="data">The data to add as a <see cref="IDictionary"/></param>
		public override void DataContractMultiAdd(IDictionary<string, byte[]> data)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Dictionary<string, IDictionary<string, byte[]>> splitter = new Dictionary<string, IDictionary<string, byte[]>>();

			foreach (string server in this.Servers)
			{
				splitter.Add(server, new Dictionary<string, byte[]>());
			}
			foreach (string k in data.Keys)
			{
				splitter[this.GetServerForKey(k)].Add(k, data[k]);
			}
			foreach (string server in splitter.Keys)
			{
				if (splitter[server].Count > 0)
				{
					using (IndexusMessage msg = new IndexusMessage())
					{
						msg.Action = IndexusMessage.ActionValue.MultiAdd;
						msg.Hostname = server;
						msg.Key = MultiAddKey;
						msg.Payload = Formatters.Serialization.DataContractBinarySerialize(splitter[server]);
						if (CacheUtil.Add(msg))
						{
							return;
						}
						else
						{
							Handler.LogHandler.Fatal(@"Could not add data to cache for server: " + server);
						}
					}
				}
			}

		}

		#endregion

	}
}
