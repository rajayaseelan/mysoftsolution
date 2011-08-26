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
// Name:      Cleanup.cs
// 
// Created:   23-12-2007 SharedCache.com, rschuetz
// Modified:  23-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// This class defines cache items in cache based on various properties 
	/// and configuration option to clear up memory.
	/// </summary>
	[Serializable]
	public class Cleanup : IComparable<Cleanup>
	{
		#region Enum
		/// <summary>
		/// Defines the sorting order of cleanup objects
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

		#region Properties & private variables
		#region Property: Key
		private string key;
		
		/// <summary>
		/// Gets/sets the Key
		/// </summary>
		public string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.key;  }
		}
		#endregion
		#region Property: Sorting
		private static SortingOrder sorting = SortingOrder.Asc;

		/// <summary>
		/// Gets/sets the Sorting
		/// </summary>
		public static SortingOrder Sorting
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return sorting; }

			[System.Diagnostics.DebuggerStepThrough]
			set { sorting = value; }
		}
		#endregion
		#region Property: CleanupCacheConfiguration
		// private Enums.ServiceCacheCleanUp cleanupCacheConfiguration = Enums.ServiceCacheCleanUp.CACHEITEMPRIORITY;
		private Enums.ServiceCacheCleanUp cleanupCacheConfiguration;
		private static bool cleanupValueParsed = false;
		/// <summary>
		/// Gets/sets the CleanupCacheConfiguration
		/// Defines the cleanup cache configuration, the default value is CACHEITEMPRIORITY [check <see cref="Enums.ServiceCacheCleanUp"/> for more information]
		/// </summary>
		public Enums.ServiceCacheCleanUp CleanupCacheConfiguration
		{
			[System.Diagnostics.DebuggerStepThrough]
			get {
				if (!cleanupValueParsed)
				{
					try
					{
						// validate configuration entry
						this.cleanupCacheConfiguration = Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanup;						
					}
					catch (Exception ex)
					{
						string msg = @"Configuration failure, please validate your [ServiceCacheCleanUp] key, it contains an invalid value!! Current configured value: {0}; As standard the CACHEITEMPRIORITY has been set.";
						Handler.LogHandler.Fatal(string.Format(msg, Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanup), ex);
						this.cleanupCacheConfiguration = Enums.ServiceCacheCleanUp.CACHEITEMPRIORITY;
					}
					cleanupValueParsed = true;
				}

				return this.cleanupCacheConfiguration; 
			}

			[System.Diagnostics.DebuggerStepThrough]
			set { this.cleanupCacheConfiguration = value; }
		}
		#endregion
		#region Property: Prio
		private IndexusMessage.CacheItemPriority prio = IndexusMessage.CacheItemPriority.Normal;

		/// <summary>
		/// Defines the object priority of the cache item.
		/// Gets/sets the Prio
		/// </summary>
		public IndexusMessage.CacheItemPriority Prio
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.prio; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.prio = value; }
		}
		#endregion
		#region Property: Span
		private TimeSpan span;

		/// <summary>
		/// Gets/sets the Span - calculate how much time left until the item 
		/// will be deleted from the cache.
		/// </summary>
		public TimeSpan Span
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.span; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.span = value; }
		}
		#endregion
		#region Property: HitRatio
		private long hitRatio = 0;

		/// <summary>
		/// Gets/sets the HitRatio
		/// </summary>
		public long HitRatio
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.hitRatio; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.hitRatio = value; }
		}
		#endregion
		#region Property: UsageDatetime
		private DateTime usageDatetime;

		/// <summary>
		/// Gets/sets the UsageDatetime
		/// </summary>
		public DateTime UsageDatetime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.usageDatetime; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.usageDatetime = value; }
		}
		#endregion
		#region Property: ObjectSize
		private long objectSize;

		/// <summary>
		/// Gets/sets the ObjectSize
		/// </summary>
		public long ObjectSize
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.objectSize; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.objectSize = value; }
		}
		#endregion
		#region Property: HybridPoint
		private long hybridPoint = -1;

		/// <summary>
		/// Gets/sets the HybridPoint
		/// </summary>
		public long HybridPoint
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.hybridPoint; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.hybridPoint = value; }
		}
		#endregion
		#endregion Properties & private variables

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Cleanup"/> class.
		/// </summary>
		public Cleanup()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cleanup"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="prio">The prio.</param>
		/// <param name="span">The span.</param>
		/// <param name="hitRatio">The hit ratio.</param>
		/// <param name="usageDatetime">The usage datetime.</param>
		/// <param name="objectSize">Size of the object.</param>
		/// <param name="hybridPoint">The hybrid point.</param>
		public Cleanup(string key, IndexusMessage.CacheItemPriority prio, TimeSpan span, long hitRatio, DateTime usageDatetime, long objectSize, int hybridPoint)
			: this()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.key = key;
			this.prio = prio;
			this.span = span;
			this.hitRatio = hitRatio;
			this.usageDatetime = usageDatetime;
			this.objectSize = objectSize;
			this.hybridPoint = hybridPoint;
		}
		#endregion Constructor

		#region Methods
		#region Override Methods
		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Prio: {0}; \tspan: {1}; \tUsageCntr: {2}; \tUsageDateTime: {3}; \tSize: {4}; \tHyb.Pnt: {5};",
				this.prio, this.span, this.hitRatio, this.usageDatetime, this.objectSize, this.hybridPoint);

			return sb.ToString();
		}
		#endregion Override Methods

		#region IComparable<Cleanup> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
		/// </returns>
		public int CompareTo(Cleanup other)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			return this.cleanupCacheConfiguration.CompareTo(other.cleanupCacheConfiguration);
		}

		#endregion

		/// <summary>
		/// Sorts the list for cleanup based on the configuration entry it will sort the 
		/// objects in a correct order to start to delete entries from the cache. Start point
		/// to delete is always the first object.
		/// </summary>
		/// <param name="listToSort">The list to sort, the list contains objects of type <see cref="Cleanup"/></param>
		private void SortListForCleanup(List<Cleanup> listToSort)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			switch (this.cleanupCacheConfiguration)
			{
				case Enums.ServiceCacheCleanUp.HYBRID:
					Cleanup.Sorting = SortingOrder.Desc;
					listToSort.Sort(Cleanup.CacheItemHybridPoints);
					break;
				case Enums.ServiceCacheCleanUp.LFU:
					Cleanup.Sorting = SortingOrder.Desc;
					listToSort.Sort(Cleanup.CacheItemHitRatio);
					break;
				case Enums.ServiceCacheCleanUp.LLF:
				case Enums.ServiceCacheCleanUp.SIZE:
					Cleanup.Sorting = SortingOrder.Desc;
					listToSort.Sort(Cleanup.CacheItemObjectSize);
					break;
				case Enums.ServiceCacheCleanUp.LRU:
					Cleanup.Sorting = SortingOrder.Desc;
					listToSort.Sort(Cleanup.CacheItemHitRatio);
					break;
				case Enums.ServiceCacheCleanUp.TIMEBASED:
					Cleanup.Sorting = SortingOrder.Desc;
					listToSort.Sort(Cleanup.CacheItemUsageDateTime);
					break;
			}
		}

		#endregion Methods

		#region static specific comparable methods

		#region CacheItemPriority
		/// <summary>
		/// comparsion based on object priority
		/// </summary>
		public static Comparison<Cleanup> CacheItemPriority =
			delegate(Cleanup cu1, Cleanup cu2)
			{
				if (Cleanup.Sorting == SortingOrder.Asc)
				{
					return cu1.prio.CompareTo(cu2.prio);
				}
				else
				{
					return cu2.prio.CompareTo(cu1.prio);
				}
			};
		#endregion CacheItemPriority

		#region CacheItemTimeSpan
		/// <summary>
		/// Comparsion based on cache item time span
		/// </summary>
		public static Comparison<Cleanup> CacheItemTimeSpan =
			delegate(Cleanup cu1, Cleanup cu2)
			{
				if (Cleanup.Sorting == SortingOrder.Asc)
				{
					return cu1.span.CompareTo(cu2.span);
				}
				else
				{
					return cu2.span.CompareTo(cu1.span);
				}
			};
		#endregion CacheItemTimeSpan

		#region CacheItemHitRatio
		/// <summary>
		/// Comparsion based on item hit ratio
		/// </summary>
		public static Comparison<Cleanup> CacheItemHitRatio =
			delegate(Cleanup cu1, Cleanup cu2)
			{
				if (Cleanup.Sorting == SortingOrder.Asc)
				{
					return cu1.hitRatio.CompareTo(cu2.hitRatio);
				}
				else
				{
					return cu2.hitRatio.CompareTo(cu1.hitRatio);
				}
			};
		#endregion CacheItemHitRatio

		#region CacheItemUsageDateTime
		/// <summary>
		/// Comparsion based on open usage date time
		/// </summary>
		public static Comparison<Cleanup> CacheItemUsageDateTime =
			delegate(Cleanup cu1, Cleanup cu2)
			{
				if (Cleanup.Sorting == SortingOrder.Asc)
				{
					return cu1.usageDatetime.CompareTo(cu2.usageDatetime);
				}
				else
				{
					return cu2.usageDatetime.CompareTo(cu1.usageDatetime);
				}
			};
		#endregion CacheItemUsageDateTime

		#region CacheItemObjectSize
		/// <summary>
		/// Comparsion based on object size
		/// </summary>
		public static Comparison<Cleanup> CacheItemObjectSize =
			delegate(Cleanup cu1, Cleanup cu2)
			{
				if (Cleanup.Sorting == SortingOrder.Asc)
				{
					return cu1.objectSize.CompareTo(cu2.objectSize);
				}
				else
				{
					return cu2.objectSize.CompareTo(cu1.objectSize);
				}
			};
		#endregion CacheItemObjectSize

		#region CacheItemHybridPoints
		/// <summary>
		/// Specific comparsion based on hybrid points.
		/// </summary>
		public static Comparison<Cleanup> CacheItemHybridPoints =
			delegate(Cleanup cu1, Cleanup cu2)
			{
				if (Cleanup.Sorting == SortingOrder.Asc)
				{
					return cu1.hybridPoint.CompareTo(cu2.hybridPoint);
				}
				else
				{
					return cu2.hybridPoint.CompareTo(cu1.hybridPoint);
				}
			};
		#endregion CacheItemHybridPoints

		#endregion static specific comparable methods
	}
}
