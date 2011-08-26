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
// Name:      IndexusProviderBase.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  30-09-2007 SharedCache.com, rschuetz : updated summery + changed from bool to void back
// Modified:  30-09-2007 SharedCache.com, rschuetz : implemented additional overloads
// Modified:  01-01-2008 SharedCache.com, rschuetz : added a method to add data with prioirty
// Modified:  04-01-2008 SharedCache.com, rschuetz : added several Add methods to work with byte[] from the client
// Modified:  04-01-2008 SharedCache.com, rschuetz : change from method name RemoveAll to Clear()
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

// http://www.codeproject.com/useritems/memcached_aspnet.asp

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text;

using CCLIENT = SharedCache.WinServiceCommon.Configuration.Client;
namespace SharedCache.WinServiceCommon.Provider.Cache
{
	/// <summary>
	/// Implements the provider base for Shared Cache, based on Microsoft Provider Model.
	/// <example>
	/// <![CDATA[<?xml version="1.0" encoding="utf-8" ?>]]>
	///	<configSections>
	///	<section name="indexusNetSharedCache" type="SharedCache.WinServiceCommon.Configuration.Client.IndexusProviderSection, SharedCache.WinServiceCommon" />
	///	</configSections>
	/// 
	/// <indexusNetSharedCache defaultProvider="IndexusSharedCacheProvider">
  ///		<servers>
  ///		   <add key="Server1"	ipaddress="10.0.0.3" port="48888" />
	///		   <add key="Server2"	ipaddress="10.0.0.4" port="48888" />
  ///		</servers>
  ///		<providers>
  ///		   <add
  ///		     name="IndexusSharedCacheProvider"
  ///		     type="SharedCache.WinServiceCommon.Provider.Cache.IndexusSharedCacheProvider, SharedCache.WinServiceCommon" />
  ///		 </providers>
	///	</indexusNetSharedCache>
	/// </example>
	/// </summary>
	public abstract class IndexusProviderBase : ProviderBase
	{
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and 
		/// DateTime.MaxValue. Items are only get cleared from cache in case 
		/// max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <remarks>
		/// Data which is add with DataContractXxx() Methods need to receive Data from cache with DataContractGetXxx() Methods.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		public abstract void DataContractAdd(string key, object payload);
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and 
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case 
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items
		/// with provided <see cref="DateTime"/>.
		/// </summary>
		/// <remarks>
		/// Data which is add with DataContractXxx() Methods need to receive Data from cache with DataContractGetXxx() Methods.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		public abstract void DataContractAdd(string key, object payload, DateTime expires);
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// DateTime.MaxValue. Items are only get cleared from cache in case max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <remarks>
		/// Data which is add with DataContractXxx() Methods need to receive Data from cache with DataContractGetXxx() Methods.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public abstract void DataContractAdd(string key, object payload, IndexusMessage.CacheItemPriority prio);
		/// <summary>
		/// Adding an item to specific cache node. It let user to control on which server node the item will be placed. 
		/// Items are added with normal priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// DateTime.MaxValue <see cref="DateTime"/>. Items are only get cleared from cache in case max. cache factor 
		/// arrived or the cache get refreshed.
		/// </summary>
		/// <remarks>
		/// Data which is add with DataContractXxx() Methods need to receive Data from cache with DataContractGetXxx() Methods.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="host">The host represents the ip address of a server node.</param>
		public abstract void DataContractAdd(string key, object payload, string host);
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// provided <see cref="DateTime"/>. Items get cleared from cache in case 
		/// max. cache factor arrived, cache get refreshed or provided <see cref="DateTime"/>
		/// reached provided <see cref="DateTime"/>. The server takes care of items with provided <see cref="DateTime"/>.
		/// </summary>
		/// <remarks>
		/// Data which is add with DataContractXxx() Methods need to receive Data from cache with DataContractGetXxx() Methods.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public abstract void DataContractAdd(string key, object payload, DateTime expires, IndexusMessage.CacheItemPriority prio);
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <remarks>
		/// Data which is add with DataContractXxx() Methods need to receive Data from cache with DataContractGetXxx() Methods.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key for cache item</param>
		/// <returns>Returns received item as casted object T</returns>
		public abstract T DataContractGet<T>(string key);
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <param name="key">The key for cache item</param>
		/// <returns>Returns received item as casted <see cref="object"/></returns>
		public abstract object DataContractGet(string key);
		/// <summary>
		/// Adding a bunch of data to the cache, prevents to make several calls
		/// from the client to the server. All data is tranported with 
		/// a <see cref="IDictionary"/> with a <see cref="string"/> and <see cref="byte"/> 
		/// array combination.
		/// </summary>
		/// <param name="data">The data to add as a <see cref="IDictionary"/></param>
		public abstract void DataContractMultiAdd(IDictionary<string, byte[]> data);
		/// <summary>
		/// Based on a list of key's the client receives a dictonary with 
		/// all available data depending on the keys.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys.</param>
		/// <returns>A <see cref="IDictionary"/> with <see cref="string"/> and <see cref="byte"/> array element.</returns>
		public abstract IDictionary<string, byte[]> DataContractMultiGet(List<string> keys);
		/// <summary>
		/// Returns items from cache node based on provided pattern.
		/// </summary>
		/// <remarks>
		/// Data need to be added to cache over DataContractAdd() methods otherwise the application throws an exception.
		/// </remarks>
		/// <param name="regularExpression">The regular expression.</param>
		/// <returns>An IDictionary with <see cref="string"/> and <see cref="byte"/> array with all founded elementes</returns>
		public abstract IDictionary<string, byte[]> DataContractRegexGet(string regularExpression);


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
		internal abstract void Add(string key, byte[] payload, DateTime expires, IndexusMessage.ActionValue action, IndexusMessage.CacheItemPriority prio, IndexusMessage.StatusValue status, string host);
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and 
		/// DateTime.MaxValue. Items are only get cleared from cache in case 
		/// max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		public abstract void Add(string key, object payload);
		/// <summary>
		/// Adding an item to cache. Items are added with Normal priority and 
		/// DateTime.MaxValue. Items are only get cleared from cache in case 
		/// max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		public abstract void Add(string key, byte[] payload);
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
		public abstract void Add(string key, object payload, DateTime expires);
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
		public abstract void Add(string key, byte[] payload, DateTime expires);
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// DateTime.MaxValue. Items are only get cleared from cache in case max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public abstract void Add(string key, object payload, IndexusMessage.CacheItemPriority prio);
		/// <summary>
		/// Adding an item to cache. Items are added with provided priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// DateTime.MaxValue. Items are only get cleared from cache in case max. cache factor arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="prio">Item priority - See also <see cref="IndexusMessage.CacheItemPriority"/></param>
		public abstract void Add(string key, byte[] payload, IndexusMessage.CacheItemPriority prio);
		/// <summary>
		/// Adding an item to specific cache node. It let user to control on which server node the item will be placed. 
		/// Items are added with normal priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// DateTime.MaxValue <see cref="DateTime"/>. Items are only get cleared from cache in case max. cache factor 
		/// arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="host">The host represents the ip address of a server node.</param>
		public abstract void Add(string key, object payload, string host);
		/// <summary>
		/// Adding an item to specific cache node. It let user to control on which server node the item will be placed. 
		/// Items are added with normal priority <see cref="IndexusMessage.CacheItemPriority"/> and 
		/// DateTime.MaxValue <see cref="DateTime"/>. Items are only get cleared from cache in case max. cache factor 
		/// arrived or the cache get refreshed.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <param name="payload">The payload which is the object itself.</param>
		/// <param name="host">The host represents the ip address of a server node.</param>
		public abstract void Add(string key, byte[] payload, string host);
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
		public abstract void Add(string key, object payload, DateTime expires, IndexusMessage.CacheItemPriority prio);
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
		public abstract void Add(string key, byte[] payload, DateTime expires, IndexusMessage.CacheItemPriority prio);
		/// <summary>
		/// This Method extends items time to live.
		/// </summary>
		/// <remarks>WorkItem Request: http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=6129</remarks>
		/// <param name="key">The key for cache item</param>
		/// <param name="expires">Identify when item will expire from the cache.</param>
		public abstract void ExtendTtl(string key, DateTime expires);
		/// <summary>
		/// Remove cache item with provided key. 
		/// </summary>
		/// <param name="key">The key for cache item</param>
		public abstract void Remove(string key);
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key for cache item</param>
		/// <returns>Returns received item as casted object T</returns>
		public abstract T Get<T>(string key);		
		/// <summary>
		/// Retrieve specific item from cache based on provided key.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <returns>Returns received item as casted <see cref="object"/></returns>
		public abstract object Get(string key);
		/// <summary>
		/// Based on a list of key's the client receives a dictonary with 
		/// all available data depending on the keys.
		/// </summary>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys.</param>
		/// <returns>A <see cref="IDictionary"/> with <see cref="string"/> and <see cref="byte"/> array element.</returns>
		public abstract IDictionary<string, byte[]> MultiGet(List<string> keys);
		/// <summary>
		/// Adding a bunch of data to the cache, prevents to make several calls
		/// from the client to the server. All data is tranported with 
		/// a <see cref="Dictonary"/> with a <see cref="string"/> and <see cref="byte"/> 
		/// array combination.
		/// </summary>
		/// <param name="data">The data to add as a <see cref="IDictionary"/></param>
		public abstract void MultiAdd(IDictionary<string, byte[]> data);
		/// <summary>
		/// Delete a bunch of data from the cache. This prevents several calls from 
		/// the client to the server. Only one single call is done with all relevant 
		/// key's for the server node.
		/// </summary>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys to delete</param>
		public abstract void MultiDelete(List<string> keys);
		/// <summary>
		/// Retrieve amount of configured server nodes.
		/// </summary>
		/// <value>The count.</value>
		public abstract long Count { get;}
		/// <summary>
		/// Retrieve configured server nodes as an array of <see cref="string"/>
		/// </summary>
		/// <value>The servers.</value>
		public abstract string[] Servers { get;}
		/// <summary>
		/// Retrieve configured server nodes configuration as a <see cref="List"/>. This 
		/// is provides the Key and IPAddress of each item in the configuration section.
		/// </summary>
		/// <value>The servers list.</value>
		public abstract List<CCLIENT.IndexusServerSetting> ServersList { get; }
		/// <summary>
		/// Retrieve replication server nodes configuration as an array of <see cref="string"/>. This 
		/// is provides the Key and IPAddress of each item in the configuration section.
		/// </summary>
		/// <value>An array of <see cref="string"/> with all configured replicated servers.</value>
		public abstract string[] ReplicatedServers { get;}
		/// <summary>
		/// Retrieve replication server nodes configuration as a <see cref="List"/>. This 
		/// is provides the Key and IPAddress of each item in the configuration section.
		/// </summary>
		/// <value>A List of <see cref="string"/> with all configured replicated servers.</value>
		public abstract List<CCLIENT.IndexusServerSetting> ReplicatedServersList { get; }
		/// <summary>
		/// Force each configured cache server node to clear the cache.
		/// </summary>
		public abstract void Clear();
		/// <summary>
		/// Retrieve all statistic information <see cref="IndexusStatistic"/> from each configured 
		/// server as one item.
		/// </summary>
		/// <returns>an aggrigated <see cref="IndexusStatistic"/> object with all server statistics</returns>
		public abstract IndexusStatistic GetStats();
		/// <summary>
		/// Retrieve statistic information <see cref="IndexusStatistic"/> from specific  
		/// server based on provided host.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>an <see cref="IndexusStatistic"/> object</returns>
		public abstract IndexusStatistic GetStats(string host);
		/// <summary>
		/// Evaluate the correct server node for provided key.
		/// </summary>
		/// <param name="key">The key for cache item</param>
		/// <returns>returns the correct server for provided key</returns>
		public abstract string GetServerForKey(string key);
		/// <summary>
		/// Retrieve a list with all key which are available on cache.
		/// </summary>
		/// <returns>A <see cref="List"/> of strings with all available keys.</returns>
		public abstract List<string> GetAllKeys();
		/// <summary>
		/// Retrieve a list with all key which are available on all cofnigured server nodes.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>A <see cref="List"/> of strings with all available keys.</returns>
		public abstract List<string> GetAllKeys(string host);
		/// <summary>
		/// Pings the specified host.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns>if the server is available then it returns true otherwise false.</returns>
		public abstract bool Ping(string host);
		/// <summary>
		/// Remove Cache Items on server node based on regular expression. Each item which matches
		/// will be automatically removed from each server.
		/// </summary>
		/// <param name="regularExpression">The regular expression.</param>
		/// <returns></returns>
		public abstract bool RegexRemove(string regularExpression);
		/// <summary>
		/// Returns items from cache node based on provided pattern.
		/// </summary>
		/// <param name="regularExpression">The regular expression.</param>
		/// <returns>An IDictionary with <see cref="string"/> and <see cref="byte"/> array with all founded elementes</returns>
		public abstract IDictionary<string, byte[]> RegexGet(string regularExpression);
		/// <summary>
		/// Return Servers CLR (Common Language Runtime), this is needed to decide which 
		/// Hashing codes can be used.
		/// </summary>
		/// <returns>CLR (Common Language Runtime) version number as <see cref="string"/> e.g. xxxx.xxxx.xxxx.xxxx</returns>
		public abstract IDictionary<string, string> ServerNodeVersionClr();		
		/// <summary>
		/// Returns current build version of Shared Cache
		/// </summary>
		/// <returns>Shared Cache version number as <see cref="string"/> e.g. xxxx.xxxx.xxxx.xxxx</returns>
		public abstract IDictionary<string, string> ServerNodeVersionSharedCache();

		/// <summary>
		/// Gets the absolute time expiration of items within cache nodes
		/// </summary>
		/// <param name="keys">A list with keys of type <see cref="string"/></param>
		/// <returns>A IDictionary&lg;<see cref="string"/>, <see cref="DateTime"/>> were each key has its expiration absolute DateTime</returns>
		public abstract IDictionary<string, DateTime> GetAbsoluteTimeExpiration(List<string> keys);
	}
}
