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
// Name:      IndexusProviderBase.cs
// 
// Created:   23-09-2007 SharedCache.com, rschuetz
// Modified:  23-09-2007 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text;
using System.Web.SessionState;

namespace SharedCache.WinServiceCommon.Provider.Session
{
	/// <summary>
	/// IndexusProviderBase inherites from <see cref="SessionStateStoreProviderBase"/>
	/// </summary>
	public abstract class IndexusProviderBase : SessionStateStoreProviderBase
	{
		/// <summary>
		/// Creates a new <see cref="T:System.Web.SessionState.SessionStateStoreData"></see> object to be used for the current request.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="timeout">The session-state <see cref="P:System.Web.SessionState.HttpSessionState.Timeout"></see> value for the new <see cref="T:System.Web.SessionState.SessionStateStoreData"></see>.</param>
		/// <returns>
		/// A new <see cref="T:System.Web.SessionState.SessionStateStoreData"></see> for the current request.
		/// </returns>
		public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Adds a new session-state item to the data store.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"></see> for the current request.</param>
		/// <param name="timeout">The session <see cref="P:System.Web.SessionState.HttpSessionState.Timeout"></see> for the current request.</param>
		public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Releases all resources used by the <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"></see> implementation.
		/// </summary>
		public override void Dispose()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Called by the <see cref="T:System.Web.SessionState.SessionStateModule"></see> object at the end of a request.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		public override void EndRequest(System.Web.HttpContext context)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Returns read-only session-state data from the session data store.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"></see> for the current request.</param>
		/// <param name="locked">When this method returns, contains a Boolean value that is set to true if the requested session item is locked at the session data store; otherwise, false.</param>
		/// <param name="lockAge">When this method returns, contains a <see cref="T:System.TimeSpan"></see> object that is set to the amount of time that an item in the session data store has been locked.</param>
		/// <param name="lockId">When this method returns, contains an object that is set to the lock identifier for the current request. For details on the lock identifier, see "Locking Session-Store Data" in the <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"></see> class summary.</param>
		/// <param name="actions">When this method returns, contains one of the <see cref="T:System.Web.SessionState.SessionStateActions"></see> values, indicating whether the current session is an uninitialized, cookieless session.</param>
		/// <returns>
		/// A <see cref="T:System.Web.SessionState.SessionStateStoreData"></see> populated with session values and information from the session data store.
		/// </returns>
		public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Returns read-only session-state data from the session data store.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"></see> for the current request.</param>
		/// <param name="locked">When this method returns, contains a Boolean value that is set to true if a lock is successfully obtained; otherwise, false.</param>
		/// <param name="lockAge">When this method returns, contains a <see cref="T:System.TimeSpan"></see> object that is set to the amount of time that an item in the session data store has been locked.</param>
		/// <param name="lockId">When this method returns, contains an object that is set to the lock identifier for the current request. For details on the lock identifier, see "Locking Session-Store Data" in the <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"></see> class summary.</param>
		/// <param name="actions">When this method returns, contains one of the <see cref="T:System.Web.SessionState.SessionStateActions"></see> values, indicating whether the current session is an uninitialized, cookieless session.</param>
		/// <returns>
		/// A <see cref="T:System.Web.SessionState.SessionStateStoreData"></see> populated with session values and information from the session data store.
		/// </returns>
		public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Called by the <see cref="T:System.Web.SessionState.SessionStateModule"></see> object for per-request initialization.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		public override void InitializeRequest(System.Web.HttpContext context)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Releases a lock on an item in the session data store.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The session identifier for the current request.</param>
		/// <param name="lockId">The lock identifier for the current request.</param>
		public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Deletes item data from the session data store.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The session identifier for the current request.</param>
		/// <param name="lockId">The lock identifier for the current request.</param>
		/// <param name="item">The <see cref="T:System.Web.SessionState.SessionStateStoreData"></see> that represents the item to delete from the data store.</param>
		public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Updates the expiration date and time of an item in the session data store.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The session identifier for the current request.</param>
		public override void ResetItemTimeout(System.Web.HttpContext context, string id)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Updates the session-item information in the session-state data store with values from the current request, and clears the lock on the data.
		/// </summary>
		/// <param name="context">The <see cref="T:System.Web.HttpContext"></see> for the current request.</param>
		/// <param name="id">The session identifier for the current request.</param>
		/// <param name="item">The <see cref="T:System.Web.SessionState.SessionStateStoreData"></see> object that contains the current session values to be stored.</param>
		/// <param name="lockId">The lock identifier for the current request.</param>
		/// <param name="newItem">true to identify the session item as a new item; false to identify the session item as an existing item.</param>
		public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Sets a reference to the <see cref="T:System.Web.SessionState.SessionStateItemExpireCallback"></see> delegate for the Session_OnEnd event defined in the Global.asax file.
		/// </summary>
		/// <param name="expireCallback">The <see cref="T:System.Web.SessionState.SessionStateItemExpireCallback"></see>  delegate for the Session_OnEnd event defined in the Global.asax file.</param>
		/// <returns>
		/// true if the session-state store provider supports calling the Session_OnEnd event; otherwise, false.
		/// </returns>
		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
