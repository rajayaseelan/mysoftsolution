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
// Name:      Util.cs
// 
// Created:   17-01-2008 SharedCache.com, rschuetz
// Modified:  17-01-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon.Handler.Generic
{
	/// <summary>
	/// Offers some generic utility methods
	/// </summary>
	public static class Util
	{
		#region Enum
		/// <summary>
		/// Defines the sorting order
		/// </summary>
		public enum SortingOrder
		{
			/// <summary>
			/// ascending sorting order
			/// </summary>
			Asc,
			/// <summary>
			/// descending sorting order
			/// </summary>
			Desc
		}
		#endregion Enum

		/// <summary>
		/// Sorts the dictionary in Decending order - default
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public static Dictionary<string, long> SortDictionaryDesc(Dictionary<string, long> data)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Util).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return Sort(data, SortingOrder.Desc);
		}

		/// <summary>
		/// Sorts the dictionary with specific 
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public static Dictionary<string, long> SortDictionaryAsc(Dictionary<string, long> data)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Util).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return Sort(data, SortingOrder.Asc);
		}

		/// <summary>
		/// Sorts the specified data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="sort">The sort.</param>
		/// <returns></returns>
		private static Dictionary<string, long> Sort(Dictionary<string, long> data, SortingOrder sort)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Util).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>(data);
			Dictionary<string, long> returnVal = new Dictionary<string, long>();
			result.Sort(
					 delegate(
						KeyValuePair<string, long> first,
						KeyValuePair<string, long> second)
					 {
						 if (sort == SortingOrder.Desc)
						 {
							 return second.Value.CompareTo(first.Value);
						 }
						 else
						 {
							 return first.Value.CompareTo(second.Value);
						 }
					 }
				);

			foreach (KeyValuePair<string, long> kvp in result)
				returnVal.Add(kvp.Key, kvp.Value);

			return returnVal;
		}

	}
}
