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
// Name:      IndexusStatistic.cs
// 
// Created:   29-01-2007 SharedCache.com, rschuetz
// Modified:  29-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  18-12-2007 SharedCache.com, rschuetz : since SharedCache works internally with byte[] instead of objects almoast all needed code has been adapted
// Modified:  18-12-2007 SharedCache.com, rschuetz : changes done in overrided method ToString(), the method checks first if value is 0 - this prevents DividedByZeroException
// Modified:  20-01-2008 SharedCache.com, rschuetz : added public getters to all Api variables
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon
{

	[Serializable]
	public class ServerStats
	{
		#region Property: Name
		private string name;

		/// <summary>
		/// Gets/sets the Name
		/// </summary>
		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.name; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.name = value; }
		}
		#endregion
		#region Property: AmountOfObjects
		private long amountOfObjects;

		/// <summary>
		/// Gets/sets the AmountOfObjects
		/// </summary>
		public long AmountOfObjects
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.amountOfObjects; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.amountOfObjects = value; }
		}
		#endregion
		#region Property: CacheSize
		private long cacheSize;

		/// <summary>
		/// Gets/sets the CacheSize
		/// </summary>
		public long CacheSize
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.cacheSize; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.cacheSize = value; }
		}
		#endregion
		#region Property: Long> UsageList
		private Dictionary<string, long> usageList;

		/// <summary>
		/// Gets/sets the Long> NodeUsageList
		/// </summary>
		public Dictionary<string, long> UsageList
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.usageList; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.usageList = value; }
		}
		#endregion
		
		public ServerStats()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
		}

		public ServerStats(string nodeName, long nodeAmountOfObjects, long nodeSize, Dictionary<string, long> nodeUsageList)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.name = nodeName;
			this.amountOfObjects = nodeAmountOfObjects;
			this.cacheSize = nodeSize;
			this.usageList = nodeUsageList;
		}
	}

	/// <summary>
	/// Contains statistical information
	/// </summary>
	[Serializable]
	public class IndexusStatistic
	{
		#region API
		/// <summary>Count the amount of success objects from cache</summary>
		internal long apiHitSuccess = 0;
		/// <summary>Count the amount of failed objects from cache</summary>
		internal long apiHitFailed = 0;
		/// <summary>Counts the amount of total added objects per instance</summary>
		internal long apiCounterAdd = 0;
		/// <summary>Counts the amount of total received objects per instance</summary>
		internal long apiCounterGet = 0;
		/// <summary>Counts the amount of total removed objects per instance</summary>
		internal long apiCounterRemove = 0;
		/// <summary>Counts the amount of statistics calls per instance</summary>
		internal long apiCounterStatistic = 0;
		/// <summary>amount of success calls</summary>
		internal long apiCounterSuccess = 0;
		/// <summary>amount of failed calls</summary>
		internal long apiCounterFailed = 0;
		/// <summary>amount of failed calls to a node which is not available</summary>
		internal long apiCounterFailedNodeNotAvailable = 0;

		/// <summary>
		/// Gets the API hit success.
		/// </summary>
		/// <value>The API hit success.</value>
		public long ApiHitSuccess
		{
			get { return this.apiHitSuccess; }
		}

		/// <summary>
		/// Gets the API hit failed.
		/// </summary>
		/// <value>The API hit failed.</value>
		public long ApiHitFailed
		{
			get { return this.apiHitFailed; }
		}
		/// <summary>
		/// Gets the API counter add.
		/// </summary>
		/// <value>The API counter add.</value>
		public long ApiCounterAdd
		{
			get { return this.apiCounterAdd; }
		}
		/// <summary>
		/// Gets the API counter get.
		/// </summary>
		/// <value>The API counter get.</value>
		public long ApiCounterGet
		{
			get { return this.apiCounterGet; }
		}
		/// <summary>
		/// Gets the API counter remove.
		/// </summary>
		/// <value>The API counter remove.</value>
		public long ApiCounterRemove
		{
			get { return this.apiCounterRemove; }
		}
		/// <summary>
		/// Gets the API counter statistic.
		/// </summary>
		/// <value>The API counter statistic.</value>
		public long ApiCounterStatistic
		{
			get { return this.apiCounterStatistic; }
		}
		/// <summary>
		/// Gets the API counter success.
		/// </summary>
		/// <value>The API counter success.</value>
		public long ApiCounterSuccess
		{
			get { return this.apiCounterSuccess; }
		}
		/// <summary>
		/// Gets the API counter failed.
		/// </summary>
		/// <value>The API counter failed.</value>
		public long ApiCounterFailed
		{
			get { return this.apiCounterFailed; }
		}

		public long ApiCounterFailedNodeNotAvailable
		{
			get { return this.apiCounterFailedNodeNotAvailable; }
		}

		#endregion API

			/// <summary>
			/// Contains server information fore specific node
			/// </summary>
		public List<ServerStats> NodeDate = new List<ServerStats>();

		#region Services
		/// <summary>the amount of objects the service contains.</summary>
		private long serviceAmountOfObjects = 0;
		/// <summary>current total RAM which is needed.</summary>
		private long serviceTotalSize = 0;
		/// <summary>enables possibility to receive the usage of cache items</summary>
		private Dictionary<string, long> serviceUsageList = null;
		#endregion Services

		/// <summary>
		/// Initializes a new instance of the <see cref="T:IndexusStatistic"/> class.
		/// </summary>
		public IndexusStatistic()
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
		/// Initializes a new instance of the <see cref="T:IndexusStatistic"/> class.
		/// </summary>
		/// <param name="add">The add. A <see cref="T:System.Int64"/> Object.</param>
		/// <param name="get">The get. A <see cref="T:System.Int64"/> Object.</param>
		/// <param name="remove">The remove. A <see cref="T:System.Int64"/> Object.</param>
		/// <param name="stat">The stat. A <see cref="T:System.Int64"/> Object.</param>
		/// <param name="failed">The failed.</param>
		/// <param name="success">The success.</param>
		/// <param name="amount">The amount. A <see cref="T:System.Int64"/> Object.</param>
		/// <param name="srvSize">Size of the SRV.</param>
		public IndexusStatistic(long add, long get, long remove, long stat, long failed, long success, long amount, long srvSize)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			this.apiCounterAdd = add;
			this.apiCounterGet = get;
			this.apiCounterRemove = remove;
			this.apiCounterStatistic = stat;
			this.apiHitFailed = failed;
			this.apiHitSuccess = success;
			if (amount > 0)
				this.serviceAmountOfObjects = amount;
			if (srvSize > 0)
				this.serviceTotalSize = srvSize;
			if (serviceUsageList == null)
				serviceUsageList = new Dictionary<string, long>();
		}

		public long GetHitRatio
		{
			get {
				// prevent DivideByZeroException 
				if ((apiHitFailed + apiHitSuccess) == 0)
				{
					return 0;
				}

				return (apiHitSuccess * 100 / (apiHitFailed + apiHitSuccess)); 
			}
		}


		/// <summary>
		/// Gets or sets the service amount of objects.
		/// </summary>
		/// <value>The service amount of objects.</value>
		public long ServiceAmountOfObjects
		{
			[System.Diagnostics.DebuggerStepThrough]
			set { this.serviceAmountOfObjects = value; }
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.serviceAmountOfObjects; }
		}

		/// <summary>
		/// Gets or sets the total size of the service.
		/// </summary>
		/// <value>The total size of the service.</value>
		public long ServiceTotalSize
		{
			[System.Diagnostics.DebuggerStepThrough]
			set { this.serviceTotalSize = value; }
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.serviceTotalSize; }
		}

		#region Property: serviceUsageList
		/// <summary>
		/// Gets/sets the ServiceUsageList
		/// </summary>
		public Dictionary<string, long> ServiceUsageList
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.serviceUsageList; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.serviceUsageList = value; }
		}
		#endregion


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

			if (Handler.NetworkMessage.countTransferDataToServer == 0)
			{
				return @"Run first some client / server interactions before you call the statistics [Send Data To Servers is 0]";
			}
			if (Handler.NetworkMessage.countTransferDataFromServer == 0)
			{
				return @"Run first some client / server interactions before you call the statistics [Receive Data From Servers is 0]";
			}
			
			string serviceUsageListOutput = string.Empty + Environment.NewLine;

			if (this.serviceUsageList != null && serviceUsageList.Count > 0)
			{
				int cntr = 0;
				foreach (KeyValuePair<string, long> kvp in this.serviceUsageList)
				{
					serviceUsageListOutput +=
						string.Format(@"   Key: {0} - Hits: {1}",
								kvp.Key,
								kvp.Value
							)
						+ Environment.NewLine;

					++cntr;
					if (cntr >= 20)
					{
						break;
					}
				}
			}
			// calculate hit ratio
			long hitratio = 0;
			if (apiHitFailed == 0)
			{
				hitratio = 100;
			}
			else
			{
				hitratio = apiHitSuccess * 100 / (apiHitFailed + apiHitSuccess);
			}

			string msg = string.Format(
				Environment.NewLine + Environment.NewLine +
				"* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * " +
				Environment.NewLine +
				"Statistic for Shared Cache API and Service;" + Environment.NewLine +
				"API Data: " + Environment.NewLine +
				" {0,7} objects have been added." + Environment.NewLine +
				" {1,7} objects have been received." + Environment.NewLine +
				" {2,7} objects have been removed." + Environment.NewLine +
				" {3,7} statistics have been called." + Environment.NewLine +
				" {4,7} tried to call node which is not avialable." + Environment.NewLine +
				"Shared Service Data: " + Environment.NewLine +
				" {5,7} items cache contains currently." + Environment.NewLine +
				" {6,7}kb contains the cache currently." + Environment.NewLine +
				" Hit Stats for key's (limited list to top 20 keys - if empty then are no key's available in cache!)" + Environment.NewLine +
				"   {7}" + Environment.NewLine +
				"Network Data: " + Environment.NewLine +
				" {8,7} MB have been transferred to server." + Environment.NewLine +
				" {9,7} MB have been received from server." + Environment.NewLine +
				" {10,7} transfers succeeded." + Environment.NewLine +
				" {11,7} transfers failed." + Environment.NewLine +
				" {12,7}% are successfully." + Environment.NewLine + Environment.NewLine +
				"Overall Hitrate: " + Environment.NewLine +
				" {13,7} are successfully." + Environment.NewLine + 
				" {14,7} are failed." + Environment.NewLine + 
				" {15,7}% Hit Ratio" + Environment.NewLine + 
				"* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * " +
				Environment.NewLine + "*       Stat Output date: {16:f}                    *" + Environment.NewLine +
				"* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * " +
				Environment.NewLine,
				/*0*/apiCounterAdd,
				/*1*/apiCounterGet,
				/*2*/apiCounterRemove,
				/*3*/apiCounterStatistic,
				/*4*/apiCounterFailedNodeNotAvailable,
				/*5*/serviceAmountOfObjects,
				/*6*/serviceTotalSize > 0 ? serviceTotalSize / 1024 : 0,
				/*7*/serviceUsageListOutput,
				/*8*/Handler.NetworkMessage.countTransferDataToServer > 0 ? Handler.NetworkMessage.countTransferDataToServer / (1024 * 1024) : 0,
				/*9*/Handler.NetworkMessage.countTransferDataFromServer > 0 ? Handler.NetworkMessage.countTransferDataFromServer / (1024 * 1024) : 0,
				/*10*/apiCounterSuccess,
				/*11*/apiCounterFailed,
				/*12*/apiCounterSuccess > 0 ? apiCounterSuccess * 100 / (apiCounterFailed + apiCounterSuccess) : -1,
				/*13*/apiHitSuccess,
				/*14*/apiHitFailed,
				/*15*/hitratio,
				/*16*/DateTime.Now
				);


			return msg;
		}


		/// <summary>
		/// Same like ToString, used within Notify Application
		/// </summary>
		/// <returns>A formatted <see cref="string"/> object.</returns>
		public string ToNotify()
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			string result = string.Empty;

			result = string.Format(
				" - {0} items cache contains currently." + Environment.NewLine +
				" - {1} kb contains the cache currently." + Environment.NewLine,
				serviceAmountOfObjects,
				serviceTotalSize / 1024);

			return result;

		}


	}
}
