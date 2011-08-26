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
// Name:      ManageTcpSocketConnectionPoolFactory.cs
// 
// Created:   10-02-2008 SharedCache.com, rschuetz
// Modified:  10-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharedCache.WinServiceCommon.Sockets
{
	/// <summary>
	/// A factory to access the Connection pools
	/// </summary>
	public static class ManageClientTcpSocketConnectionPoolFactory
	{
		internal static bool CheckPool(string host)
		{
			// no connection made so far, therefore the instance is null and we return true 
			// to make first a try to connect to this host.
			if (instance == null)
			{
				return true;
			}

			bool hostAvailable = instance.GetPoolStatus(host);
			return hostAvailable;
		}

		internal static bool DisablePool(string host)
		{
			if (instance == null)
			{
				return true;
			}
			
			return instance.Disable(host);
		}

		private static ManageTcpSocketConnectionPool instance;

		/// <summary>
		/// Gets the client from the pool based on the host
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns></returns>
		public static SharedCacheTcpClient GetClient(string host)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(ManageClientTcpSocketConnectionPoolFactory).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (instance == null)
			{
				// true / false indicates if connection pool needs to read from client or server config
				// true = client
				// false = server
				instance = new ManageTcpSocketConnectionPool(true);
			}

			return instance.GetSocketFromPool(host);
		}

		/// <summary>
		/// Puts the client back to the pool
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="client">The client.</param>
		public static void PutClient(string host, SharedCacheTcpClient client)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(ManageClientTcpSocketConnectionPoolFactory).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			instance.PutSocketToPool(host, client);
		}

	}
}
