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
// Name:      Constants.cs
// 
// Created:   21-01-2007 SharedCache.com, rschuetz
// Modified:  21-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  10-07-2007 SharedCache.com, rschuetz : Deleted ServiceConnectionPoolSize, no further need
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated consistency of logging calls
// Modified:  03-01-2008 SharedCache.com, rschuetz : deleted the following strings since they have no usage anymore: ServiceCacheUdpPort,ServiceCacheUdpServerPort,ServiceCacheUdpListenerPort,ServiceCacheUdpBroadcast 
// Modified:  12-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - added compression and size values
// Modified:  16-02-2008 SharedCache.com, rschuetz : added Caching option HashingAlgorithm
// Modified:  26-02-2008 SharedCache.com, rschuetz : removed all strings appSetting strings since each module has its own provider
// Modified:  26-02-2008 SharedCache.com, rschuetz : added TRAFFICLOG / POSTTRAFFICLOG basic strings for traffic logging
// Modified:  26-02-2008 SharedCache.com, rschuetz : added replication string for logging
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// <b>Contains app.config constant names, to prevent spelling problems.</b>
	/// </summary>
	public class Constants
	{
		/// <summary>
		/// 
		/// </summary>
		public const string TRAFFICLOG = "\t{0,20}\t{1,3}\tID\t{2,6}\tAction\t{3,-25}\tStatus\t{4,-10}\tAmount\t{5,7}";
		/// <summary>
		/// 
		/// </summary>
		public const string POSTTRAFFICLOG = "\t{0,20}\t{1,3}\tID\t{2,6}\tAction\t{3,-25}\tStatus\t{4,-10}\tAmount\t{5,7}\tMilliseconds\t{6,6}";
		/// <summary>
		/// 
		/// </summary>
		public const string SYNCENQUEUELOG = "\tEnque: ID:\t{0,6}; Action: \t{1,-19}; \tStatus: \t{2,-10};";
		/// <summary>
		/// 
		/// </summary>
		public const string SYNCDEQUEUELOG = "\tEnque: ID:\t{0,6}; Action: \t{1,-19}; \tStatus: \t{2,-10};";
	}
}
