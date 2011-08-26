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
// Name:      IndexusServerProviderBase.cs
// 
// Created:   31-12-2007 SharedCache.com, rschuetz
// Modified:  31-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : same as the client but element names are differnt
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text;

namespace SharedCache.WinServiceCommon.Provider.Server
{
	/// <summary>
	/// Implements the provider base for Shared Cache, based on Microsoft Provider Model.
	/// <example>
	/// <![CDATA[<?xml version="1.0" encoding="utf-8" ?>]]>
	///	<configSections>
	///	<section name="replicatedSharedCache" type="SharedCache.WinServiceCommon.Configuration.Server.IndexusServerProviderSection, SharedCache.WinServiceCommon"/>
	///	</configSections>
	/// 
	/// <replicatedSharedCache defaultProvider="ServerSharedCacheProvider">
	///		<replicatedServers>
	///			<!-- DO NOT DEFINE THE INSTANCE ITSELF !!! IT WILL BE AUTOMATICALLY REMOVED -->
	///			<add key="SrvZh02" ipaddress="192.168.212.37" port="48888" />
	///		</replicatedServers>
	///		<providers>
	///			<add name="ServerSharedCacheProvider"
  ///		     type="SharedCache.WinServiceCommon.Provider.Server.IndexusServerSharedCacheProvider, SharedCache.WinServiceCommon">
	///			</add>
	///		</providers>
	/// </replicatedSharedCache>
	/// </example>
	/// </summary>
	public abstract class IndexusServerProviderBase : ProviderBase
	{
		/// <summary>
		/// Gets the amount of servers which are configured.
		/// </summary>
		/// <value>The amount of servers</value>
		public abstract long Count { get;}
		/// <summary>
		/// Gets the servers key's
		/// </summary>
		/// <value>The servers.</value>
		public abstract string[] Servers { get;}
		/// <summary>
		/// Gets the server list.
		/// </summary>
		/// <value>The server list.</value>
		public abstract List<SharedCache.WinServiceCommon.Configuration.Server.IndexusServerSetting> ServersList { get;}
		/// <summary>
		/// Distributes the specified to other server nodes.
		/// </summary>
		/// <param name="msg">The MSG <see cref="IndexusMessage"/></param>
		public abstract void Distribute(IndexusMessage msg);
		/// <summary>
		/// Pings the specified host.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns>if the server is available then it returns true otherwise false.</returns>
		public abstract bool Ping(string host);
		/// <summary>
		/// Retrieve a list with all key which are available on all cofnigured server nodes.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>A <see cref="List"/> of strings with all available keys.</returns>
		public abstract List<string> GetAllKeys(string host);
		/// <summary>
		/// Based on a list of key's the client receives a dictonary with
		/// all available data depending on the keys.
		/// </summary>
		/// <param name="keys">A List of <see cref="string"/> with all requested keys.</param>
		/// <param name="host">The host to request the key's from</param>
		/// <returns>
		/// A <see cref="IDictionary"/> with <see cref="string"/> and <see cref="byte"/> array element.
		/// </returns>
		public abstract IDictionary<string, byte[]> MultiGet(List<string> keys, string host);
	}
}
