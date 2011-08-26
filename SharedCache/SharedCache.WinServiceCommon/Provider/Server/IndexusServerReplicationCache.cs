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
// Name:      IndexusServerReplicationCache.cs
// 
// Created:   31-12-2007 SharedCache.com, rschuetz
// Modified:  31-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : same as the client but element names are differnt
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 


using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Text;
using System.Web.Configuration;

using SharedCache.WinServiceCommon.Configuration.Server;

namespace SharedCache.WinServiceCommon.Provider.Server
{
	/// <summary>
	/// Loading defined providers in config files (web.config / app.config)
	/// </summary>
	public static class IndexusServerReplicationCache
	{
		#region Constructor
		/// <summary>
		/// Initializes the <see cref="IndexusServerReplicationCache"/> class.
		/// </summary>
		static IndexusServerReplicationCache()
		{
			LoadProvider();
		}
		#endregion Constructor

		#region Properties and Variables

		#region Variables
		/// <summary>
		/// prevents concurrent usage of data
		/// </summary>
		private static object bulkObject = new object();

		/// <summary>
		/// a static <see cref="IndexusServerProviderBase"/> to load configured data.
		/// </summary>
		private static IndexusServerProviderBase providerBase = null;
		/// <summary>
		/// a static <see cref="IndexusServerProviderCollection"/> to load configured data.
		/// </summary>
		private static IndexusServerProviderCollection providerCollection = null;
		/// <summary>
		/// a static <see cref="IndexusServerProviderSection"/> to load configured data.
		/// </summary>
		private static IndexusServerProviderSection providerSection;
		#endregion Variables

		#region Getter for Properties
		/// <summary>
		/// Gets the current provider.
		/// </summary>
		/// <value>The current provider.</value>
		public static IndexusServerProviderBase CurrentProvider
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return providerBase;
			}
		}

		/// <summary>
		/// Gets the provider section.
		/// </summary>
		/// <value>The provider section.</value>
		public static IndexusServerProviderSection ProviderSection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return providerSection;
			}
		}
		#endregion Getter for Properties
		#endregion Properties and Variables

		/// <summary>
		/// Loads the provider which is configured in applicaiton config file (app.config / web.config)
		/// <example>
		/// <![CDATA[<?xml version="1.0" encoding="utf-8" ?>]]>
		///	<configSections>
		///	<section name="indexusNetSharedCache" type="SharedCache.WinServiceCommon.Configuration.Client.IndexusProviderSection, SharedCache.WinServiceCommon" />
		///	</configSections>
		/// 
		/// <indexusNetSharedCache defaultProvider="IndexusSharedCacheProvider">
		///		<servers>
		///		   <add key="Server1"	ipaddress="10.0.0.3" port="48888" />
		///		   <add key="Server2"	ipaddress="10.0.0.4" port="48888" />
		///		</servers>
		///		<providers>
		///		   <add
		///		     name="IndexusSharedCacheProvider"
		///		     type="SharedCache.WinServiceCommon.Provider.Cache.IndexusSharedCacheProvider, SharedCache.WinServiceCommon" />
		///		 </providers>
		///	</indexusNetSharedCache>
		/// </example>
		/// </summary>
		private static void LoadProvider()
		{
			if (providerBase == null)
			{
				lock (bulkObject)
				{
					if (providerBase == null)
					{
						// get a reference to the <cacheProvider> section
						providerSection = (IndexusServerProviderSection)WebConfigurationManager.GetSection("replicatedSharedCache");

						// load registered provider and point provider base to the default provider
						providerCollection = new IndexusServerProviderCollection();
						ProvidersHelper.InstantiateProviders(
								providerSection.Providers,
								providerCollection,
								typeof(IndexusServerProviderBase)
							);

						providerBase = providerCollection[providerSection.DefaultProvider];

						if (providerBase == null)
						{
							throw new ProviderException(@"Unable to load default replication shared Cache Provider!");
						}
					}
				}
			}
		}
	}
}
