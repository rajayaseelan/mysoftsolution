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
// Name:      Ports.cs
// 
// Created:   13-07-2007 SharedCache.com, rschuetz
// Modified:  13-07-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : removed all handlers for UDP Ports retrival
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// <b>Retrives fixed ports which are defined in configuration file</b>
	/// </summary>
	public class Ports
	{

		private static int port = -1;

		/// <summary>
		/// TCP Ports.
		/// </summary>
		/// <returns>A <see cref="T:System.Int32"/> Object.</returns>
		public static int ServerDefaultPortTcp()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Ports).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log


			// case the port could not be found the default port is used.
			if (port == -1)
			{
				if(Provider.Server.IndexusServerReplicationCache.ProviderSection != null)
					int.TryParse(Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheIpPort.ToString(), out port);
				if (port == 0 || port == -1)
					return 48888;
				else
					return port;
			}
			else
			{
				return port;
			}
		}
	}
}