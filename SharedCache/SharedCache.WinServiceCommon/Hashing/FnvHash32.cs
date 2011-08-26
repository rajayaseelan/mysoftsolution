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
// Modified:  16-02-2008 SharedCache.com, rschuetz : more information about this hashing algorithm: http://www.isthe.com/chongo/tech/comp/fnv/
// Modified:  16-02-2008 SharedCache.com, rschuetz : more about hashing is available at: http://burtleburtle.net/bob/hash/index.html
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
	/// Enables a charsequence with the capability to produce a Fowler-Noll-Vo (FNV) 32-bit hashcode
	/// FNV hashes are desigend to be fast while maintaining a low collision rate. The FNV speed
	/// allows one to quickly hash lots of data while maintaining a reasonable collision rate. 
	/// The high dispersion of the FNV hashes makes them well suited for hashing nearly identical string such
	/// as URL's hostnames, filesnames, text, IP Addresses and others.
	/// </summary>
	public class FnvHash32 : HashAlgorithm
	{
		/// <summary>
		/// represent a initial value
		/// </summary>
		private const ulong FnvInitializeValue = 0x1000193;
		/// <summary>
		/// prinzipal value
		/// </summary>
		private const ulong FnvPrincipalValue = 0x811C9DC5;
		/// <summary>
		/// represent the actual value
		/// </summary>
		private ulong hashValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="FnvHash64"/> class.
		/// </summary>
		public FnvHash32()
			: base()
		{
			base.HashSizeValue = 32;
			this.Initialize();
		}

		/// <summary>
		/// When overridden in a derived class, routes data written to the object into the hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The input to compute the hash code for.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			int max = ibStart + cbSize;
			for (int i = ibStart; i < max; i++)
			{
				this.hashValue = (this.hashValue * FnvPrincipalValue) ^ array[i];
			}
		}

		/// <summary>
		/// When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>The computed hash code.</returns>
		protected override byte[] HashFinal()
		{
			return BitConverter.GetBytes(this.hashValue);
		}

		/// <summary>
		/// Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm"></see> class.
		/// </summary>
		public override void Initialize()
		{
			this.hashValue = FnvInitializeValue;
		}
	}
}
