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
// Name:      Listener.cs
// 
// Created:   21-01-2007 SharedCache.com, rschuetz
// Modified:  21-01-2007 SharedCache.com, rschuetz : Creation
// Modified:  15-07-2007 SharedCache.com, rschuetz : added Sync log
// Modified:  15-07-2007 SharedCache.com, rschuetz : added on some methods the check if in App.Config Logging is enabled
// Modified:  31-12-2007 SharedCache.com, rschuetz : applied on all Log methods the checked if LoggingEnable == 1
// Modified:  06-01-2008 SharedCache.com, rschuetz : removed checked for LoggingEnable on MemoryFatalException, while systems are using configuration option -1 on the key: CacheAmountOfObject they need to be informed in any case that the cache is full. 
// Modified:  06-01-2008 SharedCache.com, rschuetz : introduce to Force Method, this enables log writting in any case even if LoggingEnable = 0;
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using NLog;

namespace SharedCache.WinServiceCommon.Handler
{
	/// <summary>
	/// LogHandler uses to log information with NLog.
	/// </summary>
	public class LogHandler
	{
		private static string LogNameError = "General";
		private static string LogNameTraffic = "Traffic";
		private static string LogNameTracking = "Tracking";
		private static string LogNameSync = "Sync";
		private static string LogNameMemory = "Memory";

		private static Logger error = LogManager.GetLogger(LogNameError);
		private static Logger traffic = LogManager.GetLogger(LogNameTraffic);
		private static Logger tracking = LogManager.GetLogger(LogNameTracking);
		private static Logger sync = LogManager.GetLogger(LogNameSync);
		private static Logger memory = LogManager.GetLogger(LogNameMemory);

		#region Sync
		/// <summary>
		/// Logging Message and Exception around synchronizing data
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void SyncDebugException(string msg, Exception ex)
		{
			
			{
				sync.DebugException(msg, ex);
			}
		}
		/// <summary>
		/// Logging Message and Exception around synchronizing data
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void SyncDebug(string msg)
		{
			
			{
				sync.Debug(msg);
			}
		}

		/// <summary>
		/// Logging information around synchronizing data
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void SyncInfo(string msg)
		{
			
			{
				sync.Info(msg);
			}
		}

		/// <summary>
		/// Logging Fatal Error Message around synchronizing data
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void SyncFatal(string msg)
		{
			
			{
				sync.Fatal(msg);
			}			
		}

		/// <summary>
		/// Logging Fatal Error Message and Exception around synchronizing data
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void SyncFatalException(string msg, Exception ex)
		{
			
			{
				sync.FatalException(msg, ex);
			}			
		}
		#endregion Sync

		#region Error Log

		/// <summary>
		/// Force a message, undepend of the Configuration.
		/// </summary>
		/// <param name="msg">a <see cref="string"/> message</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Force(string msg)
		{
			error.Info(msg);
		}

		/// <summary>
		/// Debugs the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Debug(string msg, Exception ex)
		{
			
			{
				error.DebugException(System.Environment.MachineName + ": " + msg, ex);
			}
		}
		/// <summary>
		/// Debugs the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Debug(string msg)
		{
			
			{
				error.Debug(System.Environment.MachineName + ": " + msg);
			}
		}
		/// <summary>
		/// Infoes the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Info(string msg, Exception ex)
		{
			
			{
				error.InfoException(System.Environment.MachineName + ": " + msg, ex);
			}
		}
		/// <summary>
		/// Infoes the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Info(string msg)
		{
			
			{
				error.Info(System.Environment.MachineName + ": " + msg);
			}
		}
		/// <summary>
		/// Fatals the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Fatal(string msg, Exception ex)
		{
			error.FatalException(System.Environment.MachineName + ": " + msg, ex);			
		}
		/// <summary>
		/// Fatals the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Fatal(string msg)
		{	
			error.Fatal(System.Environment.MachineName + ": " + msg);
		}

		/// <summary>
		/// Errors the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Error(string msg, Exception ex)
		{
			
			{
				error.ErrorException(System.Environment.MachineName + "; " + msg, ex);
			}			
		}
		/// <summary>
		/// Errors the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG. A <see cref="T:System.String"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Error(string msg)
		{
			
			{
				error.Error(System.Environment.MachineName + ": " + msg);
			}
		}
		/// <summary>
		/// Errors the specified ex.
		/// </summary>
		/// <param name="ex">The ex. A <see cref="T:System.Exception"/> Object.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Error(Exception ex)
		{
			
			{
				Error(System.Environment.MachineName, ex); 
			}			
		}
		#endregion Error Log

		#region Traffic
		/***************************************************************************************/
		/// <summary>
		/// Traffic exceptions.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// <param name="ex">The ex.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		[System.Diagnostics.DebuggerStepThrough]
		public static void TrafficException(string msg, Exception ex)
		{
			traffic.LogException(LogLevel.Error, System.Environment.MachineName + ": " + msg.Replace('\n', '0'), ex);
		}
		/// <summary>
		/// Adding Traffic messages to log
		/// </summary>
		/// <param name="msg">The MSG.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Traffic(string msg)
		{
			traffic.Log(LogLevel.Info, System.Environment.MachineName + ": " + msg);
		}
		#endregion Traffic

		#region Tracking
		/***************************************************************************************/
		/// <summary>
		/// Tracking exception and a message.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// <param name="ex">The ex.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		[System.Diagnostics.DebuggerStepThrough]
		public static void TrackingException(string msg, Exception ex)
		{
			#if TRACE
			
			{
				tracking.LogException(LogLevel.Error, System.Environment.MachineName + ": " + msg.Replace('\n', '0'), ex);
			}
			#endif
		}
		/// <summary>
		/// Trackings the specified MSG.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		[System.Diagnostics.DebuggerStepThrough]
		public static void Tracking(string msg)
		{
			#if TRACE
			
			{
				tracking.Log(LogLevel.Debug, System.Environment.MachineName + ": " + msg);				
			}	
			#endif
		}
		#endregion Tracking

		#region Memory

		/// <summary>
		/// Logs the Memory fatal message.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		[System.Diagnostics.DebuggerStepThrough]
		public static void MemoryFatalException(string msg)
		{
			
			{
				SystemManagement.Cpu.LogCpuData();
				SystemManagement.Memory.LogMemoryData();
				memory.FatalException(msg, new Exception(msg));
			}			
		}

		/// <summary>
		/// Logs the Memory fatal exception.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// <param name="ex">The ex.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		[System.Diagnostics.DebuggerStepThrough]
		public static void MemoryFatalException(string msg, Exception ex)
		{
			SystemManagement.Cpu.LogCpuData();
			SystemManagement.Memory.LogMemoryData();
			memory.FatalException(msg, ex);
		}

		/// <summary>
		/// Memories the debug.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		[System.Diagnostics.DebuggerStepThrough]
		public static void MemoryDebug(string msg)
		{
			
			{
				SystemManagement.Cpu.LogCpuData();
				SystemManagement.Memory.LogMemoryData();
				memory.Debug(msg);
			}
		}
		#endregion Memory
	}
}
