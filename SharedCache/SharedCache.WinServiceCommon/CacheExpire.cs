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
// Name:      CacheExpire.cs
// 
// Modified:  03-01-2008 SharedCache.com, rschuetz : introduction to Clear() to enable to delete all items at once from the provider
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// Modified:  18-05-2008 SharedCache.com, rschuetz : Added return value for Remove Method
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// <b>Manage Data which expires in cache based on provided DateTime</b>
	/// </summary>
	public class CacheExpire
	{
		#region Variables & Properties
		/// <summary>
		/// Defines a dic object with a <see cref="string"/> and <see cref="DateTime"/>.
		/// </summary>
		private static Dictionary<string, DateTime> expireTable = null;
		/// <summary>
		/// defines a bulk <see cref="object"/> to manage concurrency.
		/// </summary>
		private static object bulkObject = new object();

		#region Property: Enable
		private bool enable = false;
		
		/// <summary>
		/// Gets/sets the Enable
		/// </summary>
		public bool Enable
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.enable;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.enable = value;  }
		}
		#endregion

		#endregion Variables & Properties

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheExpire"/> class.
		/// </summary>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		public CacheExpire(bool enable)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			this.enable = enable;

			expireTable = new Dictionary<string, DateTime>();
		}

		/// <summary>
		/// Dumps the cache item at.
		/// If key is already available, it will update the expire date and time.
		/// </summary>
		/// <param name="key">The key. A <see cref="T:System.String"/> Object.</param>
		/// <param name="expire">The expire. A <see cref="T:System.DateTime"/> Object.</param>
		public void DumpCacheItemAt(string key, DateTime expire)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			// check to extend the expire date
			if (expireTable.ContainsKey(key))
			{
				lock (bulkObject)
				{
					expireTable[key] = expire;
				}
			}
			else
			{
				// first time 
				lock (bulkObject)
				{
					expireTable.Add(key, expire);
				}
			}
		}

		/// <summary>
		/// Clear all items within the expiry table.
		/// </summary>
		public void Clear()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			lock (bulkObject)
			{
				expireTable.Clear();
			}
			GC.Collect();
		}

		/// <summary>
		/// Removes the specified key.
		/// </summary>
		/// <param name="key">The key. A <see cref="T:System.String"/> Object.</param>
		public void Remove(string key)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (expireTable.ContainsKey(key))
			{
				lock (bulkObject)
				{
					expireTable.Remove(key);
				}
				GC.WaitForPendingFinalizers();
			}
		}

		/// <summary>
		/// Check if an Item within cache is already expired upon Get Request
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool CheckExpired(string key)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (expireTable.ContainsKey(key))
			{
				DateTime dt = DateTime.MinValue;
				lock (bulkObject)
				{
					dt = expireTable[key];
				}
				if (DateTime.Now > dt)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Check if an Item within cache is already expired upon Get Request
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public DateTime GetExpireDateTime(string key)
		{
			#region Access Log
#if TRACE			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (expireTable.ContainsKey(key))
			{
				DateTime dt = DateTime.MinValue;
				lock (bulkObject)
				{
					dt = expireTable[key];
				}
				return dt;
			}

			return DateTime.MinValue;
		}


		/// <summary>
		/// Returns a list of <see cref="string"/> with key's to clean up from 
		/// the cache.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.List&lt;System.String&gt;"/> Object.
		/// </returns>
		public List<string> CleanUp()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			Dictionary<string, DateTime> dict = null;
			List<string> result = new List<string>();

			// create a copy of current status.
			lock (bulkObject)
			{
				dict = new Dictionary<string, DateTime>(expireTable);
			}
			if (dict != null)
			{
				foreach (KeyValuePair<string, DateTime> kpv in dict)
				{
					// QuickFix
					if (kpv.Value != DateTime.MaxValue)
					{
						// kpv.Value contains the datetime when this item 
						// needs to expire
						if (DateTime.Now > kpv.Value)
						{
							result.Add(kpv.Key);
						}
					}
				}
			}
			return result;
		}
	}
}
