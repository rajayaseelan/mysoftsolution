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
// Name:      IndexusProviderSection.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  30-09-2007 SharedCache.com, rschuetz : added comments
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// Modified:  28-02-2008 SharedCache.com, rschuetz : added replicatedServers section to configuration
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;
using System.Configuration;

namespace SharedCache.WinServiceCommon.Configuration.Client
{
	/// <summary>
	/// Defines the config section in config file (web.config / app.config)
	/// </summary>
	public class IndexusProviderSection : ConfigurationSection
	{
		/// <summary>
		/// Gets the providers.
		/// </summary>
		/// <value>The providers.</value>
		[ConfigurationProperty("providers", IsRequired=true)]		
		public ProviderSettingsCollection Providers
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ProviderSettingsCollection)base["providers"]; }
		}

		/// <summary>
		/// Gets the servers.
		/// </summary>
		/// <value>The servers.</value>
		[ConfigurationProperty("servers", IsRequired = true)]
		public IndexusServerSettingCollection Servers
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (IndexusServerSettingCollection)this["servers"]; }
		}


		/// <summary>
		/// Gets the replicated servers.
		/// </summary>
		/// <value>The replicated servers.</value>
		[ConfigurationProperty("replicatedServers", IsRequired = false)]
		public IndexusServerSettingCollection ReplicatedServers
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (IndexusServerSettingCollection)this["replicatedServers"]; }
		}

		/// <summary>
		/// Gets the client settings.
		/// </summary>
		/// <value>The client settings.</value>
		[ConfigurationProperty("clientSetting")]
		public ClientSettingElement ClientSetting
		{
			// [System.Diagnostics.DebuggerStepThrough]
			get { return (ClientSettingElement)this["clientSetting"]; }
		}

		/// <summary>
		/// Gets or sets the default provider, min. 1 server has to be defined.
		/// </summary>
		/// <value>The default provider.</value>
		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("defaultProvider", DefaultValue = "IndexusSharedCacheProvider", IsRequired = true)]
		public string DefaultProvider
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (string)base["defaultProvider"]; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base["defaultProvider"] = value; }
		}
	}
}
