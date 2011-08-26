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
// Name:      CacheCleanup.cs
// 
// Created:   22-12-2007 SharedCache.com, rschuetz
// Modified:  22-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  23-12-2007 SharedCache.com, rschuetz : moved Cleanup into its own file -> Cleanup.cs
// Modified:  23-12-2007 SharedCache.com, rschuetz : created public methods to access cleanup logic, prepared purge method to run from multible threads
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated consistency of logging calls
// Modified:  11-01-2008 SharedCache.com, rschuetz : pre_release_1.0.2.132 - protocol changed - added string key and byte[] payload instead KeyValuePair
// Modified:  23-02-2008 SharedCache.com, rschuetz : performacne refactoring, instead of using list we changed managment to Dictonary
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// Logic within this class maintains cache data and statistics, like increasing key
	/// hit counts this is used in case the cache reaches its maximum memory capacity
	/// </summary>
	[Serializable]
	public class CacheCleanup
	{
		#region Singleton / Properties & private Variables
		/// <summary>
		/// needed for lock(){} operations 
		/// </summary>
		private static object bulkObject = new object();
		#region Property: CleanupCacheConfiguration
		private Enums.ServiceCacheCleanUp cleanupCacheConfiguration = Enums.ServiceCacheCleanUp.CACHEITEMPRIORITY;

		/// <summary>
		/// Gets/sets the CleanupCacheConfiguration
		/// Defines the cleanup cache configuration, the default value is CACHEITEMPRIORITY [check <see cref="Enums.ServiceCacheCleanUp"/> for more information]
		/// </summary>
		public Enums.ServiceCacheCleanUp CleanupCacheConfiguration
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.cleanupCacheConfiguration; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.cleanupCacheConfiguration = value; }
		}
		#endregion
		#region Property: CacheAmountOfObjects
		private long cacheAmountOfObjects = -1;

		/// <summary>
		/// Gets the CacheAmountOfObjects (extern readonly)
		/// </summary>
		public long CacheAmountOfObjects
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.cacheAmountOfObjects; }
		}
		#endregion
		#region Property: CacheFillFactor
		private long cacheFillFactor = 90;

		/// <summary>
		/// Gets/sets the CacheFillFactor
		/// </summary>
		public long CacheFillFactor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.cacheFillFactor; }
		}
		#endregion
		#region Property: PurgeIsRunning
		private bool purgeIsRunning;

		/// <summary>
		/// Gets the PurgeIsRunning
		/// </summary>
		public bool PurgeIsRunning
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.purgeIsRunning; }
		}
		#endregion
		private Dictionary<string, Cleanup> cleanupDict;
		/// <summary>
		/// Gets the cleanup list.
		/// </summary>
		/// <value>The cleanup list.</value>
		public Dictionary<string, Cleanup> CleanupDict
		{
			[System.Diagnostics.DebuggerStepThrough]
			get {
				if (this.cleanupDict == null)
				{
					this.cleanupDict = new Dictionary<string, Cleanup>();
				}
				return this.cleanupDict;
			}
		}

		#endregion Singleton / Properties & private Variables

		#region CTor
		/// <summary>
		/// Initializes a new instance of the <see cref="CacheCleanup"/> class.
		/// </summary>
		public CacheCleanup()
		{
			#region Access Log
			Handler.LogHandler.Tracking(
				"Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;"
			);
			#endregion Access Log

			#region retrieve configuration information
			#region validate configuration entry for cleanup strategy - ServiceCacheCleanUp
			try
			{
				this.cleanupCacheConfiguration = Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanup;
			}
			catch (Exception ex)
			{
				string msg = @"Configuration failure, please validate your [ServiceCacheCleanUp] key, it contains an invalid value!! Current configured value: {0}; As standard the CACHEITEMPRIORITY has been set.";
				Handler.LogHandler.Fatal(string.Format(msg, Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanup), ex);
				this.cleanupCacheConfiguration = Enums.ServiceCacheCleanUp.LRU;
			}
			#endregion validate configuration entry for cleanup strategy - ServiceCacheCleanUp
			#region retrieve configured maximum memory cache size - CacheAmountOfObjects
			try
			{
				this.cacheAmountOfObjects =
					long.Parse(Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects.ToString())
					* (1024 * 1024);
				
			}
			catch (Exception ex)
			{
				string msg = @"Configuration failure, please validate your [CacheAmountOfObjects] key, it contains an invalid value!! Current configured value: {0}; standard taken, value:-1. You may explorer now OutOfMemoryExceptions in your log files.";
				Handler.LogHandler.Fatal(string.Format(msg, Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountOfObjects), ex);
			}
			#endregion retrieve configured maximum memory cache size - CacheAmountOfObjects
			#region retrieve configured fill factor - CacheAmountFillFactorInPercentage
			try
			{
				this.cacheFillFactor =
					long.Parse(Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountFillFactorInPercentage.ToString());
				
			}
			catch (Exception ex)
			{
				string msg = @"Configuration failure, please validate your [CacheAmountFillFactorInPercentage] key, it contains an invalid value!! Current configured value: {0}; standard taken, value:90.";
				Handler.LogHandler.Fatal(string.Format(msg, Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.CacheAmountFillFactorInPercentage), ex);
				this.cacheFillFactor = 90;
			}
			#endregion retrieve configured fill factor - CacheAmountFillFactorInPercentage
			#endregion retrieve configuration information
		}
		#endregion CTor

		#region Methods

		#region Static Methods [HybridChecksum]
		/// <summary>
		/// checksum for Hybrid cache clearup, makes a recursive call 
		/// until it receives a smaller value then: 100000
		/// </summary>
		/// <param name="para">The para.</param>
		/// <returns>a smaller value then: 100000</returns>
		public static long HybridChecksum(long para)
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheCleanup).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			if (para < 100000) return para;
			return HybridChecksum(para / 100000) + para % 100000;
		}

		/// <summary>
		/// Calculates the hybrid checksum, this is used to
		/// create rate for every specific object in the cache
		/// based on various object attributes.
		/// <remarks>
		/// The calculate done like this: HybridChecksum((c.HitRatio + ((c.ObjectSize / 1024) + (c.Span.Ticks / 1024) + (c.UsageDatetime.Ticks / 1024))));
		/// </remarks>
		/// </summary>
		/// <param name="c">an object of type <see cref="Cleanup"/></param>
		/// <param name="currentConfiguration">The current configuration.</param>
		/// <returns>a rate as <see cref="long"/></returns>
		public static long CalculateHybridChecksum(Cleanup c, Enums.ServiceCacheCleanUp currentConfiguration)
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(CacheCleanup).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			// do not make Hybrid calculations on other cleanup strategy - boost performance
			if (currentConfiguration == Enums.ServiceCacheCleanUp.HYBRID)
			{
				return HybridChecksum((c.HitRatio + ((c.ObjectSize / 1024) + (c.Span.Ticks / 1024) + (c.UsageDatetime.Ticks / 1024))));
			}
			else
			{
				return 0;
			}
		}
		#endregion Static Methods

		/// <summary>
		/// Updates the specified msg or its updating the available record if its already added.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		public void Update(IndexusMessage msg)
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
				Cleanup c = null;
				if (this.CleanupDict.ContainsKey(msg.Key))
				{
					c = this.CleanupDict[msg.Key];
					// update
					if (msg.Payload != null)
					{
						c.ObjectSize = msg.Payload.Length == 1 ? c.ObjectSize : msg.Payload.Length;
					}
					else
					{
						c.ObjectSize = 0;
					}
					
					c.Prio = msg.ItemPriority;
					c.HitRatio += 1;
					c.HybridPoint = CalculateHybridChecksum(c, this.CleanupCacheConfiguration);
					c.Span = msg.Expires.Subtract(DateTime.Now);
					c.UsageDatetime = DateTime.Now;
				}
				else
				{ 
					// add
					// object is not available, create a new instance / calculations / and add it to the list.
					Cleanup cleanup = new Cleanup(msg.Key, msg.ItemPriority, msg.Expires.Subtract(DateTime.Now), 0, DateTime.Now, msg.Payload != null ? msg.Payload.Length : 0 , 0);
					cleanup.HybridPoint = CalculateHybridChecksum(cleanup, this.CleanupCacheConfiguration);
					this.CleanupDict[msg.Key] = cleanup;
				}
			}
		}

		/// <summary>
		/// Gets the access stat list.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, long> GetAccessStatList()
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			Dictionary<string, long> result = new Dictionary<string, long>();
			List<Cleanup> cleanupCopy = null;
			
			lock (bulkObject)
			{
				cleanupCopy = new List<Cleanup>(this.CleanupDict.Values);
			}

			
			Cleanup.Sorting = Cleanup.SortingOrder.Desc;
			cleanupCopy.Sort(Cleanup.CacheItemHitRatio);

			int i = 0;
			foreach (Cleanup c in cleanupCopy)
			{
				result.Add(c.Key, c.HitRatio);
				// do not send back more then 20 keys
				if (++i == 20)
					break;
			}

			return result;
		}

		/// <summary>
		/// Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		public void Remove(string key)
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log
			lock(bulkObject)
			{
				if (this.CleanupDict.ContainsKey(key))
				{
					this.CleanupDict.Remove(key);
				}
			}
			GC.WaitForPendingFinalizers();
		}

		/// <summary>
		/// Clears this instance.
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
				this.CleanupDict.Clear();				
			}

			GC.Collect();
		}

		/// <summary>
		/// Purges this instance, free memory
		/// step 1: sort list based on requested configuration
		/// step 2: start to remove items from the list.
		/// </summary>
		/// <param name="actualCacheSize">Actual size of the cache.</param>
		/// <returns></returns>
		public List<string> Purge(long actualCacheSize)
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
				// do not run purge concurrently, there we need to 
				// maintain a status if purge is running
				if (this.purgeIsRunning) return null;

				// set the flag to prevent additional threads to access 
				// and to wait for the following lock.
				this.purgeIsRunning = true;
			}

			List<string> removeObjectKeys = new List<string>();
			List<Cleanup> removeObjectValues;
			lock (bulkObject)
			{
				removeObjectValues = new List<Cleanup>(this.CleanupDict.Values);
			}
			lock (bulkObject)
			{
				try
				{
					// sort list based on requested configuration before it 
					// starts to purge objects from the cache
					switch (this.cleanupCacheConfiguration)
					{
						#region Sorting
						case Enums.ServiceCacheCleanUp.LRU:
							Cleanup.Sorting = Cleanup.SortingOrder.Asc;
							removeObjectValues.Sort(Cleanup.CacheItemUsageDateTime);
							break;
						case Enums.ServiceCacheCleanUp.LFU:
							Cleanup.Sorting = Cleanup.SortingOrder.Asc;
							removeObjectValues.Sort(Cleanup.CacheItemHitRatio);
							break;
						case Enums.ServiceCacheCleanUp.TIMEBASED:
							Cleanup.Sorting = Cleanup.SortingOrder.Asc;
							removeObjectValues.Sort(Cleanup.CacheItemTimeSpan);
							break;
						case Enums.ServiceCacheCleanUp.SIZE:
							Cleanup.Sorting = Cleanup.SortingOrder.Desc;
							removeObjectValues.Sort(Cleanup.CacheItemObjectSize);
							break;
						case Enums.ServiceCacheCleanUp.LLF:
							Cleanup.Sorting = Cleanup.SortingOrder.Asc;
							removeObjectValues.Sort(Cleanup.CacheItemObjectSize);
							break;
						case Enums.ServiceCacheCleanUp.HYBRID:
							Cleanup.Sorting = Cleanup.SortingOrder.Asc;
							removeObjectValues.Sort(Cleanup.CacheItemHybridPoints);
							break;
						default:
							Cleanup.Sorting = Cleanup.SortingOrder.Asc;
							removeObjectValues.Sort(Cleanup.CacheItemPriority);
							break;
						#endregion
					}

					do
					{
						// store a list to delete it afterwards from the Cache itself
						removeObjectKeys.Add(removeObjectValues[0].Key);

						// calcualte actual cache size
						actualCacheSize -= removeObjectValues[0].ObjectSize;

						// phisically remove it
						this.CleanupDict.Remove(removeObjectValues[0].Key);

					} while (!((actualCacheSize * this.cacheFillFactor / this.cacheAmountOfObjects) < this.cacheFillFactor));
				}
				finally
				{
					this.purgeIsRunning = false;
				}
			}
			return removeObjectKeys;
		}
		#endregion Methods
	}
}
