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
// Name:      IInit.cs
// 
// Created:   18-07-2007 SharedCache.com, rschuetz
// Modified:  18-07-2007 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCache.WinServiceCommon.Extenders
{
	/// <summary>
	/// Defines an interface for Services which has to 
	/// be started upon Win32 Service start.
	/// </summary>
	public interface IInit
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
		/// Inits this instance.
		/// </summary>
		void Init();

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		void Dispose();
	}
}
