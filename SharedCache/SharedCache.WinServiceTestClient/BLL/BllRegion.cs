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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SharedCache.WinServiceTestClient.BLL
{
	/// <summary>
	/// Business Logic Layer for Region relevant Data
	/// </summary>
	public class BllRegion
	{
		#region Singleton: DAL.DalRegion
		private DAL.DalRegion region;
		/// <summary>
		/// Singleton for <see cref="DAL.DalRegion" />
		/// </summary>
		public DAL.DalRegion Region
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.region == null)
					this.region = new DAL.DalRegion();
		
				return this.region;
			}
		}
		#endregion

		#region Specific Cache Keys
		/// <summary>
		/// Defines a shared cache key for getting all data
		/// </summary>
		private const string cacheKeyAllRegion = @"region_sharedCacheKey_AllData";
		/// <summary>
		/// Defines a shared cache key for indvidual item based on its id.
		/// </summary>
		private const string cacheKeyIdRegion = @"region_sharedCacheKey_DataByID_{0}";
		/// <summary>
		/// Defines a shared cache key for indvidual item received by name
		/// </summary>
		private const string cacheKeyNameRegion = @"region_sharedCacheKey_DataByName_{0}";
		#endregion Specific Cache Keys

		#region Methods

		/// <summary>
		/// Gets the by country id.
		/// </summary>
		/// <param name="name">The county name.</param>
		/// <returns>
		/// A list of <see cref="Common.Region"/> objects
		/// </returns>
		public Common.Region GetByName(string name)
		{
			string key = string.Format(cacheKeyNameRegion, name);
			Common.Region result = Common.Util.CacheGet<Common.Region>(key);

			if (result == null)
			{
				foreach (Common.Region region in this.GetAll())
				{
					if (name.Equals(region.Name))
					{
						result = region;
					}
				}

				// now we can add it to the cache
				Common.Util.CacheAdd(key, result);
			}

			return result;
		}

		/// <summary>
		/// Gets the by country id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>A list of <see cref="Common.Region"/> objects</returns>
		public List<Common.Region> GetByCountryId(int id)
		{
			// create a unique key for this list of items.
			string key = string.Format(cacheKeyIdRegion, id.ToString());

			List<Common.Region> result = Common.Util.CacheGet<List<Common.Region>>(key);

			if (result == null)
			{
				result = new List<Common.Region>();

				// you can also use: this.Region.GetRegionByCountryId(id); instead of this.GetAll()
				foreach (Common.Region region in this.GetAll())
				{
					if (id.Equals(region.CountryId))
					{
						result.Add(region);
					}
				}

				// now we can add it to the cache
				Common.Util.CacheAdd(key, result);
			}

			return result;
		}

		/// <summary>
		/// Get all items
		/// </summary>
		/// <returns>A list of <see cref="Common.Region"/> objects</returns>
		public List<Common.Region> GetAll()
		{
			List<Common.Region> result = Common.Util.CacheGet<List<Common.Region>>(cacheKeyAllRegion);
			if (result == null)
			{
				Console.WriteLine(@"region data loaded from XML File!");
				result = this.Region.GetAllRegion();

				// now we can add it to the cache
				Common.Util.CacheAdd(cacheKeyAllRegion, result);
			}
			return result;
		}
		
		#endregion Methods
	}
}
