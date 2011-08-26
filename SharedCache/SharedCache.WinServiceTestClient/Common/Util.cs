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
// Name:      Helper.cs
// 
// Created:   01-01-2008 SharedCache.com, rschuetz
// Modified:  01-01-2008 SharedCache.com, rschuetz : Creation
// Modified:  04-01-2008 SharedCache.com, rschuetz : introduction on cache provider to clear() all cache data with one single call instead to iterate over all key's.
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

// 1 - Add this using to use the cache.
using SharedCache.WinServiceCommon.Provider.Cache;

// 2 - Add this using statement to work with Priority
using PRIORITY = SharedCache.WinServiceCommon.IndexusMessage.CacheItemPriority;

using SharedCache.WinServiceCommon;

namespace SharedCache.WinServiceTestClient.Common
{

	/// <summary>
	/// Populates some Helper methods for I/O and Cache
	/// </summary>
	public class Util
	{
		#region Variables
		/// <summary>
		/// Constant file path for region data
		/// </summary>
		private const string regionFile		= @"\Data\Region.xml";
		/// <summary>
		/// Constant file path for country data
		/// </summary>
		private const string countryFile	= @"\Data\Country.xml";
		/// <summary>
		/// Constant file path for reporting data
		/// </summary>
		private const string reportingFile = @"\Data\Reporting.xml";
		#endregion 

		#region Cache Methods

		public static void CacheExtendTtl(string key, DateTime expire)
		{
			IndexusDistributionCache.SharedCache.ExtendTtl(key, expire);
		}

		/// <summary>
		/// The simple method how to add data to the cache.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public static void CacheAdd(string key, object value)
		{
			IndexusDistributionCache.SharedCache.Add(key, value);
		}
		/// <summary>
		/// The simple method how to add data to the cache.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public static void CacheAddWcf(string key, object value)
		{
			IndexusDistributionCache.SharedCache.DataContractAdd(key, value);
		}

		/// <summary>
		/// Adding an item to the cache with an expiration time.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="expires">The expires.</param>
		public static void CacheAdd(string key, object value, DateTime expires)
		{
			IndexusDistributionCache.SharedCache.Add(key, value, expires);
		}

		public static IDictionary<string, DateTime> GetAbsolutExpireDateTime(List<string> keys)
		{
			return IndexusDistributionCache.SharedCache.GetAbsoluteTimeExpiration(keys);
		}

		/// <summary>
		/// Adding an item to the cache with an expiration time in combination to to priority
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="expires">The expires.</param>
		/// <param name="prio">The prio.</param>
		public static void CacheAddPrio(string key, object value, DateTime expires, PRIORITY prio)
		{
			IndexusDistributionCache.SharedCache.Add(key, value, expires, prio);
		}

		/// <summary>
		/// Remove an item from the cache.
		/// </summary>
		/// <param name="key">The key.</param>
		public static void CacheRemove(string key)
		{
			IndexusDistributionCache.SharedCache.Remove(key);
		}

		/// <summary>
		/// Get an Item from the cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static T CacheGet<T>(string key)
		{
			return IndexusDistributionCache.SharedCache.Get<T>(key);
		}

		/// <summary>
		/// Get an Item from the cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static T CacheGetWcf<T>(string key)
		{
			return IndexusDistributionCache.SharedCache.DataContractGet<T>(key);
		}

		public static IndexusStatistic CacheGetStats()
		{
			return IndexusDistributionCache.SharedCache.GetStats();
		}

		public static IndexusStatistic CacheGetStats(string host)
		{
			return IndexusDistributionCache.SharedCache.GetStats(host);
		}

		public static List<string> CacheGetAllKeys()
		{
			return IndexusDistributionCache.SharedCache.GetAllKeys();
		}

		public static List<string> CacheGetAllKeys(string host)
		{
			return IndexusDistributionCache.SharedCache.GetAllKeys(host);
		}

		public static void CacheClear()
		{
			IndexusDistributionCache.SharedCache.Clear();
		}

		public static IDictionary<string, byte[]> CacheMultiGet(List<string> keys)
		{
			return IndexusDistributionCache.SharedCache.MultiGet(keys);
		}

		public static void CacheMultiAdd(IDictionary<string, byte[]> data)
		{
			IndexusDistributionCache.SharedCache.MultiAdd(data);
		}

		public static void CacheMultiDelete(List<string> keys)
		{
			IndexusDistributionCache.SharedCache.MultiDelete(keys);
		}

		public static void CacheRegexRemove(string pattern)
		{
			IndexusDistributionCache.SharedCache.RegexRemove(pattern);
		}

		public static IDictionary<string, byte[]> CacheRegexGet(string pattern)
		{
			return IndexusDistributionCache.SharedCache.RegexGet(pattern);
		}

		#endregion 
		
		#region File Handling

		/// <summary>
		/// Gets the region data fs path.
		/// </summary>
		/// <returns>the correct path to the region data file</returns>
		public static string GetRegionPath()
		{
			return GetBasePath() + regionFile;
		}

		/// <summary>
		/// Gets the country data fs path.
		/// </summary>
		/// <returns>the correct path to the country data file</returns>
		public static string GetCountryPath()
		{
			return GetBasePath() + countryFile;
		}

		/// <summary>
		/// Gets the reporting path.
		/// </summary>
		/// <returns>the correct path to the reporting data file</returns>
		public static string GetReportingPath()
		{
			return GetBasePath() + reportingFile;
		}

		/// <summary>
		/// Gets the base executing fs path.
		/// </summary>
		/// <returns>the current location</returns>
		private static string GetBasePath()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
		}

		#endregion File Handling

		#region Reporting

		public static void AddReport(Reporting report)
		{
			if (report != null)
			{
				BLL.BllReporting.Save(report);
			}
		}

		#endregion Reporting
	}
}
