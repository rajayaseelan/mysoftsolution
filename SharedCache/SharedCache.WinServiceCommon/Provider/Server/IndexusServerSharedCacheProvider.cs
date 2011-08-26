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
// Name:      IndexusServerSharedCacheProvider.cs
// 
// Created:   31-12-2007 SharedCache.com, rschuetz
// Modified:  31-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : same as the client but element names are differnt
// Modified:  13-01-2008 SharedCache.com, rschuetz : implemented count method
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 


using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.WinServiceCommon.Provider.Server
{
	/// <summary>
	/// Implementing a provider for Shared Cache based on
	/// Microsofts Provider model.
	/// </summary>
	public class IndexusServerSharedCacheProvider : IndexusServerProviderBase
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexusServerSharedCacheProvider"/> class.
		/// </summary>
		public IndexusServerSharedCacheProvider()
		{
			CacheUtil.SetContext(false);
		}

		#region Properties
		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public override long Count
		{
			[System.Diagnostics.DebuggerStepThrough]
			get {
				if (this.Servers != null && this.Servers.Length > 0)
					return (long)this.Servers.Length;
				else
					return 0;
			}
		}

		/// <summary>
		/// Retriving a list of all available servers
		/// </summary>
		/// <value>An <see cref="string"/> array with all servers.</value>
		public override string[] Servers
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (serverListArray == null)
				{
					serverListArray = new List<string>();
					foreach (COM.Configuration.Server.IndexusServerSetting configEntry in ServersList)
					{
						serverListArray.Add(configEntry.IpAddress);
					}
				}
				return serverListArray.ToArray();
			}
		}
		private static List<string> serverListArray;
		private static List<COM.Configuration.Server.IndexusServerSetting> serverList;

		/// <summary>
		/// Retriving a list of all available servers
		/// </summary>
		/// <value>The servers list.</value>
		public override List<COM.Configuration.Server.IndexusServerSetting> ServersList
		{
			get
			{
				if (serverList == null)
				{
					serverList = new List<COM.Configuration.Server.IndexusServerSetting>();
					foreach (COM.Configuration.Server.IndexusServerSetting server in COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.Servers)
					{
						serverList.Add(server);
					}
				}
				return serverList;
			}
		}
		#endregion Properties

		/// <summary>
		/// Distributes the specified to other server nodes.
		/// </summary>
		/// <param name="msg">The MSG <see cref="IndexusMessage"/></param>
		public override void Distribute(IndexusMessage msg)
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			// very important because if this is not set explizit to 
			// server mode it will try to access wrong configuration providers
			msg.ClientContext = false;
			
			switch (msg.Action)
			{
				case IndexusMessage.ActionValue.Add:
					COM.CacheUtil.Add(msg);
					break;
				case IndexusMessage.ActionValue.Remove:
					COM.CacheUtil.Remove(msg);
					break;
				case IndexusMessage.ActionValue.Get:
				case IndexusMessage.ActionValue.GetAllKeys:				
				case IndexusMessage.ActionValue.Statistic:
				case IndexusMessage.ActionValue.Error:
				case IndexusMessage.ActionValue.Successful:
				case IndexusMessage.ActionValue.Ping:
				case IndexusMessage.ActionValue.RemoveAll:
				case IndexusMessage.ActionValue.MultiAdd:
				case IndexusMessage.ActionValue.MultiDelete:
				case IndexusMessage.ActionValue.MultiGet:
				default:
					Handler.LogHandler.Fatal(string.Format("Distribute option '{0}' is not supported!!", msg.Action));
					#if DEBUG
					Console.WriteLine("Distribute option '{0}' is not supported!!", msg.Action);
					#endif
					break;
			}			
		}

		public override bool Ping(string host)
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
				return CacheUtil.Ping(host);
			}
			catch (Exception)
			{
				Handler.LogHandler.Fatal(string.Format("Exception: Could not Ping host: '{0}'!!", host));
#if DEBUG
				Console.WriteLine("Exception: Could not Ping host: '{0}'!!", host);
#endif
				return false;
			}

		}

		/// <summary>
		/// Retrieve a list with all key which are available on all cofnigured server nodes.
		/// </summary>
		/// <param name="host">The host represents the ip address of a server node.</param>
		/// <returns>
		/// A <see cref="List"/> of strings with all available keys.
		/// </returns>
		public override List<string> GetAllKeys(string host)
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
				return CacheUtil.GetAllKeys(host);
			}
			catch (Exception)
			{
				Handler.LogHandler.Fatal(string.Format("Exception: Could not run 'GetAllKeys' on host: '{0}'!!", host));

#if DEBUG
				Console.WriteLine("Exception: Could not run 'GetAllKeys' on host: '{0}'!!", host);
#endif
				return null;
			}			
		}

		public override IDictionary<string, byte[]> MultiGet(List<string> keys, string host)
		{
			Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();

			using (IndexusMessage msg = new IndexusMessage())
			{
				msg.Hostname = host;
				msg.Key = "MultiGetKeyServerNode2ServerNode";
				msg.Action = IndexusMessage.ActionValue.MultiGet;
				msg.Payload = Formatters.Serialization.BinarySerialize(keys);
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
			return result;
		}
	}
}
