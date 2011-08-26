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
// Name:      BllCountry.cs
// 
// Created:   01-01-2008 SharedCache.com, rschuetz
// Modified:  01-01-2008 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

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
	/// Business Logic Layer for Country relevant Data
	/// </summary>
	public class BllCountry
	{
		#region Singleton: DAL.DalCountry
		private DAL.DalCountry country;
		/// <summary>
		/// Singleton for <see cref="DAL.DalCountry" />
		/// </summary>
		private DAL.DalCountry Country
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.country == null)
					this.country = new DAL.DalCountry();
		
				return this.country;
			}
		}
		#endregion

		#region Specific Cache Keys
		/// <summary>
		/// Defines a shared cache key for getting all data
		/// </summary>
		private const string cacheKeyAllCountry = @"country_sharedCacheKey_AllData";
		/// <summary>
		/// Defines a shared cache key for indvidual item based on its id.
		/// </summary>
		private const string cacheKeyCountryById = @"country_sharedCacheKey_ById_{0}";
		/// <summary>
		/// Defines a shared cache key for indvidual item received by name
		/// </summary>
		private const string cacheKeyCountryByName = @"country_sharedCacheKey_ByName_{0}";
		#endregion Specific Cache Keys

		#region Methods

		/// <summary>
		/// Get item by name.
		/// </summary>
		/// <param name="name">The country name.</param>
		/// <param name="loadRegion">if set to <c>true</c> [load region].</param>
		/// <returns>
		/// A list of <see cref="Common.Country"/> objects
		/// </returns>
		public Common.Country GetByName(string name, bool loadRegion)
		{
			// create a unique key for this item.
			string key = string.Format(cacheKeyCountryByName, name);

			Common.Country result = Common.Util.CacheGet<Common.Country>(key);

			if (result == null)
			{
				foreach (Common.Country country in this.GetAll(false, loadRegion))
				{
					if (name.Equals(country.Name))
					{
						result = country;
						break;
					}
				}

				// now we can add it to the cache
				Common.Util.CacheAdd(key, result);
			}

			return result;
		}

		/// <summary>
		/// Get item by id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="loadRegion">if set to <c>true</c> [load region].</param>
		/// <returns>
		/// A list of <see cref="Common.Country"/> objects
		/// </returns>
		public Common.Country GetById(int id, bool loadRegion)
		{
			try
			{
				// create a unique key for this item.
				string key = string.Format(cacheKeyCountryById, id.ToString());

				Common.Country result = Common.Util.CacheGet<Common.Country>(key);

				if (result == null)
				{

					foreach (Common.Country country in this.GetAll(false, loadRegion))
					{
						if (id.Equals(country.CountryId))
						{
							result = country;
							break;
						}
					}

					if (result == null)
					{
						return null;
					}

					// now we can add it to the cache
					Common.Util.CacheAdd(key, result);
				}
				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine(@"Error: " + ex.Message);
				throw ex;
			}			
		}

		/// <summary>
		/// Get all country items
		/// </summary>
		/// <param name="withPrint">if set to <c>true</c> [with print].</param>
		/// <param name="loadRegion">if set to <c>true</c> [load region].</param>
		/// <returns>
		/// A list of <see cref="Common.Country"/> objects
		/// </returns>
		public List<Common.Country> GetAll(bool withPrint, bool loadRegion)
		{
			string key = cacheKeyAllCountry + loadRegion.ToString();
			List<Common.Country> result = Common.Util.CacheGet<List<Common.Country>>(key);

			if (result == null)
			{
				Console.WriteLine(@"country data loaded from XML File!");
				result = this.Country.GetAllCountry();

				// now we can add it to the cache
				Common.Util.CacheAdd(key, result);
			}

			// loading all regions which are available for the country id
			if (loadRegion)
			{
				foreach (Common.Country country in result)
				{
					country.Region = new BllRegion().GetByCountryId(country.CountryId);
					if (withPrint)
					{
						Console.WriteLine("\t{0}", country.Name);
						foreach (Common.Region region in country.Region)
						{
							Console.WriteLine("\t\t - {0}", region.Name);
						}
					}
						
				}				
			}

			return result;
		}

		#endregion Methods		
	}
}
