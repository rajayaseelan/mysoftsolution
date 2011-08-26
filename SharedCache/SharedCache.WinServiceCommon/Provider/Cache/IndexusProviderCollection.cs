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
// Name:      IndexusProviderCollection.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  30-09-2007 SharedCache.com, rschuetz : added comments
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text;

namespace SharedCache.WinServiceCommon.Provider.Cache
{
	/// <summary>
	/// Represents a collection of provider objects that inherit from System.Configuration.Provider.ProviderCollection
	/// </summary>
	public class IndexusProviderCollection : ProviderCollection
	{
		/// <summary>
		/// Gets the <see cref="SharedCache.WinServiceCommon.Provider.Cache.IndexusProviderBase"/> with the specified name.
		/// </summary>
		/// <value></value>
		public new IndexusProviderBase this[string name]
		{
			get
			{
				return (IndexusProviderBase)base[name];
			}
		}

		/// <summary>
		/// Adds a provider to the base provider collection and makes it available.
		/// </summary>
		/// <param name="provider">The provider to be added.</param>
		/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Configuration.Provider.ProviderBase.Name"></see> of provider is null.- or -The length of the <see cref="P:System.Configuration.Provider.ProviderBase.Name"></see> of provider is less than 1.</exception>
		/// <exception cref="T:System.ArgumentNullException">provider is null.</exception>
		/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
		/// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
		public override void Add(ProviderBase provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("IndexusProviderBase");
			}

			if (!(provider is IndexusProviderBase))
			{
				throw new ArgumentException("Invalid provider type", "IndexusProviderBase");
			}
			base.Add(provider);
		}
	}
}
