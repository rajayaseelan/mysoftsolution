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
// Name:      Cpu.cs
// 
// Created:   29-09-2007 SharedCache.com, rschuetz
// Modified:  29-09-2007 SharedCache.com, rschuetz : Creation
// Modified:  24-02-2008 SharedCache.com, rschuetz : updated logging part for tracking, instead of using appsetting we use precompiler definition #if TRACE
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management;
using System.Text;
using System.Reflection;

namespace SharedCache.WinServiceCommon.SystemManagement
{
	/// <summary>
	/// Represents the CPU scope information
	/// </summary>
	public class Cpu
	{
		/// <summary>
		/// Logs the cpu data.
		/// </summary>
		public static void LogCpuData()
		{
			#region Access Log
			#if TRACE
			
			{
				Handler.LogHandler.Tracking("Access Method: " + typeof(Cpu).FullName + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			ManagementScope mgmtScope = new ManagementScope(@"\\.\root\cimv2");
			mgmtScope.Connect();

			ManagementPath mp = new ManagementPath("Win32_Processor");
			ManagementClass mc = new ManagementClass(mgmtScope, mp, null);
			ManagementObjectCollection procs = mc.GetInstances();

			foreach (ManagementObject mo in procs)
			{
				foreach (PropertyData pd in mo.Properties)
				{
					switch (pd.Name)
					{
						case "DeviceID":
						case "Name":
						case "LoadPercentage":
							Console.WriteLine(@"Name: {0}; Value: {1}", pd.Name, pd.Value == null ? string.Empty : pd.Value.ToString().Trim());
							Handler.LogHandler.Info(string.Format(@"Name: {0}; Value: {1}", pd.Name, pd.Value == null ? string.Empty : pd.Value.ToString().Trim()));
							Handler.LogHandler.MemoryFatalException(string.Format(@"Name: {0}; Value: {1}", pd.Name, pd.Value == null ? string.Empty : pd.Value.ToString().Trim()));

							break;
					}
				}
				Console.WriteLine(@" --- ");
				Handler.LogHandler.Info(@" --- ");
			}
		}
	}
}
