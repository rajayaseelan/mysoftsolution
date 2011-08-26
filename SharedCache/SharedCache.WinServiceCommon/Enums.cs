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
// Name:      Enums.cs
// 
// Created:   21-01-2007 SharedCache.com, rschuetz
// Modified:  21-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  23-12-2007 SharedCache.com, rschuetz : created EnumUtil with several Enum Utility methods
// Modified:  23-12-2007 SharedCache.com, rschuetz : introduction for ServiceCacheCleanUp enum
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// several Enum Utility methods
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class EnumUtil<T>
	{
		/// <summary>
		/// Enums to list.
		/// </summary>
		/// <remarks>
		/// List DayOfWeek> weekdays = EnumHelper.EnumToList&lg;DayOfWeek>().FindAll(
		///     delegate (DayOfWeek x)
		///     {
		///         return x != DayOfWeek.Sunday && x != DayOfWeek.Saturday;
		///     });
		/// </remarks>
		/// <typeparam name="T">generic type</typeparam>
		/// <returns>a list of generic type</returns>
		public static List<T> EnumToList<T>()
		{
			Type enumType = typeof(T);

			// Can't use type constraints on value types, so have to do check like this
			if (enumType.BaseType != typeof(Enum))
				throw new ArgumentException("T must be of type System.Enum");

			Array enumValArray = Enum.GetValues(enumType);

			List<T> enumValList = new List<T>(enumValArray.Length);

			foreach (int val in enumValArray)
			{
				enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
			}

			return enumValList;
		}

		/// <summary>
		/// Parses the specified string and returns a enum.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns>requested enums</returns>
		public static T Parse(string s)
		{
			return (T)Enum.Parse(typeof(T), s);
		}
	}

	/// <summary>
	/// <b>Defines General Enums</b>
	/// </summary>
	public class Enums
	{
		/// <summary>
		/// LogCategory Options
		/// </summary>
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		[System.FlagsAttribute]
		public enum LogCategory
		{
			/// <summary>
			/// General information
			/// </summary>
			General,
			/// <summary>
			/// information upon WinService start
			/// </summary>
			ServiceStart,
			/// <summary>
			/// information upon WinService stops
			/// </summary>
			ServiceStop,
			/// <summary>
			/// information upon WinService restarts
			/// </summary>
			ServiceRestart,
			/// <summary>
			/// information around the winservice
			/// </summary>
			ServiceInfo,
			/// <summary>
			/// cleanup after service started
			/// </summary>
			ServiceCleanUpStart,
			/// <summary>
			/// cleanup after service stopped
			/// </summary>
			ServiceCleanUpStop,
			/// <summary>
			/// adding action information
			/// </summary>
			Cache_Add,
			/// <summary>
			/// removing action information
			/// </summary>
			Cache_Remove,
			/// <summary>
			/// retrivel action information
			/// </summary>
			Cache_Get,
			/// <summary>
			/// statistical action information
			/// </summary>
			Cache_Stat,
			/// <summary>
			/// after retrival action information
			/// </summary>
			Cache_Result
		}

		/// <summary>
		/// Defines Action options
		/// </summary>
		[Serializable]
		public enum Action
		{
			/// <summary>
			/// Statistics
			/// </summary>
			Stat,
			/// <summary>
			/// adding
			/// </summary>
			Add,
			/// <summary>
			/// adding with expire date
			/// </summary>
			AddWithExpire,
			/// <summary>
			/// before data retrival
			/// </summary>
			BeforeGet,
			/// <summary>
			/// normal get retrival
			/// </summary>
			Get,
			/// <summary>
			/// remove data
			/// </summary>
			Remove,
			/// <summary>
			/// ping instance
			/// </summary>
			Ping,
			/// <summary>
			/// distributes data over UDP protocol
			/// </summary>
			UdpDistribution
		}

		/// <summary>
		/// Service Cache CleanUp Pattern options
		/// </summary>
		[Serializable]
		public enum ServiceCacheCleanUp
		{ 
			/// <summary>
			/// Depends on the item priority the items will stay longer in cache, default is Normal <see cref="IndexusMessage.CacheItemPriority"/>
			/// </summary>
			CACHEITEMPRIORITY,
			/// <summary>
			/// Least Recent Used, The objects with the oldest requests will be deleted for free memory space
			/// </summary>
			LRU,
			/// <summary>
			/// Least Frquently used objects will be deleted for free memory space
			/// </summary>
			LFU,
			/// <summary>
			/// The objects with the smallest time left in cache will be deleted to free memory space
			/// </summary>
			TIMEBASED,
			/// <summary>
			/// Delete always biggest objects to free memory space
			/// </summary>
			SIZE,
			/// <summary>
			/// Delete smallest objects to free memory space
			/// </summary>
			LLF,
			/// <summary>
			/// a combination with a pointing individual pointing system
			/// </summary>
			HYBRID
		}

		[Serializable]
		public enum HashingAlgorithm
		{ 
			Hashing = 10,
			Ketama = 20,
			FvnHash32 = 30,
			FvnHash64 = 40,
		}
	}
}
