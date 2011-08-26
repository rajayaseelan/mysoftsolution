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
	public class SharedCache : IHttpModule
	{
		HttpApplication _app;
		Tracker _trackerCapturingResponse;

		void IHttpModule.Init(HttpApplication app)
		{
			_app = app;
			//// module's Init method could be called many times,
			//// while Cache needs to be initialized only once.
			//// EnsureInitilized takes care of that
			HttpModule.OutputCache.EnsureInitialized();
			// Cache.EnsureInitialized();

			app.ResolveRequestCache += new EventHandler(OnResolveRequestCache);
			app.UpdateRequestCache += new EventHandler(OnUpdateRequestCache);
			app.EndRequest += new EventHandler(OnEndRequest);
		}

		void IHttpModule.Dispose()
		{
		}

		void OnResolveRequestCache(object sender, EventArgs e)
		{
			// start clean
			_trackerCapturingResponse = null;

			Tracker tracker = HttpModule.OutputCache.Lookup(_app.Context);

			if (tracker == null)
			{
				// this request is not subject to cache
				return;
			}

			// try to send response or start capture
			// (use 'finally' because starting capture would lock)
			try { }
			finally
			{
				if (tracker.TrySendResponseOrStartResponseCapture(_app.Response))
				{
					// successfully sent current response
					_app.CompleteRequest();
				}
				else
				{
					// started capturing
					_trackerCapturingResponse = tracker;
				}
			}
		}

		void OnUpdateRequestCache(object sender, EventArgs e)
		{
			if (_trackerCapturingResponse != null)
			{
				// if capturing, finish the capture and save the file
				_trackerCapturingResponse.FinishCaptureAndSaveResponse(_app.Response);
				_trackerCapturingResponse = null;
			}
		}

		void OnEndRequest(object sender, EventArgs e)
		{
			if (_trackerCapturingResponse != null)
			{
				// if still capturing, abandon the process
				try
				{
					_trackerCapturingResponse.CancelCapture(_app.Response);
				}
				finally
				{
					_trackerCapturingResponse = null;
				}
			}
		}

	}
}
