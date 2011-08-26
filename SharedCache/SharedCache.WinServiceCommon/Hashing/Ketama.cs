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
// Modified:  16-02-2008 SharedCache.com, rschuetz : implementation done based on http://www.last.fm/user/RJ/journal/2007/04/10/392555/
// Modified:  16-02-2008 SharedCache.com, rschuetz : more info can be also found here: http://www8.org/w8-papers/2a-webserver/caching/paper2.html
// Modified:  30-08-2008 SharedCache.com, rschuetz : uncommented hashing algorithm
// *************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SharedCache.WinServiceCommon.Hashing
{
	/// <summary>
	/// MD5 Based consistent hashing
	/// </summary>
	public class Ketama
	{
		/// <summary>
		/// Lock code operation
		/// </summary>
		private static object bulkObject = new object();
		/// <summary>
		/// create MD5 Hash
		/// </summary>
		private static MD5 hash = MD5.Create();

		/// <summary>
		/// Gets the hash for given key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static int Generate(string key)
		{
			byte[] arr = null;
			lock (bulkObject)
			{
				arr = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
			}
			int result = ((int)(arr[3] & 0xFF) << 24) | ((int)(arr[2] & 0xFF) << 16) | ((int)(arr[1] & 0xFF) << 8) | (int)(arr[0] & 0xFF);
			return result;
		}

	}
}
