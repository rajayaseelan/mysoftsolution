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
// Name:      IExpire.cs
// 
// Created:   22-12-2007 SharedCache.com, rschuetz
// Modified:  22-12-2007 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections.Generic;

namespace SharedCache.WinServiceCommon.Extenders
{
	/// <summary>
	/// Defines an interface for Cleanup Options which have to 
	/// be configured in app.config
	/// </summary>
	public interface IExpire
	{
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		string Name
		{
			get;
		}

		/// <summary>
		/// Dumps the cache item with specific pattern
		/// </summary>
		void DumpCacheItemAt(string key, object expire);

		/// <summary>
		/// Remove specified key.
		/// </summary>
		void Remove();

		/// <summary>
		/// Returns a list of <see cref="string"/> with key's to clean up from 
		/// the cache.
		/// </summary>
		/// <returns></returns>
		List<string> CleanUp();
	}
}
