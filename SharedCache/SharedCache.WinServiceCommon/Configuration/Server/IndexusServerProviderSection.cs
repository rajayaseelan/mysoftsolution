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
// Name:      IndexusServerProviderSection.cs
// 
// Created:   31-12-2007 SharedCache.com, rschuetz
// Modified:  31-12-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : same as the client but element names are differnt
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 


using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;
using System.Configuration;

namespace SharedCache.WinServiceCommon.Configuration.Server
{
	/// <summary>
	/// Defines the config section in config file (web.config / app.config)
	/// </summary>
	public class IndexusServerProviderSection : ConfigurationSection
	{
		/// <summary>
		/// Gets the providers.
		/// </summary>
		/// <value>The providers.</value>
		[ConfigurationProperty("providers", IsRequired = true)]
		public ProviderSettingsCollection Providers
		{
			get { return (ProviderSettingsCollection)base["providers"]; }
		}

		/// <summary>
		/// Gets the servers.
		/// </summary>
		/// <value>The servers.</value>
		[ConfigurationProperty("replicatedServers", IsRequired = true)]
		public IndexusServerSettingCollection Servers
		{
			get { return (IndexusServerSettingCollection)this["replicatedServers"]; }
		}

		[ConfigurationProperty("serverSetting",IsRequired=true)]
		public IndexusServerSettingElement ServerSetting
		{
			get { return (IndexusServerSettingElement)this["serverSetting"]; }
		}

		/// <summary>
		/// Gets or sets the default provider, min. 1 server has to be defined.
		/// </summary>
		/// <value>The default provider.</value>
		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("defaultProvider", DefaultValue = "ServerSharedCacheProvider")]
		public string DefaultProvider
		{
			get { return (string)base["defaultProvider"]; }
			set { base["defaultProvider"] = value; }
		}
	}
}
