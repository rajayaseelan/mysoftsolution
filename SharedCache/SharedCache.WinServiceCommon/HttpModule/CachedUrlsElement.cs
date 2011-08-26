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
	public class CachedUrlsElement : System.Configuration.ConfigurationElement
	{

		[System.Configuration.ConfigurationProperty("path", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string Path
		{
			get
			{
				return ((string)(base["path"]));
			}
			set
			{
				base["path"] = value;
			}
		}

		[System.Configuration.ConfigurationProperty("duration", DefaultValue = "00:00:00", IsKey = false, IsRequired = false)]
		public System.TimeSpan Duration
		{
			get
			{
				return ((System.TimeSpan)(base["duration"]));
			}
			set
			{
				base["duration"] = value;
			}
		}

		[System.Configuration.ConfigurationProperty("verbs", DefaultValue = "GET", IsKey = false, IsRequired = false)]
		public string Verbs
		{
			get
			{
				return ((string)(base["verbs"]));
			}
			set
			{
				base["verbs"] = value;
			}
		}

		[System.Configuration.ConfigurationProperty("varyBy", DefaultValue = "", IsKey = false, IsRequired = false)]
		public string VaryBy
		{
			get
			{
				return ((string)(base["varyBy"]));
			}
			set
			{
				base["varyBy"] = value;
			}
		}

		[System.Configuration.ConfigurationProperty("emptyQueryStringOnly", DefaultValue = false, IsKey = false, IsRequired = false)]
		public bool EmptyQueryStringOnly
		{
			get
			{
				return ((bool)(base["emptyQueryStringOnly"]));
			}
			set
			{
				base["emptyQueryStringOnly"] = value;
			}
		}

		[System.Configuration.ConfigurationProperty("emptyPathInfoOnly", DefaultValue = false, IsKey = false, IsRequired = false)]
		public bool EmptyPathInfoOnly
		{
			get
			{
				return ((bool)(base["emptyPathInfoOnly"]));
			}
			set
			{
				base["emptyPathInfoOnly"] = value;
			}
		}

		[System.Configuration.ConfigurationProperty("serveFromMemory", DefaultValue = false, IsKey = false, IsRequired = false)]
		public bool ServeFromMemory
		{
			get
			{
				return ((bool)(base["serveFromMemory"]));
			}
			set
			{
				base["serveFromMemory"] = value;
			}
		}
	}
}
