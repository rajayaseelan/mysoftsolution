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
// Name:      IndexusMessage.cs
// 
// Created:   29-01-2007 SharedCache.com, rschuetz
// Modified:  29-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  18-12-2007 SharedCache.com, rschuetz : since SharedCache works internally with byte[] instead of objects almoast all needed code has been adapted
// Modified:  21-12-2007 SharedCache.com, rschuetz : added CacheItemPriority for one of the clean-upstrategies, extended object by additional Constructor
// Modified:  22-12-2007 SharedCache.com, rschuetz : NotRemovable has been deleted, the user itself has to ensure that he doesn't fill to much into cache!
// Modified:  31-12-2007 SharedCache.com, rschuetz : added timestamp value for distribution between nodes
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated GetBytes() / SetBytes() / Copy(IndexusMessage msg) since this has been added i forgot to serialize / deserialize it.
// Modified:  31-12-2007 SharedCache.com, rschuetz : make usage of: Ports.PortTcp() instead of a direct call
// Modified:  04-01-2008 SharedCache.com, rschuetz : introduction of new ActionValue: RemoveAll -> this enables the client to send with one command to clear the whole cache.
// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - updated GetBytes() / SetBytes() - there is no usage anymore of BinaraySerialization to serialize / deserialize key and data.
// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - protocol changed - added string key and byte[] payload instead KeyValuePair
// Modified:  12-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - removed all Consturctor inheritance since its called to ofthen GetBytes() method.
// Modified:  08-02-2008 SharedCache.com, rschuetz : added usage of Socket pooling and socket factory
// Modified:  11-02-2008 SharedCache.com, rschuetz : updated sending and receiving messages with pooling objects
// Modified:  24-02-2008 SharedCache.com, rschuetz : refactored full protocol, removed a lot of constructores which are not needed anymore - also removed property length
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// IndexusMessage
	/// </summary>
	[Serializable]
	public class IndexusMessage : IDisposable
	{
		#region Enum
		/// <summary>
		/// public enum StatusValue
		/// </summary>
		public enum StatusValue
		{
			/// <summary>
			/// Defines Request method
			/// </summary>
			Request,
			/// <summary>
			/// Defines Response method
			/// </summary>
			Response,
			/// <summary>
			/// Defines replication request handling - this is defined for server to server messages
			/// </summary>
			ReplicationRequest,
		};
		/// <summary>
		/// public enum ActionValue
		/// </summary>
		public enum ActionValue
		{
			/// <summary>
			/// Adding value to cache.
			/// </summary>
			Add,
			/// <summary>
			/// Retrive data from cache
			/// </summary>
			Get,
			/// <summary>
			/// get all keys from cache
			/// </summary>
			GetAllKeys,
			/// <summary>
			/// remove data from cache
			/// </summary>
			Remove,
			/// <summary>
			/// get statistic from cache
			/// </summary>
			Statistic,
			/// <summary>
			/// defines an error appears
			/// </summary>
			Error,
			/// <summary>
			/// sussessfully transmission
			/// </summary>
			Successful,
			/// <summary>
			/// pings a server
			/// </summary>
			Ping,
			/// <summary>
			/// remove all items at once from cache
			/// </summary>
			RemoveAll,
			/// <summary>
			/// Get a bunch of data without any relation between the items.
			/// </summary>
			MultiGet,
			/// <summary>
			/// Adding a bunch of data without any relation between the items.
			/// </summary>
			MultiAdd,
			/// <summary>
			/// Delete a bunch of data without any relation between the items.
			/// </summary>
			MultiDelete,
			/// <summary>
			/// Executing a regular expression on the server key's and remove all 
			/// matches with one single call.
			/// </summary>
			RegexRemove,
			/// <summary>
			/// Receive a bunch of data where key's where item keys 
			/// are matching pattern
			/// </summary>
			RegexGet,
			/// <summary>
			/// Modify item attribute expires within cache node.
			/// </summary>
			ExtendTtl,
			/// <summary>
			/// Identify CLR (Common Language Runtime) Version number, e.g. 2.0.50727.3053 / 2.0.50727.1433
			/// </summary>
			VersionNumberClr,
			/// <summary>
			/// Identify Shared Cache Version Number, e.g.: 2.0.4.276, 2.0.4.277, 2.0.4.278, etc
			/// </summary>
			VersionNumberSharedCache,
			/// <summary>
			/// Request to receive absolut expiration <see cref="DateTime"/> for cached objects.
			/// </summary>
			GetAbsoluteTimeExpiration,
		};

		/// <summary>
		/// Specifies the relative priority of items stored in the Cache object <see cref="Cache"/>
		/// </summary>
		public enum CacheItemPriority
		{
			/// <summary>
			/// Cache items with this priority level are the most likely to be deleted from the cache as the server frees system memory.
			/// </summary>
			Low = 10,
			/// <summary>
			/// Cache items with this priority level are more likely to be deleted from the cache as the server frees system memory than items assigned a Normal priority.
			/// </summary>
			BelowNormal = 20,
			/// <summary>
			/// Cache items with this priority level are likely to be deleted from the cache as the server frees system memory only after those items with Low or BelowNormal priority. This is the default.
			/// </summary>
			Normal = 30,
			/// <summary>
			/// Cache items with this priority level are less likely to be deleted as the server frees system memory than those assigned a Normal priority.
			/// </summary>
			AboveNormal = 40,
			/// <summary>
			/// Cache items with this priority level are the least likely to be deleted from the cache as the server frees system memory.
			/// </summary>
			High = 50, 
			/// <summary>
			/// Do not modify anything, used upon attribute extensios.
			/// </summary>
			None
		}
		#endregion Enum

		#region Properties
		/// <summary>
		/// this property is not sent over the wire its used to evaluate in which context the message runs
		/// </summary>
		public bool ClientContext = true;

		#region Property: Id
		private long id;

		/// <summary>
		/// Gets/sets the Id
		/// </summary>
		public long Id
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.id; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.id = value; }
		}
		#endregion

		#region Property: Status
		private StatusValue status = StatusValue.Request;

		/// <summary>
		/// Gets/sets the Status
		/// </summary>
		public StatusValue Status
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.status; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.status = value; }
		}
		#endregion

		#region Property: Action
		private ActionValue action = ActionValue.Add;

		/// <summary>
		/// Gets/sets the Action
		/// </summary>
		public ActionValue Action
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.action; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.action = value; }
		}
		#endregion

		#region Property: ItemPriority
		private CacheItemPriority itemPriority = CacheItemPriority.Normal;

		/// <summary>
		/// Gets/sets the ItemPriority
		/// </summary>
		public CacheItemPriority ItemPriority
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.itemPriority; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.itemPriority = value; }
		}
		#endregion

		#region Property: Key
		private string key = string.Empty;

		/// <summary>
		/// Gets/sets the Key
		/// </summary>
		public string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.key; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.key = value; }
		}
		#endregion

		#region Property: Payload
		private byte[] payload;

		/// <summary>
		/// Gets/sets the Payload
		/// </summary>
		public byte[] Payload
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.payload; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.payload = value; }
		}
		#endregion

		#region Property: Expires
		private DateTime expires = DateTime.MaxValue;

		/// <summary>
		/// Gets/sets the Expires
		/// </summary>
		public DateTime Expires
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.expires; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.expires = value; }
		}
		#endregion

		#region Property: Hostname

		private string hostname = string.Empty; // Handler.Config.GetStringValueFromConfigByKey(Constants.ConfigServiceCacheIpAddress);

		/// <summary>
		/// Gets/sets the Hostname
		/// </summary>
		public string Hostname
		{
			// [System.Diagnostics.DebuggerStepThrough]
			get { return this.hostname; }

			// [System.Diagnostics.DebuggerStepThrough]
			set { this.hostname = value; }
		}
		#endregion

		#region Property: Timestamp
		private DateTime timestamp = DateTime.Now;

		/// <summary>
		/// Gets/sets the Timestamp
		/// </summary>
		public DateTime Timestamp
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.timestamp; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.timestamp = value; }
		}
		#endregion

		#endregion Properties

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexusMessage"/> class.
		/// </summary>
		public IndexusMessage()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.id = Handler.Unique.NextUniqueId();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexusMessage"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="value">The value.</param>
		public IndexusMessage(long id, ActionValue value)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.id = id;
			this.action = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexusMessage"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="status">The status.</param>
		/// <param name="action">The action.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="host">The host.</param>
		/// <param name="expires">The expires.</param>
		/// <param name="key">The key.</param>
		/// <param name="payload">The payload.</param>
		public IndexusMessage(long id, StatusValue status, ActionValue action, CacheItemPriority priority, string host, DateTime expires, string key, byte[] payload)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.id = id;
			this.status = status;
			this.action = action;
			this.itemPriority = priority;

			if (!string.IsNullOrEmpty(host))
				this.hostname = host;
			this.expires = expires;
			this.key = key;
			this.payload = payload;

			this.timestamp = DateTime.Now;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexusMessage"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public IndexusMessage(MemoryStream stream)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.SetBytes(stream);
		}

		#endregion Constructor

		/// <summary>
		/// Sends this instance.
		/// </summary>
		/// <returns></returns>
		public bool Send()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Sockets.SharedCacheTcpClient client = null;
			StatusValue statusBeforeSending = this.Status;
			try
			{
				if (statusBeforeSending != StatusValue.ReplicationRequest)
				{
					client = Sockets.ManageClientTcpSocketConnectionPoolFactory.GetClient(this.hostname);
					//try
					//{
					//  
					//}
					//catch (Exception ex)
					//{
					//  Console.WriteLine(ex.ToString());
					//}					

					// case for replication mode, choose another server node.
					if (client == null && Provider.Cache.IndexusDistributionCache.SharedCache.ReplicatedServersList.Count > 0)
					{
						client = Sockets.ManageClientTcpSocketConnectionPoolFactory.GetClient(
							Provider.Cache.IndexusDistributionCache.SharedCache.ReplicatedServersList[0].IpAddress
							);
					}
				}
				else
				{
					// server to server node communication
					client = Sockets.ManageServerTcpSocketConnectionPoolFactory.GetServerClient(this.hostname);
				}

				if (client != null)
				{
					byte[] dataToSend = this.GetBytes();

					#region Pre Logging
#if DEBUG
					System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
					sp.Start();
					if (this.ClientContext)
					{
						if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
						{
							#region Client
							// Add request to log
							Handler.LogHandler.Traffic(
								string.Format(Constants.TRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,
									this.Id,
									this.Action.ToString(),
									this.Status.ToString(),
									dataToSend.LongLength
								)
							);
							#endregion
						}
					}
					else
					{
						if (1 == Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
						{
							#region Server
							// Add request to log
							Handler.LogHandler.SyncInfo(
								string.Format(Constants.TRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,
									this.Id,
									this.Action.ToString(),
									this.Status.ToString(),
									dataToSend.LongLength
								)
							);
							#endregion
						}
					}
#else
					if (this.ClientContext)
					{
						if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
						{
					#region Client
							// Add request to log
							Handler.LogHandler.Traffic(
								string.Format(Constants.TRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,								
									this.Id,	
									this.Action.ToString(),
									this.Status.ToString(),
									dataToSend.LongLength
								)
							);
							#endregion
						}
					}
					else
					{
						if (1 == Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
						{
					#region Client
							// Add request to log
							Handler.LogHandler.SyncInfo(
								string.Format(Constants.TRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,								
									this.Id,	
									this.Action.ToString(),
									this.Status.ToString(),
									dataToSend.LongLength
								)
							);
							#endregion
						}
					}
#endif
					#endregion Pre Logging

					// potential botleneck!!!
					this.Copy(client.Send(dataToSend));

					#region Post Logging
#if DEBUG
					if (this.ClientContext)
					{
						if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
						{
							sp.Stop();
							#region Client
							// Add request to log
							Handler.LogHandler.Traffic(
								string.Format(Constants.POSTTRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,
									this.Id,
									this.Action.ToString(),
									this.Status.ToString(),
									this.GetBytes().LongLength,
									sp.ElapsedMilliseconds
								)
							);
							#endregion
						}
					}
					else
					{
						if (1 == Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
						{
							sp.Stop();
							#region Server
							// Add request to log
							Handler.LogHandler.SyncInfo(
								string.Format(Constants.POSTTRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,
									this.Id,
									this.Action.ToString(),
									this.Status.ToString(),
									this.GetBytes().LongLength,
									sp.ElapsedMilliseconds
								)
							);
							#endregion
						}
					}


#else
					if (this.ClientContext)
					{
						if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
						{
					#region Client
							// Add request to log
							Handler.LogHandler.Traffic(
								string.Format(Constants.TRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,								
									this.Id,	
									this.Action.ToString(),
									this.Status.ToString(),
									dataToSend.LongLength
								)
							);
							#endregion
						}
					}
					else
					{
						if (1 == Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.LoggingEnable)
						{
					#region Server
							// Add request to log
							Handler.LogHandler.SyncInfo(
								string.Format(Constants.TRAFFICLOG,
									client.ServerConnection.LocalEndPoint,
									System.Threading.Thread.CurrentThread.ManagedThreadId,								
									this.Id,	
									this.Action.ToString(),
									this.Status.ToString(),
									dataToSend.LongLength
								)
							);
							#endregion
						}
					}
#endif
					#endregion Postlogging
					switch (this.action)
					{
						case ActionValue.Successful:
							{
								// Action done successfully;
								return true;
							}
						case ActionValue.Error:
							{
								// TODO: Error handling somehow, maybe we need to extend 
								// to return an error code or something similar.
								//try
								//{
								//  //
								//  //CacheException ex = Formatters.Serialization.BinaryDeSerialize<CacheException>(this.Payload);
								//  //Console.WriteLine(ex.Title);
								//}
								//catch (Exception ex)
								//{}
								Handler.LogHandler.Error("Error, check log files!");
								return false;
							}
					}
				}
				else
				{
					Console.WriteLine(string.Format("Could not receive Socket Client from pool {0}", this.hostname));
					Handler.LogHandler.Force(string.Format("Could not receive Socket Client from pool {0}", this.hostname));
					Handler.LogHandler.Fatal(string.Format("Could not receive Socket Client from pool {0}", this.hostname));
					return false;
				}
			}
			finally
			{
				if (client != null)
				{
					if (statusBeforeSending != StatusValue.ReplicationRequest)
					{
						Sockets.ManageClientTcpSocketConnectionPoolFactory.PutClient(this.hostname, client);						
					}
					else
					{
						Sockets.ManageServerTcpSocketConnectionPoolFactory.PutServerClient(this.hostname, client);
					}					
				}				
			}

			return false;
		}
		/// <summary>
		/// Copies the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		private void Copy(IndexusMessage msg)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (msg == null)
			{
				this.action = ActionValue.Error;
				throw new ArgumentNullException("IndexusMessage cannot be NULL");
			}
			this.itemPriority = msg.ItemPriority;
			this.id = msg.id;
			this.action = msg.action;
			this.key = msg.key;
			this.payload = msg.payload;
			this.expires = msg.expires;
			this.status = msg.status;
			this.timestamp = msg.timestamp;
		}

		/// <summary>
		/// Sets the bytes.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		public bool SetBytes(MemoryStream stream)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			BinaryReader br = new BinaryReader(stream);

			this.status = (StatusValue)br.ReadByte();
			this.action = (ActionValue)br.ReadByte();
			this.id = br.ReadInt64();
			this.itemPriority = (CacheItemPriority)br.ReadByte();

			// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=5046
			this.expires = Formatters.DateTimeUnix.DateTimeFromUnixTime((long)br.ReadUInt64());   // Formatters.DateTimeUnix.ToDateTime(br.ReadDouble());
			this.timestamp = Formatters.DateTimeUnix.DateTimeFromUnixTime((long)br.ReadUInt64());  // Formatters.DateTimeUnix.ToDateTime(br.ReadDouble());

			int dataLength1 = br.ReadInt32();
			byte[] buf1 = new byte[dataLength1];
			int read1 = br.Read(buf1, 0, dataLength1);
			this.key = System.Text.Encoding.UTF8.GetString(buf1);
			int dataLength = br.ReadInt32();
			if (dataLength > 0)
			{
				byte[] buf = new byte[dataLength];
				int read = br.Read(buf, 0, dataLength);
				this.payload = buf;
				buf = null;
			}
			return true;
		}

		/// <summary>
		/// Gets the bytes.
		/// </summary>
		/// <returns></returns>
		public byte[] GetBytes()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			MemoryStream stream = new MemoryStream();
			using (BinaryWriter bw = new BinaryWriter(stream))
			{
				bw.Write((byte)this.status);
				bw.Write((byte)this.action);
				bw.Write(this.id);
				bw.Write((byte)this.itemPriority);

				// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 http://www.codeplex.com/SharedCache/WorkItem/View.aspx?WorkItemId=5046
				bw.Write(Formatters.DateTimeUnix.UnixTimeFromDateTime(this.expires)); /*Formatters.DateTimeUnix.ToInt64(this.expires)*/
				bw.Write(Formatters.DateTimeUnix.UnixTimeFromDateTime(this.timestamp)); /*Formatters.DateTimeUnix.ToInt64(this.timestamp)*/
				// Modified:  18-02-2008 SharedCache.com, rschuetz : handle different charsets correclty https://www.codeplex.com/Thread/View.aspx?ProjectName=SharedCache&ThreadId=22354
				byte[] arrKeyBytes = System.Text.Encoding.UTF8.GetBytes(this.key);
				bw.Write(arrKeyBytes.Length);
				bw.Write(arrKeyBytes);
				bw.Write(payload == null ? 0 : payload.Length);
				if (payload != null)
				{
					bw.Write(payload);
				}
			}
			return stream.ToArray();
		}

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
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(@"Object ID: {0}" + Environment.NewLine, this.id);
			sb.AppendFormat(@"Object Status: {0}" + Environment.NewLine, this.status.ToString());
			sb.AppendFormat(@"Object Action: {0}" + Environment.NewLine, this.action.ToString());
			sb.AppendFormat(@"Object Expires: {0}" + Environment.NewLine, this.expires.ToLocalTime());
			if (this.key != null && this.payload != null)
			{
				sb.AppendFormat(@"Object KeyValuePair: Key: '{0}' Value: {1}" + Environment.NewLine, this.key, this.payload.ToString());
			}
			else
			{
				sb.AppendFormat(@"Object KeyValuePair: seems to be empty!");
			}

			return sb.ToString();
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.hostname = null;
			this.id = -1;
			this.key = null;
			this.payload = null;
		}
		
		#endregion
	}
}
