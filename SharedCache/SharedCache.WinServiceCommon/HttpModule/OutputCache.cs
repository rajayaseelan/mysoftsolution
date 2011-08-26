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
// Modified:  01-09-2008 SharedCache.com, rschuetz : created
// Modified:  01-09-2008 SharedCache.com, rschuetz : takeover code from "dmitryr's blog": http://blogs.msdn.com/dmitryr/archive/2005/12/13/503411.aspx
// Modified:  01-09-2008 SharedCache.com, rschuetz : in case you are using this use note please that we use here an addapted version of his main 
//																									 distibution, instead of the disc we are using sharedcache as storage provider
//																									 in version 3.0.5.1 this option is not supported - we will make it work for next upcoming release. 
//																									 Licensing:
//=======================================================================
// Copyright (C) Microsoft Corporation.  All rights reserved.
// 
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
//=======================================================================*/
// *************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using System.IO;
using System.Web.Hosting;
using System.Xml;
using System.Web.Compilation;
using System.Web.Configuration;

using _CACHE = SharedCache.WinServiceCommon.Provider.Cache.IndexusDistributionCache;


namespace SharedCache.WinServiceCommon.HttpModule
{
	public class OutputCache : IRegisteredObject
	{

		static HttpModule.OutputCache _theCache = new HttpModule.OutputCache();
		int _varyByLimit = 10;
		// the default location is in temporary asp.net files
		string _location;
		TimeSpan _fileValidationDelay = new TimeSpan(0, 0, 10);
		volatile bool _shuttingDown = false;

		// table of trackers per path
		Dictionary<string, Tracker> _trackers = new Dictionary<string, Tracker>(StringComparer.OrdinalIgnoreCase);

		public static void EnsureInitialized()
		{			
			// _CACHE.SharedCache.Ping(string.Empty);
			available = _theCache.Startup();
		}

		bool Startup()
		{
			DiskOutputCacheSettingsSection config = (DiskOutputCacheSettingsSection)
								WebConfigurationManager.GetWebApplicationSection("diskOutputCacheSettings");

			if (config == null)
			{
				// no config - the list of URLs is empty
				return false;
			}

			if (string.IsNullOrEmpty(config.Location))
			{
				// the default location is in temporary asp.net files
				_location = Path.Combine(HttpRuntime.CodegenDir, "DiskOutputCache");

				if (!Directory.Exists(_location))
				{
					Directory.CreateDirectory(_location);
				}
			}
			else
			{
				_location = config.Location;

				if (!Directory.Exists(_location))
				{
					throw new InvalidDataException(string.Format("Invalid location '{0}'", _location));
				}
			}

			_varyByLimit = config.VaryByLimitPerUrl;
			_fileValidationDelay = config.FileValidationDelay;

			foreach (CachedUrlsElement e in config.CachedUrls)
			{
				string path = e.Path;

				if (!VirtualPathUtility.IsAppRelative(path) && !VirtualPathUtility.IsAbsolute(path))
				{
					throw new InvalidDataException(string.Format("Invalid path '{0}', absolute or app-relative path expected", path));
				}

				path = VirtualPathUtility.ToAbsolute(path);
				// create file path prefix for this path
				string relPathPrefix = VirtualPathUtility.ToAppRelative(path);
				if (relPathPrefix.StartsWith("~/"))
				{
					relPathPrefix = relPathPrefix.Substring(2);
				}
				relPathPrefix = relPathPrefix.Replace('.', '_');
				relPathPrefix = relPathPrefix.Replace('/', '_');

				// list of verbs
				string[] verbs = ParseStringList(e.Verbs);
				if (verbs.Length == 0)
				{
					throw new InvalidDataException(string.Format("Invalid list of verbs '{0}'", e.Verbs));
				}

				// vary-by
				string[] varyBy = ParseStringList(e.VaryBy);

				// remember the tracker object
				_trackers[path] = new Tracker(path, Path.Combine(_location, relPathPrefix), e.Duration,
						verbs, varyBy, e.EmptyQueryStringOnly, e.EmptyPathInfoOnly, e.ServeFromMemory);

			}

			return true;
		}

		static string[] ParseStringList(string listAsString)
		{
			string[] list = listAsString.Trim().Split(',');
			List<string> result = new List<string>(list.Length);

			foreach (string elem in list)
			{
				string s = elem.Trim();
				if (s.Length > 0)
				{
					result.Add(s);
				}
			}

			return result.ToArray();
		}

		public static Tracker Lookup(HttpContext context)
		{
			CheckInitialized();
			return _theCache.LookupTracker(context);
		}
		static bool available;
		static void CheckInitialized()
		{
			if (!available)
				throw new InvalidOperationException("Cache is not available");
		}
		Tracker LookupTracker(HttpContext context)
		{
			HttpRequest request = context.Request;
			Tracker tracker;
			if (_trackers.TryGetValue(request.FilePath, out tracker))
				return tracker.FindTrackerForRequest(request);

			return null;
		}
		internal static string Location
		{
			get { return _theCache._location; }
		}
		internal static TimeSpan FileValidationDelay
		{
			get { return _theCache._fileValidationDelay; }
		}
		internal static int VaryByLimit
		{
			get { return _theCache._varyByLimit; }
		}
		internal static bool ShuttingDown
		{
			get { return _theCache._shuttingDown; }
		}

		public static void ScheduleFileDeletion(string filename)
		{
			_CACHE.SharedCache.Remove(filename);
			//_theCache.AddToRemovalList(filename);
			//_theCache.ScheduleScavanger();
		}

		public void Stop(bool immediate)
		{

		}
	}
}
