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
// Name:      NetworkMessage.cs
// 
// Created:   29-01-2007 SharedCache.com, rschuetz
// Modified:  29-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Reflection;

namespace SharedCache.WinServiceCommon.Handler
{
	/// <summary>
	/// <b>NetworkMessage is a Network handler of <see cref="IndexusMessage"/> objects based
	/// which are received over the passed Socket.</b>
	/// </summary>
	public class NetworkMessage
	{
		/// <summary>
		/// defines a bulk <see cref="object"/> to manage concurrency.
		/// </summary>
		static private object bulkObject = new object();
		/// <summary>
		/// defines a counter for the amount of data 
		/// </summary>
		 public static long countTransferDataToServer = 0;
		/// <summary>
		/// 
		/// </summary>
		public static long countTransferDataFromServer = 0;
		/// <summary>
		/// Increment the byte amount which sent to server.
		/// </summary>
		/// <param name="value">The value.</param>
		public static void IncToServer(long value)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(NetworkMessage).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			lock (bulkObject)
			{
				countTransferDataToServer += value;
			}
		}
		/// <summary>
		/// Increment the byte amount which is received from server.
		/// </summary>
		/// <param name="value">The value.</param>
		public static void IncFromServer(long value)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(NetworkMessage).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			lock (bulkObject)
			{
				countTransferDataFromServer += value;
			}
		}
	}
}
