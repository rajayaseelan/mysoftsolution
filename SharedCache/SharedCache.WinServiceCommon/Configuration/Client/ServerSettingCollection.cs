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
// Name:      IndexusServerSettingCollection.cs
// 
// Created:   30-09-2007 Netcraft, rdayan: 
// Modified:  30-09-2007 Netcraft, rdayan: Creation
// Modified:  30-09-2007 SharedCache.com, rschuetz : added comments
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;
using System.Configuration;

namespace SharedCache.WinServiceCommon.Configuration.Client
{
	/// <summary>
	/// Defines the config server setting collection within config file (web.config / app.config)
	/// </summary>
	public class IndexusServerSettingCollection : System.Configuration.ConfigurationElementCollection
	{
		/// <summary>
		/// Gets or sets the <see cref="SharedCache.WinServiceCommon.Configuration.Client.IndexusServerSetting"/> at the specified index.
		/// </summary>
		/// <value></value>
		public IndexusServerSetting this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.BaseGet(index) as IndexusServerSetting;
			}
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				if (base.BaseGet(index) != null)
				{
					base.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"></see>.
		/// </summary>
		/// <returns>
		/// A new <see cref="T:System.Configuration.ConfigurationElement"></see>.
		/// </returns>
		[System.Diagnostics.DebuggerStepThrough]
		protected override System.Configuration.ConfigurationElement CreateNewElement()
		{
			return new IndexusServerSetting();
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"></see> to return the key for.</param>
		/// <returns>
		/// An <see cref="T:System.Object"></see> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"></see>.
		/// </returns>
		[System.Diagnostics.DebuggerStepThrough]
		protected override object GetElementKey(System.Configuration.ConfigurationElement element)
		{
			return ((IndexusServerSetting)element).Key;
		}
	}
}
