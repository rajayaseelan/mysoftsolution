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
// Name:      SharedCacheStateObject.cs
// 
// Created:   10-02-2008 SharedCache.com, rschuetz
// Modified:  10-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : instead of using a list<byte> we enlarge a byte array
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharedCache.WinServiceCommon.Sockets
{
	/// <summary>
	/// Represents the State between each async call 
	/// and connection to the client socket.
	/// </summary>
	public class SharedCacheStateObject : IDisposable
	{
		#region Constant Variable: BufferSize
		/// <summary>
		/// Defines the default buffer size
		/// </summary>
		public const int BufferSize = 65535; // 1024000; //
		#endregion	

		#region Property: AlreadySent
		private long alreadySent;

		/// <summary>
		/// Gets/sets the Already Sent byte amount
		/// </summary>
		public long AlreadySent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.alreadySent; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.alreadySent = value; }
		}
		#endregion
		#region Property: AlreadyRead
		private long alreadyRead;
		
		/// <summary>
		/// Gets/sets the AlreadyRead
		/// </summary>
		public long AlreadyRead
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.alreadyRead;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.alreadyRead = value;  }
		}
		#endregion
		#region Property: ReadHeader
		private bool readHeader = true;

		/// <summary>
		/// Gets/sets the if the logic needs to read the Header
		/// </summary>
		public bool ReadHeader
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.readHeader; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.readHeader = value; }
		}
		#endregion
		#region Property: MessageLength
		private long messageLength;

		/// <summary>
		/// Gets/sets the Message Length which needs to receive or send.
		/// </summary>
		public long MessageLength
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.messageLength; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.messageLength = value; }
		}
		#endregion
		#region Property: DataBigBuffer
		private byte[] dataBigBuffer;
		/// <summary>
		/// Gets or sets the data big buffer.
		/// </summary>
		/// <value>The data big buffer.</value>
		public byte[] DataBigBuffer
		{
			get{ return this.dataBigBuffer; }
			set { this.dataBigBuffer = value; }
		}
		#endregion Property: DataBigBuffer
		#region Property: WorkSocket
		private Socket workSocket = null;

		/// <summary>
		/// Gets/sets the Worker Socket
		/// </summary>
		public Socket WorkSocket
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.workSocket; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.workSocket = value; }
		}
		#endregion
		#region Property: Buffer
		private byte[] buffer = new byte[BufferSize];

		/// <summary>
		/// Gets/sets the temporary Buffer
		/// </summary>
		public byte[] Buffer
		{			
			get { return this.buffer; }			
			set { this.buffer = value; }
		}
		#endregion
		#region Property: AliveTimeStamp
		private DateTime aliveTimeStamp;

		/// <summary>
		/// Gets/sets the AliveTimeStamp before the server free up server resources
		/// </summary>
		public DateTime AliveTimeStamp
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this.aliveTimeStamp; }

			[System.Diagnostics.DebuggerStepThrough]
			set { this.aliveTimeStamp = value; }
		}
		#endregion
		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.dataBigBuffer = null;
			this.buffer = null;
		}

		#endregion
	}
}
