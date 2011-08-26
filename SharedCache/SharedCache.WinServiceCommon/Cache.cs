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
// Name:      Cache.cs
// 
// Created:   30-01-2007 SharedCache.com, rschuetz
// Modified:  30-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  16-12-2007 SharedCache.com, rschuetz : added calculated cache size, upon each action ( add || remove ) it updates the value
// Modified:  18-12-2007 SharedCache.com, rschuetz : changed Cache from HybridDictonary into Dictionary<string, byte[]>() upon both constructors
// Modified:  18-12-2007 SharedCache.com, rschuetz : since SharedCache works internally with byte[] instead of objects almoast all needed code has been adapted
// Modified:  18-12-2007 SharedCache.com, rschuetz : added regions
// Modified:  18-12-2007 SharedCache.com, rschuetz : implementation of Cache Size [CalculatedCacheSize], the property is configurable over the App.config file: CacheAmountOfObjects
// Modified:  20-01-2008 SharedCache.com, rschuetz : within Insert method calculated cache size were not be updated.
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Threading;


namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// Provides generic object cache/pool that based on
	/// CLR garbage collector (GC) logic.
	/// </summary>
	/// <remarks>
	/// You can extend Cache behaviour in distributed
	/// environment by lifetime services setting
	/// of Your classes.
	/// </remarks>
	[Serializable]
	public sealed class Cache
	{
		#region Properties & private Variables
		#region Property: CalculatedCacheSize
		private long calculatedCacheSize;

		/// <summary>
		/// Gets/sets the CalculatedCacheSize
		/// </summary>
		public long CalculatedCacheSize
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.calculatedCacheSize; }
		}
		#endregion
		#region Singleton: CacheCleanup
		private CacheCleanup cacheCleanup;
		/// <summary>
		/// Singleton for <see cref="CacheCleanup" />
		/// </summary>
		public CacheCleanup CacheCleanup
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (this.cacheCleanup == null)
					this.cacheCleanup = new CacheCleanup();
		
				return this.cacheCleanup;
			}
		}
		#endregion
		/// <summary>
		/// The Cache Dictionary which contains all data
		/// </summary>
		readonly Dictionary<string, byte[]> dict;
		#endregion Properties & private Variables

		#region Constructors
		/// <summary>
		/// Creates an empty Cache.
		/// </summary>
		public Cache()
		{
			dict = new Dictionary<string, byte[]>();
			this.calculatedCacheSize = 0;
		}
		/// <summary>
		/// Creates a Cache with the specified initial size.
		/// </summary>
		/// <param name="initialSize">The approximate number of objects that the Cache can initially contain.</param>
		public Cache(int initialSize) : this()
		{
			dict = new Dictionary<string, byte[]>(initialSize);			
		}
		#endregion Constructors

		#region Methods

		#region General Methods [Amount / Size / Clear / GetAllKeys]

		/// <summary>
		/// returns the amount this instance contains.
		/// </summary>
		/// <returns></returns>
		public long Amount()
		{
			long result = 0;
			lock (dict)
			{
				result = dict.Count;
			}
			return result;
		}

		/// <summary>
		/// calculates the actual size of this instance.
		/// <remarks>
		///	This is a very heavy operation, please consider not to use this to often then 
		/// it locks the cache exclusivly which is very expensive!!!
		/// </remarks>
		/// </summary>
		/// <returns>a <see cref="long"/> object with the actual Dictionary size</returns>
		public long Size()
		{
			Dictionary<string, byte[]> di = null;
			long size = 0;

			// consider, this is very expensive!!
			lock (dict)
			{
				di = new Dictionary<string, byte[]>(dict);
			}
			foreach (KeyValuePair<string, byte[]> de in di)
			{
				size += de.Value.Length;
			}
			return size;
		}

		/// <summary>
		/// Retrive all key's
		/// </summary>
		/// <returns>returns a list with all key's as a <see cref="string"/> objects.</returns>
		public List<string> GetAllKeys()
		{
			Dictionary<string, byte[]> hd = null;
			lock (dict)
			{
				hd = new Dictionary<string, byte[]>(dict);
			}
			return new List<string>(hd.Keys);
		}
		#endregion General Methods [Amount / Size / Clear / GetAllKeys]

		#region Add / Insert Methods
		/// <summary>
		/// Adds an object that identified by the provided key to the Cache.
		/// if the key is already available it will replace it.
		/// </summary>
		/// <param name="key">The Object to use as the key of the object to add.</param>
		/// <param name="value">The Object to add. </param>
		public void Add(string key, byte[] value)
		{
			try
			{
				lock (dict)
				{
					dict[key] = value;
					this.calculatedCacheSize += value.Length;
				}
			}
			catch (Exception ex)
			{
				Handler.LogHandler.Force(@"Add Failed - " + ex.Message);
			}
		}

		/// <summary>
		/// Adds the specified KeyValuePair
		/// </summary>
		/// <param name="kpv">The KPV. A <see cref="T:System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;"/> Object.</param>
		public void Add(KeyValuePair<string, byte[]> kpv)
		{
			this.Add(kpv.Key, kpv.Value);
		}

		/// <summary>
		/// Inserts the specified key.
		/// </summary>
		/// <param name="key">The key. A <see cref="T:System.String"/> Object.</param>
		/// <param name="value">The value. A <see cref="T:System.Object"/> Object.</param>
		public void Insert(string key, byte[] value)
		{
			lock (dict)
			{
				dict.Add(key, value);
				this.calculatedCacheSize += value.Length;
			}
		}
		#endregion Add / Insert Methods

		#region Retrive Method
		/// <summary>
		/// Gets the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>a <see cref="object"/> object.</returns>
		public byte[] Get(string key)
		{
			byte[] o = null;
			try
			{
				lock (dict)
				{
					if (dict.ContainsKey(key))
					{
						o = dict[key];
					}
					else
						o = null;
				}
			}
			catch (Exception ex)
			{
				Handler.LogHandler.Info(@"Get Failed!", ex);
			}
			return o;
		}
		#endregion Retrive Methods

		#region Remove / Clear Methods
		/// <summary>
		/// Removes the object that identified by the specified key from the Cache.
		/// </summary>
		/// <param name="key">The key of the object to remove.</param>
		public void Remove(string key)
		{
			try
			{
				lock (dict)
				{
					if (dict.ContainsKey(key))
					{
						this.calculatedCacheSize -= dict[key].Length;
						dict.Remove(key);
					}
				}
				GC.WaitForPendingFinalizers();
			}
			catch (Exception ex)
			{
				Handler.LogHandler.Info(@"Remove failed!", ex);
			}
		}

		/// <summary>
		/// Removes all objects from the Cache.
		/// </summary>
		public void Clear()
		{
			lock (dict)
			{
				dict.Clear();
				this.calculatedCacheSize = 0;
			}
			// ISSUE: Not sure if this has to be here
			GC.Collect();
		}
		#endregion Remove Methods

		#endregion Methods
	}
}
