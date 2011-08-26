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
// Name:      IndexusDistributionCache.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  30-09-2007 SharedCache.com, rschuetz : added comments and regions
// Modified:  30-09-2007 SharedCache.com, rschuetz : added [System.Diagnostics.DebuggerStepThrough] to properties
// Modified:  01-01-2008 SharedCache.com, rschuetz : changed name DefaultProvider to SharedCache
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Text;
using System.Web.Configuration;
using System.Reflection;

using SharedCache.WinServiceCommon.Configuration.Client;

namespace SharedCache.WinServiceCommon.Provider.Cache
{
	/// <summary>
	/// Loading defined providers in config files (web.config / app.config)
	/// </summary>
	public static class IndexusDistributionCache
	{
		#region Constructor
		/// <summary>
		/// Initializes the <see cref="IndexusDistributionCache"/> class.
		/// </summary>
		static IndexusDistributionCache()
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(IndexusDistributionCache).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

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
		/// a static <see cref="IndexusProviderBase"/> to load configured data.
		/// </summary>
		private static IndexusProviderBase providerBase = null;
		/// <summary>
		/// a static <see cref="IndexusProviderCollection"/> to load configured data.
		/// </summary>
		private static IndexusProviderCollection providerCollection = null;
		/// <summary>
		/// a static <see cref="IndexusProviderSection"/> to load configured data.
		/// </summary>
		private static IndexusProviderSection providerSection;
		#endregion Variables

		#region Getter for Properties
		/// <summary>
		/// Gets the current provider.
		/// </summary>
		/// <value>The current provider.</value>
		public static IndexusProviderBase SharedCache
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
		public static IndexusProviderSection ProviderSection
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
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(IndexusDistributionCache).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			if (providerBase == null)
			{
				lock (bulkObject)
				{
					if (providerBase == null)
					{
						// get a reference to the <cacheProvider> section
						providerSection = (IndexusProviderSection)WebConfigurationManager.GetSection("indexusNetSharedCache");

						// load registered provider and point provider base to the default provider
						providerCollection = new IndexusProviderCollection();
						ProvidersHelper.InstantiateProviders(
																providerSection.Providers,
								providerCollection,
								typeof(IndexusProviderBase)
							);

						providerBase = providerCollection[providerSection.DefaultProvider];

						if (providerBase == null)
						{
							throw new ProviderException(@"Unable to load default Shared Cache Provider!");
						}
					}
				}
			}
		}
	}
}
