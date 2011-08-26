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
// Name:      Compression.cs
// 
// Created:   07-01-2006 SharedCache.com, rschuetz
// Modified:  07-01-2006 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

using SharedCache.WinServiceCommon.SharpZipLib.Zip.Compression.Streams;

namespace SharedCache.WinServiceCommon.Formatters
{
	/// <summary>
	/// Makes usage of ICSharpCode.SharepZipLib which is origanlly written 
	/// by Mike Krueger. Expose functionlity to compress array's of <see cref="byte"/>
	/// and uncompress them. There is also a funktionlity available to validate 
	/// if the header of the stream indicates it as compressed or not. In case 
	/// its not compressed the client should not call the Decompress() Method.
	/// </summary>
	public class Compression
	{
		/// <summary>
		/// Compresses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>a compressed array of <see cref="byte"/></returns>
		public static byte[] Compress(byte[] value)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Compression).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (value == null)
				return value;

			try
			{
				MemoryStream ms = new MemoryStream();
				Stream s = new DeflaterOutputStream(ms);
				s.Write(value, 0, value.Length);
				s.Close();
				byte[] compressedData = (byte[])ms.ToArray();

				
				{
					Handler.LogHandler.Info(string.Format("Original: {0, 10}; Compressed: {1, 10}", value.Length, compressedData.Length));
				}

				return compressedData;
			}
			catch (Exception ex)
			{
				Handler.LogHandler.Force(@"A problem appears to compress payload of message: " + ex.Message);
				return value;
			}
		}

		/// <summary>
		/// Check the stream header which is returend from the server.
		/// </summary>
		/// <param name="value">The value as an array of <see cref="byte"/></param>
		/// <returns>wheater is input is compressed [true] or its not compressed [false]</returns>
		public static bool CheckHeader(byte[] value)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Compression).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			MemoryStream ms = new MemoryStream(value);

			StreamManipulator input = new StreamManipulator();
			input.SetInput(value, 0, value.Length);

			return DecodeHeader(input);
		}

		/// <summary>
		/// Decodes a zlib/RFC1950 header - to evaluate input if
		/// decompression is needed or not.
		/// </summary>
		/// <param name="input">The input as an object of type <see cref="StreamManipulator"/></param>
		/// <returns>wheater is input is compressed [true] or its not compressed [false]</returns>
		private static bool DecodeHeader(StreamManipulator input)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Compression).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			int header = input.PeekBits(16);
			if (header < 0)
			{
				return false;
			}
			input.DropBits(16);

			/* header is written in "wrong" byte order */
			header = ((header << 8) | (header >> 8)) & 0xffff;
			if (header % 31 != 0)
			{
				// not compressed
				return false;
			}
			// its compressed
			return true;
		}

		/// <summary>
		/// Decompresses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>a decompressed array of <see cref="byte"/></returns>
		public static byte[] Decompress(byte[] value)
		{
			#region Access Log
#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Compression).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			try
			{
				MemoryStream ms = new MemoryStream(value);

				Stream stream = new SharedCache.WinServiceCommon.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);
				MemoryStream result = new MemoryStream();
				byte[] buff = new byte[4096];
				int read = stream.Read(buff, 0, buff.Length);
				while (read > 0)
				{
					result.Write(buff, 0, read);
					read = stream.Read(buff, 0, buff.Length);
				}
				return result.ToArray();
			}
			catch (Exception ex)
			{
				Handler.LogHandler.Force(@"A problem appears to decompress payload of message: " + ex.Message);
				return null;
			}
		}
	}

	/// <summary>
	/// This class allows us to retrieve a specified number of bits from
	/// the input buffer, as well as copy big byte blocks.
	///
	/// It uses an int buffer to store up to 31 bits for direct
	/// manipulation.  This guarantees that we can get at least 16 bits,
	/// but we only need at most 15, so this is all safe.
	///
	/// There are some optimizations in this class, for example, you must
	/// never peek more than 8 bits more than needed, and you must first
	/// peek bits before you may drop them.  This is not a general purpose
	/// class but optimized for the behaviour of the Inflater.
	///
	/// authors of the original java version : John Leuner, Jochen Hoenicke
	/// <remarks>
	/// has been copied from: http://www.krugle.org/examples/p-zUID3qJxRebp1MBw/StreamManipulator.cs and http://www.icsharpcode.net/CodeReader/SharpZipLib/031/ZipCompressionStreamsStreamManipulator.cs.html
	/// Copyright (C) 2001 Mike Krueger
	/// </remarks>
	/// </summary>
	public class StreamManipulator
	{
		/// <summary>
		/// an array of <see cref="byte"/>
		/// </summary>
		private byte[] window;
		/// <summary>
		/// <see cref="int"/> where the window starts
		/// </summary>
		private int window_start = 0;
		/// <summary>
		/// <see cref="int"/> where the window ends
		/// </summary>
		private int window_end = 0;
		/// <summary>
		/// <see cref="uint"/> which indicat the buffer size
		/// </summary>
		private uint buffer = 0;
		/// <summary>
		/// the amount of bits within the buffer as <see cref="int"/> amount
		/// </summary>
		private int bits_in_buffer = 0;

		/// <summary> 
		/// Get the next n bits but don't increase input pointer.  n must be 
		/// less or equal 16 and if you if this call succeeds, you must drop 
		/// at least n-8 bits in the next call. 
		/// </summary> 
		/// <returns> 
		/// the value of the bits, or -1 if not enough bits available.  */ 
		/// </returns> 
		public int PeekBits(int n)
		{
			if (bits_in_buffer < n)
			{
				if (window_start == window_end)
				{
					return -1;
				}
				buffer |= (uint)((window[window_start++] & 0xff |
												 (window[window_start++] & 0xff) << 8) << bits_in_buffer);
				bits_in_buffer += 16;
			}
			return (int)(buffer & ((1 << n) - 1));
		}

		/// <summary> 
		/// Drops the next n bits from the input.  You should have called peekBits 
		/// with a bigger or equal n before, to make sure that enough bits are in 
		/// the bit buffer. 
		/// </summary> 
		public void DropBits(int n)
		{
			buffer >>= n;
			bits_in_buffer -= n;
		}

		/// <summary> 
		/// Gets the next n bits and increases input pointer.  This is equivalent 
		/// to peekBits followed by dropBits, except for correct error handling. 
		/// </summary> 
		/// <returns> 
		/// the value of the bits, or -1 if not enough bits available. 
		/// </returns> 
		public int GetBits(int n)
		{
			int bits = PeekBits(n);
			if (bits >= 0)
			{
				DropBits(n);
			}
			return bits;
		}

		/// <summary> 
		/// Gets the number of bits available in the bit buffer.  This must be 
		/// only called when a previous peekBits() returned -1. 
		/// </summary> 
		/// <returns> 
		/// the number of bits available. 
		/// </returns> 
		public int AvailableBits
		{
			get
			{
				return bits_in_buffer;
			}
		}

		/// <summary> 
		/// Gets the number of bytes available. 
		/// </summary> 
		/// <returns> 
		/// the number of bytes available. 
		/// </returns> 
		public int AvailableBytes
		{
			get
			{
				return window_end - window_start + (bits_in_buffer >> 3);
			}
		}

		/// <summary> 
		/// Skips to the next byte boundary. 
		/// </summary> 
		public void SkipToByteBoundary()
		{
			buffer >>= (bits_in_buffer & 7);
			bits_in_buffer &= ~7;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is needing input.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is needing input; otherwise, <c>false</c>.
		/// </value>
		public bool IsNeedingInput
		{
			get
			{
				return window_start == window_end;
			}
		}

		/// <summary> 
		/// Copies length bytes from input buffer to output buffer starting 
		/// at output[offset].  You have to make sure, that the buffer is 
		/// byte aligned.  If not enough bytes are available, copies fewer 
		/// bytes. 
		/// </summary> 
		/// <param name="output"> 
		/// the buffer. 
		/// </param> 
		/// <param name="offset"> 
		/// the offset in the buffer. 
		/// </param> 
		/// <param name="length"> 
		/// the length to copy, 0 is allowed. 
		/// </param> 
		/// <returns> 
		/// the number of bytes copied, 0 if no byte is available. 
		/// </returns> 
		public int CopyBytes(byte[] output, int offset, int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length negative");
			}
			if ((bits_in_buffer & 7) != 0)
			{
				/* bits_in_buffer may only be 0 or 8 */
				throw new InvalidOperationException("Bit buffer is not aligned!");
			}

			int count = 0;
			while (bits_in_buffer > 0 && length > 0)
			{
				output[offset++] = (byte)buffer;
				buffer >>= 8;
				bits_in_buffer -= 8;
				length--;
				count++;
			}
			if (length == 0)
			{
				return count;
			}

			int avail = window_end - window_start;
			if (length > avail)
			{
				length = avail;
			}
			System.Array.Copy(window, window_start, output, offset, length);
			window_start += length;

			if (((window_start - window_end) & 1) != 0)
			{
				/* We always want an even number of bytes in input, see peekBits */
				buffer = (uint)(window[window_start++] & 0xff);
				bits_in_buffer = 8;
			}
			return count + length;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StreamManipulator"/> class.
		/// </summary>
		public StreamManipulator()
		{
		}

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
		{
			buffer = (uint)(window_start = window_end = bits_in_buffer = 0);
		}

		/// <summary>
		/// Sets the input.
		/// </summary>
		/// <param name="buf">The buf.</param>
		/// <param name="off">The off.</param>
		/// <param name="len">The len.</param>
		public void SetInput(byte[] buf, int off, int len)
		{
			if (window_start < window_end)
			{
				throw new InvalidOperationException("Old input was not completely processed");
			}

			int end = off + len;

			/* We want to throw an ArrayIndexOutOfBoundsException early.  The 
			* check is very tricky: it also handles integer wrap around. 
			*/
			if (0 > off || off > end || end > buf.Length)
			{
				throw new ArgumentOutOfRangeException();
			}

			if ((len & 1) != 0)
			{
				/* We always want an even number of bytes in input, see peekBits */
				buffer |= (uint)((buf[off++] & 0xff) << bits_in_buffer);
				bits_in_buffer += 8;
			}

			window = buf;
			window_start = off;
			window_end = end;
		}
	}
}
