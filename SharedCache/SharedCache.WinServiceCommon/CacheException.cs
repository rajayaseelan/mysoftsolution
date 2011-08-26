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
// Name:      CacheException.cs
// 
// Created:   24-02-2008 SharedCache.com, rschuetz
// Modified:  24-02-2008 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Reflection;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// Cache Excpetion returned to the client in case the server had a problem.
	/// The original exception is transferred as innerException
	/// </summary>
	[Serializable]
	public class CacheException
	{
		/// <summary>
		/// the used action
		/// </summary>
		public IndexusMessage.ActionValue action;
		/// <summary>
		/// the actual status
		/// </summary>
		public IndexusMessage.StatusValue status;		
		#region Property: Info
		private string info;
		
		/// <summary>
		/// Gets/sets the Info
		/// </summary>
		public string Info
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.info;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.info = value;  }
		}
		#endregion
		#region Property: Title
		private string title;
		
		/// <summary>
		/// Gets/sets the Title
		/// </summary>
		public string Title
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.title;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.title = value;  }
		}
		#endregion
		#region Property: InnerExceptionMessage
		private string innerExceptionMessage;
		
		/// <summary>
		/// Gets/sets the InnerExceptionMessage
		/// </summary>
		public string InnerExceptionMessage
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.innerExceptionMessage;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.innerExceptionMessage = value;  }
		}
		#endregion
		#region Property: Source
		private string source;
		
		/// <summary>
		/// Gets/sets the Source
		/// </summary>
		public string Source
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.source;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.source = value;  }
		}
		#endregion
		#region Property: StackTrace
		private string stackTrace;
		
		/// <summary>
		/// Gets/sets the StackTrace
		/// </summary>
		public string StackTrace
		{
			[System.Diagnostics.DebuggerStepThrough]
			get  { return this.stackTrace;  }
			
			[System.Diagnostics.DebuggerStepThrough]
			set { this.stackTrace = value;  }
		}
		#endregion

		public CacheException() 
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheException"/> class.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="action">The action.</param>
		/// <param name="status">The status.</param>
		/// <param name="innerException">The inner exception.</param>
		public CacheException(string title, IndexusMessage.ActionValue action, IndexusMessage.StatusValue status, Exception innerException)
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
			#endif
			#endregion Access Log

			this.action = action;
			this.status = status;
			this.title = title;
			this.innerExceptionMessage = innerException.Message;
			this.source = innerException.Source;
			this.stackTrace = innerException.StackTrace;
		}
	}
}
