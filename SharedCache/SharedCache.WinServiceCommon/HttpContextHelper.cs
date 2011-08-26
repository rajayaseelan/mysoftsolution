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

// The code of HttpContext Helper on this page has been taken from (Ryan Olshan):
// http://community.strongcoders.com/blogs/ryan/archive/2005/12/25/httpcontext-helper.aspx
// *************************************************************************
//
// Name:      HttpContextHelper.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

namespace SharedCache.WinServiceCommon
{
	/// <summary>
	/// Encapsulates all HTTP-specific information about an individual HTTP request.
	/// The code of HttpContext Helper on this page has been taken from (Ryan Olshan):
	/// http://community.strongcoders.com/blogs/ryan/archive/2005/12/25/httpcontext-helper.aspx
	/// </summary>
	public class HttpContextHelper
	{

		/// <summary>
		/// Gets the System.Web.HttpApplicationState object for the current HTTP request.
		/// </summary>
		public static HttpApplicationState CurrentApplication
		{
			get { return HttpContext.Current.Application; }
		}

		/// <summary>
		/// Gets the System.Web.Caching.Cache object for the current HTTP request.
		/// </summary>
		public static System.Web.Caching.Cache CurrentCache
		{
			get { return HttpContext.Current.Cache; }
		}

		/// <summary>
		/// Gets the System.Web.HttpRequest object for the current HTTP request.
		/// </summary>
		public static HttpRequest CurrentRequest
		{
			get { return HttpContext.Current.Request; }
		}

		/// <summary>
		/// Gets the System.Web.HttpResponse object for the current HTTP response.
		/// </summary>
		public static HttpResponse CurrentResponse
		{
			get { return HttpContext.Current.Response; }
		}

		/// <summary>
		/// Gets the System.Web.HttpServerUtility object that provides methods used in
		/// processing Web requests.
		/// </summary>
		public static HttpServerUtility CurrentServer
		{
			get { return HttpContext.Current.Server; }
		}

		/// <summary>
		/// Gets the System.Web.SessionState.HttpSessionState object for the current
		/// HTTP request.
		/// </summary>
		public static HttpSessionState CurrentSession
		{
			get { return HttpContext.Current.Session; }
		}
	}
}
