using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Provides async service for Func T1, T2, T3, TResult
    /// </summary>
    public class ApmHelper<T1, T2, T3, TResult> : ApmHelperBase<TResult>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, T3, TResult> func) : this(func, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, T3, TResult> func, Action<Exception> exceptionHandler)
            : this(func, exceptionHandler, ThreadCallback.AsyncCallerThread)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, T3, TResult> func, ThreadCallback threadCallback)
            : this(func, null, threadCallback)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, T3, TResult> func, Action<Exception> exceptionHandler, ThreadCallback threadCallback)
            : base(func, exceptionHandler, threadCallback)
        {
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(T1 t1, T2 t2, T3 t3, Action<TResult> callback)
        {
            this.InvokeAsync(new List<object> { t1, t2, t3 }, callback);
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(T1 t1, T2 t2, T3 t3, Action<TResult> callback, Action<Exception> exceptionHandler)
        {
            this.InvokeAsync(new List<object> { t1, t2, t3 }, callback, exceptionHandler);
        }
    }

    /// <summary>
    /// Provides async service for Func T1, T2, TResult 
    /// </summary>
    public class ApmHelper<T1, T2, TResult> : ApmHelperBase<TResult>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, TResult> func) : this(func, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, TResult> func, Action<Exception> exceptionHandler)
            : this(func, exceptionHandler, ThreadCallback.AsyncCallerThread)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, TResult> func, ThreadCallback threadCallback)
            : this(func, null, threadCallback)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, T2, TResult> func, Action<Exception> exceptionHandler, ThreadCallback threadCallback)
            : base(func, exceptionHandler, threadCallback)
        {
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(T1 t1, T2 t2, Action<TResult> callback)
        {
            this.InvokeAsync(new List<object> { t1, t2 }, callback);
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(T1 t1, T2 t2, Action<TResult> callback, Action<Exception> exceptionHandler)
        {
            this.InvokeAsync(new List<object> { t1, t2 }, callback, exceptionHandler);
        }
    }

    /// <summary>
    /// Provides async service for Func T1, TResult
    /// </summary>
    public class ApmHelper<T1, TResult> : ApmHelperBase<TResult>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, TResult> func) : this(func, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, TResult> func, Action<Exception> exceptionHandler)
            : this(func, exceptionHandler, ThreadCallback.AsyncCallerThread)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, TResult> func, ThreadCallback threadCallback)
            : this(func, null, threadCallback)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<T1, TResult> func, Action<Exception> exceptionHandler, ThreadCallback threadCallback)
            : base(func, exceptionHandler, threadCallback)
        {
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(T1 t1, Action<TResult> callback)
        {
            this.InvokeAsync(new List<object> { t1 }, callback);
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(T1 t1, Action<TResult> callback, Action<Exception> exceptionHandler)
        {
            this.InvokeAsync(new List<object> { t1 }, callback, exceptionHandler);
        }
    }


    /// <summary>
    /// Provides async service for Func TResult
    /// </summary>
    public class ApmHelper<TResult> : ApmHelperBase<TResult>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<TResult> func) : this(func, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<TResult> func, Action<Exception> exceptionHandler)
            : this(func, exceptionHandler, ThreadCallback.AsyncCallerThread)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<TResult> func, ThreadCallback threadCallback)
            : this(func, null, threadCallback)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ApmHelper(Func<TResult> func, Action<Exception> exceptionHandler, ThreadCallback threadCallback)
            : base(func, exceptionHandler, threadCallback)
        {
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(Action<TResult> callback)
        {
            this.InvokeAsync(new List<object>(), callback);
        }

        /// <summary>
        /// starts the async call
        /// </summary>
        public void InvokeAsync(Action<TResult> callback, Action<Exception> exceptionHandler)
        {
            this.InvokeAsync(new List<object>(), callback, exceptionHandler);
        }
    }

    /// <summary>
    /// This class invokes an AsyncCallback delegate via a specific SynchronizationContext object.
    /// </summary>
    public sealed class SyncContextAsyncCallback
    {
        // One delegate for ALL instances of this class
        private static readonly SendOrPostCallback _sendOrPostCallback = SendOrPostCallback;

        // One SyncContextAsyncCallback object is created 
        // per callback with the following state:
        private readonly SynchronizationContext _syncContext;
        private readonly bool _send; // versus Post
        private readonly AsyncCallback _originalCallback;
        private IAsyncResult _result;

        /// <summary>
        /// Wraps the calling thread's SynchronizationContext object around the specified AsyncCallback.
        /// </summary>
        /// <param name="callback">The method that should be invoked using 
        /// the calling thread's SynchronizationContext.</param>
        /// <returns>The wrapped AsyncCallback delegate.</returns>
        public static AsyncCallback Wrap(AsyncCallback callback)
        {
            return Wrap(callback, false);  // Default to Posting
        }

        /// <summary>
        /// Wraps the calling thread's SynchronizationContext object around the specified AsyncCallback.
        /// </summary>
        /// <param name="callback">The method that should be invoked using 
        /// the calling thread's SynchronizationContext.</param>
        /// <param name="send">true if the AsyncCallback should be invoked via send; false if post.</param>
        /// <returns>The wrapped AsyncCallback delegate.</returns>
        public static AsyncCallback Wrap(AsyncCallback callback, bool send)
        {
            // If no sync context, the just call through the original delegate
            if (null == SynchronizationContext.Current) return callback;
            // If there is a synchronization context, then call through it
            // NOTE: A delegate object is constructed here
            return (new SyncContextAsyncCallback(SynchronizationContext.Current, callback, send)).AsyncCallback;
        }

        private SyncContextAsyncCallback(SynchronizationContext syncContext, AsyncCallback callback, bool send)
        {
            this._syncContext = syncContext;
            this._originalCallback = callback;
            this._send = send;
        }

        private void AsyncCallback(IAsyncResult result)
        {
            this._result = result;
            if (this._send) this._syncContext.Send(_sendOrPostCallback, this);
            else this._syncContext.Post(_sendOrPostCallback, this);
        }

        private static void SendOrPostCallback(object state)
        {
            var scac = (SyncContextAsyncCallback)state;
            scac._originalCallback(scac._result);
        }
    }
}
