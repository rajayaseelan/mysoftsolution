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
// Name:      UtilByte.cs
// 
// Created:   10-02-2008 SharedCache.com, rschuetz
// Modified:  10-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Net;
using System.Reflection;

namespace SharedCache.WinServiceCommon.Handler
{
	/// <summary>
	/// Byte Helper class
	/// </summary>
	public abstract class UtilByte
	{
		/// <summary>
		/// Creates the message header.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public static byte[] CreateMessageHeader(byte[] data)
		{
			#region Access Log
#if TRACE
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(UtilByte).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			byte[] messageLength = BitConverter.GetBytes(data.LongLength);
			return UtilByte.Combine(messageLength, data);
		}

		/// <summary>
		/// Combines the specified byte1.
		/// </summary>
		/// <param name="byte1">The byte1.</param>
		/// <param name="byte2">The byte2.</param>
		/// <returns></returns>
		public static byte[] Combine(byte[] byte1, byte[] byte2)
		{
			#region Access Log
#if TRACE
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(UtilByte).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (byte1 == null)
			{
				throw new Exception("byte1");
			}

			if (byte2 == null)
			{
				throw new Exception("byte2");
			}

			byte[] combinedBytes = new byte[byte1.Length + byte2.Length];
			Buffer.BlockCopy(byte1, 0, combinedBytes, 0, byte1.Length);
			Buffer.BlockCopy(byte2, 0, combinedBytes, byte1.Length, byte2.Length);

			return combinedBytes;
		}
	}
}
