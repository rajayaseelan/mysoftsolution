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
// Name:      CacheExpire.cs
// 
// Created:   18-07-2007 SharedCache.com, rschuetz
// Modified:  18-07-2007 SharedCache.com, rschuetz : Creation
// Modified:  31-12-2007 SharedCache.com, rschuetz : updated consistency of logging calls
// Modified:  31-12-2007 SharedCache.com, rschuetz : make usage of: COM.Ports.PortTcp() instead of a direct call.
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.IO;

using COM = SharedCache.WinServiceCommon;

namespace SharedCache.WinService
{
	/// <summary>
	/// All needed CacheExpire logic upon Win32 start and stop;
	/// The Init() will run within its own thread.
	/// </summary>
	public class CacheExpire : COM.Extenders.IInit
	{

		/// <summary>
		/// a <see cref="COM.CacheExpire"/> object.
		/// </summary>
		public COM.CacheExpire Expire = null;
		/// <summary>
		/// Defines duration while the thread is cleanung up the Cache.
		/// </summary>
		int sleepDuration = COM.Provider.Server.IndexusServerReplicationCache.ProviderSection.ServerSetting.ServiceCacheCleanupThreadJob;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:CacheExpire"/> class.
		/// </summary>
		public CacheExpire()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (sleepDuration <= 0)
			{
				this.Expire = new COM.CacheExpire(false);
			}
			else
			{
				this.Expire = new COM.CacheExpire(true);
			}
		}

		#region IInit Members

		/// <summary>
		/// Inits this instance.
		/// </summary>
		public void Init()
		{
			#region Access Log
#if TRACE
			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			if (this.Expire.Enable)
			{
				this.ExpireJob();
				COM.Handler.LogHandler.Force("Service CleanUp Job Started" + COM.Enums.LogCategory.ServiceCleanUpStart.ToString());
			}
			else
			{
				COM.Handler.LogHandler.Force("Service CleanUp STOPPED due configuration entry of: ServiceCacheCleanupThreadJob " + sleepDuration.ToString() + " Status: " + this.Expire.Enable.ToString());
				Console.WriteLine("Service CleanUp STOPPED due configuration entry of: ServiceCacheCleanupThreadJob " + sleepDuration.ToString() + " Status: " + this.Expire.Enable.ToString());
				this.Dispose();
			}
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return string.Format(@"Thread Id: {0}; Name: {0}; ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name);
			}
		}

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		public void Dispose()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log
			
			this.Expire = null;
			COM.Handler.LogHandler.Force(string.Format(@"CacheExpire->Dispose - Thread abort's Id: {0}; Name: {0}; ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
			Console.WriteLine(string.Format(@"CacheExpire->Dispose - Thread abort's Id: {0}; Name: {0}; ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
		}

		#endregion

		#region Override Methods
		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			#region Logging
			string msg = "Thread Id: " + Thread.CurrentThread.ManagedThreadId.ToString() + "; Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;";
			COM.Handler.LogHandler.Tracking(msg);
			#endregion Logging

			StringBuilder sb = new StringBuilder();
			#region Start Logging
			COM.Handler.LogHandler.Tracking("Entering method: " + ((object)MethodBase.GetCurrentMethod()).ToString());
			sb.Append("Entering method: " + ((object)MethodBase.GetCurrentMethod()).ToString() + Environment.NewLine);
			#endregion Start Logging

			#region Override ToString() default with reflection
			Type t = this.GetType();
			PropertyInfo[] pis = t.GetProperties();
			for (int i = 0; i < pis.Length; i++)
			{
				try
				{
					PropertyInfo pi = (PropertyInfo)pis.GetValue(i);
					COM.Handler.LogHandler.Info(
					string.Format(
					"{0}: {1}",
					pi.Name,
					pi.GetValue(this, new object[] { })
					)
					);
					sb.AppendFormat("{0}: {1}" + Environment.NewLine, pi.Name, pi.GetValue(this, new object[] { }));
				}
				catch (Exception ex)
				{
					COM.Handler.LogHandler.Error("Could not log property. Ex. Message: " + ex.Message);
				}
			}
			#endregion Override ToString() default with reflection

			return sb.ToString();
		}

		#endregion Override Methods

		/// <summary>
		/// This Job validate the expired items in cache;
		/// </summary>
		private void ExpireJob()
		{
			#region Access Log
#if TRACE			
			{
				COM.Handler.LogHandler.Tracking("Access Method: " + this.GetType().ToString() + "->" + ((object)MethodBase.GetCurrentMethod()).ToString() + " ;");
			}
#endif
			#endregion Access Log

			while (true)
			{
				try
				{
					// verify that the configruation does not run the system into an endless loop
					if (sleepDuration < 1000)
					{
						sleepDuration = 1000;
					}
					
					Thread.Sleep(sleepDuration);
					
					List<string> toRemove = this.Expire.CleanUp();

					#if DEBUG
					Console.WriteLine(@"Executing Cache CleanUp Job: {0:F}; Founded Items: {1} to CleanUp; Current Cache Amount {2}", DateTime.Now, toRemove.Count, TcpServer.LocalCache.Amount());
					#endif

					foreach (string key in toRemove)
					{
						try
						{
							#if DEBUG
							Console.WriteLine("Remove key: {0}", key);
							#else
							COM.Handler.LogHandler.Info(string.Format("Remove key: {0}", key));
              #endif

							TcpServer.LocalCache.Remove(key);
							TcpServer.CacheCleanup.Remove(key);
							this.Expire.Remove(key);

						}
						catch (Exception ex)
						{
							COM.Handler.LogHandler.Error(string.Format("Exception happens while it tries to remove key '{0}' from cache.", key));
							COM.Handler.LogHandler.Error(ex.Message, ex);
						}
					}
					#if DEBUG
					Console.WriteLine("Current Cache Count: {0} [{1:F}]", TcpServer.LocalCache.Amount(), DateTime.Now);
					#else
					COM.Handler.LogHandler.Info(string.Format("Current Cache Count: {0} [{1:F}]", TcpServer.LocalCache.Amount(), DateTime.Now));
					#endif
				}
				catch (Exception ex)
				{
					#if DEBUG
					Console.WriteLine("Exception: " + ex.Message);
					#endif

					COM.Handler.LogHandler.Error(ex.Message, ex);
				}
			}
		}

	}
}
