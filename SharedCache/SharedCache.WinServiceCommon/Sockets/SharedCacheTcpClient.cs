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
// Name:      SharedCacheTcpClient.cs
// 
// Created:   10-02-2008 SharedCache.com, rschuetz
// Modified:  10-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  11-02-2008 SharedCache.com, rschuetz : updated console output data and logging
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace SharedCache.WinServiceCommon.Sockets
{
	/// <summary>
	/// A Client object which handle all communication for a requst to the server 
	/// in a block mode.
	/// </summary>
	public class SharedCacheTcpClient : IDisposable
	{
		/// <summary>
		/// use for lock() operations within the client
		/// </summary>
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
		#region Property: ServerConnection
		private Socket serverConnection;

		/// <summary>
		/// Gets the ServerConnection
		/// </summary>
		public Socket ServerConnection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.serverConnection; }
		}
		#endregion
		#region Property: LastUsed
		private DateTime lastUsed = DateTime.Now;
		
		/// <summary>
		/// Gets/sets the LastUsed
		/// </summary>
		public DateTime LastUsed
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.lastUsed;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.lastUsed = value;  }
		}
		#endregion

		/// <summary>
		/// Gets a value indicating whether this <see cref="SharedCacheTcpClient"/> is connected.
		/// </summary>
		/// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
		public bool Connected
		{
			get
			{
				if (this.serverConnection != null)
				{
					return this.serverConnection.Connected;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Closes the connection to the server.
		/// </summary>
		public void Close()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (this.serverConnection != null)
			{
				this.serverConnection.Close();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SharedCacheTcpClient"/> class.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="port">The port.</param>
		public SharedCacheTcpClient(string host, int port)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.host = host;
			this.port = port;

			IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);
			this.serverConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.serverConnection.Connect(endpoint);
			this.serverConnection.NoDelay = true;
		}

		/// <summary>
		/// Sends the specified data and returns the echo from the server.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public IndexusMessage Send(byte[] data)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (this.serverConnection != null && this.serverConnection.Connected)
			{
				// create header message before we send data to server
				data = Handler.UtilByte.CreateMessageHeader(data);

				// sending data to server sync mode and a block = true;
				int sent = this.serverConnection.Send(data, 0, data.Length, SocketFlags.None);

				// incremeant the value amount of how much data sent to server
				Handler.NetworkMessage.IncToServer((long)sent);

				

				// data has been sent and need to be same size like the data which intend to be sent!
				if (sent == data.Length)
				{
					#region Read data
					int read;
					long readedBytes = 0;
					long messageLength = long.MaxValue;
					long nextPortionSize = 65535;
					bool readHeader = true;
					List<byte> dataList = new List<byte>();
					byte[] portion = null;

					do
					{
						read = 0;
						nextPortionSize = messageLength - readedBytes > nextPortionSize ? 65535 : messageLength - readedBytes;
						portion = new byte[nextPortionSize];
						try
						{
							read = this.serverConnection.Receive(portion, 0, portion.Length, SocketFlags.None);
						}
						catch (SocketException sEx)
						{
							#region
							switch (sEx.SocketErrorCode)
							{
								// 10054
								case SocketError.ConnectionAborted:
								// case SocketError.ConnectionRefused:
								case SocketError.ConnectionReset:
									{
										if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
										{
											#region Client
											// Add request to log
											Handler.LogHandler.Traffic(
												string.Format(
													@"[{0}]Socket Error Code: {1}-{2}",
													System.Threading.Thread.CurrentThread.ManagedThreadId,
													sEx.ErrorCode, sEx.SocketErrorCode
												)												
											);
											#endregion
										}
#if DEBUG
										Console.WriteLine(
											@"Socket Error Code: {0}" + Environment.NewLine +
											"Exception Title: {1}" + Environment.NewLine +
											"Exception Stacktrace: ", sEx.ErrorCode, sEx.Message, sEx.StackTrace);
#endif
										break;
									}
								default:
									{
										if (1 == Provider.Cache.IndexusDistributionCache.ProviderSection.ClientSetting.LoggingEnable)
										{
											#region Client
											// Add request to log
											Handler.LogHandler.Traffic(
												string.Format(
													@"[{0}]Not Expected Socket Error Code: {1}-{2}",
													System.Threading.Thread.CurrentThread.ManagedThreadId,
													sEx.ErrorCode, sEx.SocketErrorCode
												)
											);
											#endregion
										}
#if DEBUG
										Console.WriteLine(
											@"Not Expected Socket Error Code: {0}" + Environment.NewLine +
											"Exception Title: {1}" + Environment.NewLine +
											"Exception Stacktrace: ", sEx.ErrorCode, sEx.Message, sEx.StackTrace);
#endif
										break;
									}
							}
							#endregion
						}
						catch (Exception ex)
						{
							Console.WriteLine("Exception: " + ex.Message);
						}

						if (readHeader)
						{
							// message header are long values for this issue we need to add 8 bytes to length
							messageLength = BitConverter.ToInt64(portion, 0) + 8;
							readHeader = false;
						}
						
						byte[] tmp = new byte[read];
						Array.Copy(portion, tmp, tmp.Length);
						dataList.AddRange(tmp);

						portion = null;
						readedBytes += read;
					} while (this.serverConnection.Connected && readedBytes < messageLength);
					#endregion Read data
					// if above loop breaked because we lost the connection report about it.
					if (!this.serverConnection.Connected)
					{
						Handler.LogHandler.Info(string.Format("[{0}:{1}] Unexpected disconnection happend", this.host, this.port));
						// no further action can be taken.
						return null;
					}

					try
					{
						// incremeant the value amount of how much data sent to server
						Handler.NetworkMessage.IncFromServer((long)dataList.Count);

						// manage protocol data
						dataList.RemoveRange(0, 8);

						// create from byte [] an IndexusMessage
						System.IO.MemoryStream st = new System.IO.MemoryStream(dataList.ToArray());
						st.Seek(0, 0);
						return new IndexusMessage(st);
					}
					catch (Exception ex)
					{
						Handler.LogHandler.Error(string.Format("[{0}:{1}] An exception happend to create a new IndexusMessage after receiving data from server.", this.host, this.port), ex);
						Handler.LogHandler.Fatal(string.Format("[{0}:{1}] An exception happend to create a new IndexusMessage after receiving data from server.", this.host, this.port), ex);
						return null;
					}
				}
				else
				{
					Console.WriteLine("[{0}:{1}] Did not sent all data to host, only {2} of {3}", this.host, this.port, sent, data.Length);
					Handler.LogHandler.Traffic(string.Format("[{0}:{1}] Did not sent all data to host, only {2} of {3}", this.host, this.port, sent, data.Length));
					return null;
				}
			}
			else
			{
				Console.WriteLine("[{0}:{1}] The Socket has not been initialized (socket => NULL) or the socket is not connected {2}", this.host, this.port, this.serverConnection == null ? " 'Socket is NULL' " : this.serverConnection.Connected.ToString());
				Handler.LogHandler.Traffic(string.Format("[{0}:{1}] The Socket has not been initialized (socket => NULL) or the socket is not connected {2}", this.host, this.port, this.serverConnection == null ? " 'Socket is NULL' " : this.serverConnection.Connected.ToString()));

				return null;
			}
		}

		/// <summary>
		/// Sets the blocking mode.
		/// </summary>
		/// <param name="mode">if set to <c>true</c> [mode].</param>
		internal void SetBlockingMode(bool mode)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (this.serverConnection != null)
			{
				this.serverConnection.Blocking = mode;
			}
		}
		
		public void Dispose()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (this.serverConnection != null)
			{
				using (this.serverConnection)
				{
					this.serverConnection.Shutdown(SocketShutdown.Both);
				}
			}
			this.serverConnection = null;
		}
	}
}
