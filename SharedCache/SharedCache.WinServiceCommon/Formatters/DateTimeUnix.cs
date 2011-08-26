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
// Name:      DateTimeUnix.cs
// 
// Created:   11-01-2008 SharedCache.com, rschuetz
// Modified:  11-01-2008 SharedCache.com, rschuetz : Creation
// Modified:  11-01-2008 SharedCache.com, rschuetz : this code has been taken from Jayrock [http://www.koders.com/csharp/fid3A8F74614030F0A39C93BD24F81CDFF2E8084A1A.aspx?s=unixtime]
// Modified:  15-01-2008 SharedCache.com, rschuetz : code from jayrock has been deleted because of implemented RFC - its only support up to year 3000 which throws exception upon DateTime.MaxValue
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharedCache.WinServiceCommon.Formatters
{
	/// <summary>
	/// To minimize Protocol size the usage of Unix time is needed instead of DateTime which 
	/// is serialized with the BinarySerializer. representing the number of seconds since 
	/// Midnight UTC 1 Jan 1970 on the Gregorian Calendar
	/// </summary>
	public sealed class DateTimeUnix
	{
		/// <summary>
		/// Default unix time
		/// </summary>
		public static DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0);

		/// <summary>
		/// Convert a unix datetime which is represented as long into a .net <see cref="DateTime"/>
		/// </summary>
		/// <param name="timeT">The time T.</param>
		/// <returns>object of type <see cref="DateTime"/></returns>
		public static DateTime DateTimeFromUnixTime(long timeT)
		{

			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(DateTimeUnix).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return origin + new TimeSpan(timeT * TimeSpan.TicksPerSecond);
		}

		/// <summary>
		/// Convert a .net <see cref="DateTime"/> which represents unix datetime 
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns>an object of type <see cref="long"/></returns>
		public static long UnixTimeFromDateTime(DateTime time)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(DateTimeUnix).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			long diff = (long)(time.Ticks - origin.Ticks);
			return (diff / TimeSpan.TicksPerSecond);
		}
	}
}
