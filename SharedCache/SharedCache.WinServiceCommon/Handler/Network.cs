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
// Name:      Network.cs
// 
// Created:   31-12-2007 SharedCache.com, rschuetz
// Modified:  31-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace SharedCache.WinServiceCommon.Handler
{
	/// <summary>
	/// Provides various centralised Network methods.
	/// </summary>
	public class Network
	{
		/// <summary>
		/// Gets the IP host entry.
		/// </summary>
		/// <returns>A <see cref="IPHostEntry"/> object.</returns>
		public static IPHostEntry GetIPHostEntry()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Network).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return Dns.GetHostEntry(Dns.GetHostName());
		}

		/// <summary>
		/// Gets the first IP address.
		/// </summary>
		/// <returns>A <see cref="IPAddress"/> object.</returns>
		public static IPAddress GetFirstIPAddress()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Network).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return GetIPHostEntry().AddressList[0];
		}

		/// <summary>
		/// Gets the IP end point.
		/// </summary>
		/// <param name="port">The port.</param>
		/// <returns>a <see cref="IPEndPoint"/> object.</returns>
		public static IPEndPoint GetIPEndPoint(int port)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Network).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return new IPEndPoint(GetFirstIPAddress(), port);
		}

		/// <summary>
		/// Gets the server any IP end point.
		/// </summary>
		/// <param name="port">The port.</param>
		/// <returns>a <see cref="IPEndPoint"/> object.</returns>
		public static IPEndPoint GetServerAnyIPEndPoint(int port)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Network).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return new IPEndPoint(System.Net.IPAddress.Any, port);
		}

		static IPHostEntry ipHe = GetIPHostEntry();

		/// <summary>
		/// Evaluates the local IP address and filters known aliases like localhost, local, *, 127.0.0.1, ? 
		/// This is used to prevent to replicated data to same server node
		/// </summary>
		/// <param name="hostname">The hostname.</param>
		/// <returns></returns>
		public static bool EvaluateLocalIPAddress(string hostname)
		{
			if ((hostname == "localhost") || (hostname == "local") || (hostname == "*") || (hostname == "127.0.0.1") || (hostname == "?"))
			{
				return false;
			}

			List<string> data = new List<string>();

			foreach (IPAddress add in ipHe.AddressList)
			{
				data.Add(add.ToString());
			}

			bool result = false;
			foreach (string ip in data)
			{
				try
				{
					string[] d = hostname.Split('.');
					if (d != null && d.Length == 4)
					{
						if (ip.ToString().Equals(hostname))
						{
							return false;
						}

						result = true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					Handler.LogHandler.Info("EvaluateLocalIPAddress throws an exception possible problems with replication. Exception Msg: " + ex.Message);
					return false;
				}
			}
			return result;
		}

		/// <summary>
		/// IPs the name of the address from host.
		/// </summary>
		/// <param name="hostName">Name of the host.</param>
		/// <returns></returns>
		public static IPAddress IPAddressFromHostName(string hostName)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Network).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			try
			{
				if (
					(hostName == "localhost") ||
					(hostName == "local"))
				{
					return IPAddress.Loopback;
				}
				else if (
					(hostName == "*") ||
					(hostName == "127.0.0.1") ||
					(hostName == "?"))
				{
					return IPAddress.Any;
				}
				else
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
					return hostEntry.AddressList[(new Random()).Next() % hostEntry.AddressList.Length];
				}
			}
			catch
			{
				return IPAddress.None;
			}
		}
	}
}
