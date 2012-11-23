using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 
    /// </summary>
    public enum ThreadCallback
    {
        /// <summary>
        /// This is the thread that InvokeAsync() is called on
        /// </summary>
        AsyncCallerThread,
        /// <summary>
        /// This is the thread the InvokeAsync() spawned
        /// </summary>
        WorkerThread
    }

    /// <summary>
    /// This class is responsible for tracking the IAsyncResult from a BeginInvoke(),
    /// Catching exceptions from EndInvoke(), and Marshalling the data or exception to the
    /// correct thread
    /// </summary>
    /// <typeparam name="TResult">result of function call</typeparam>
    public abstract class ApmHelperBase<TResult> : IDisposable
    {
        /// <summary>
        /// lock to preserve thread safety on getting/setting IAsyncResult
        /// </summary>
        private readonly object _iAsynclock = new object();
        /// <summary>
        /// Pointer to method to execute when EndInvoke() throws exception
        /// </summary>
        private readonly Action<Exception> _defaultExceptionHandler;
        /// <summary>
        /// Method to get begin, end invoke functions from
        /// </summary>
        private readonly Delegate _function;
        /// <summary>
        /// cache of BeginInvoke method
        /// </summary>
        private readonly MethodInfo _beginInvokeMethod;
        /// <summary>
        /// cache of EndInvoke method
        /// </summary>
        private readonly MethodInfo _endInvokeMethod;
        /// <summary>
        /// cache of callback that all BeginInvoke()s wire to
        /// that will issue callback to correct thread
        /// </summary>
        private readonly AsyncCallback _postCallback;
        /// <summary>
        /// Cache of the current call from BeginInvoke()
        /// </summary>
        private IAsyncResult _current;
        /// <summary>
        /// cache the current synchronization context
        /// </summary>
        private readonly SynchronizationContext _context;
        /// <summary>
        /// Offer ability to timeout on async calls
        /// </summary>
        private readonly Timer _timer;
        /// <summary>
        /// lock for timer (timer callback on bg thread)
        /// </summary>
        private readonly object _timerLock = new object();
        /// <summary>
        /// optional timeout to set for calls
        /// </summary>
        private int _timeout;

        /// <summary>
        /// ctor
        /// </summary>
        protected ApmHelperBase(Delegate function, Action<Exception> exceptionHandler)
            : this(function, exceptionHandler, ThreadCallback.AsyncCallerThread) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="function">function user is going to bind to..This will actually be some func but we don't care here.  Ultimately
        /// this function will be called asynchronously on the bg thread</param>
        /// <param name="exceptionHandler">optional method user wants to be notified if async call throws exceptino</param>
        /// <param name="threadCallback">Which thread should callbacks occur on</param>
        protected ApmHelperBase(Delegate function, Action<Exception> exceptionHandler, ThreadCallback threadCallback)
        {
            if (null == function) throw new ArgumentNullException("function");
            this._function = function;
            // cache the methods
            Type type = function.GetType();
            this._beginInvokeMethod = type.GetMethod("BeginInvoke");
            this._endInvokeMethod = type.GetMethod("EndInvoke");
            // if no ex handler, we use our own
            this._defaultExceptionHandler = exceptionHandler ?? DefaultExceptionHandler;
            // all async calls will get pointed to this callback
            this._postCallback = this.PostCallbackToCorrectThread;
            // cache which thread user wants callbacks on..he can change later if he wants
            this.TheThreadCallback = threadCallback;
            // cache the sync context
            this._context = SynchronizationContext.Current;
            // get our timer ready to support timeouts
            this._timer = new Timer(this.TimerCallback);
            // start with no timeout set.  User must specify
            this.TurnTimerOff();
        }

        /// <summary>
        /// Setting this to any postive value will turn on the timer for each call.  
        /// Less than or equal to 0 will turn it off.
        /// </summary>
        /// <remarks>can't be used when IssueCallbacksOnInvokesAsync == true</remarks>
        public int MillisecondTimeout
        {
            get { return this._timeout; }
            set
            {
                this._timeout = value;
                if (value > 0 && this.IssueCallbacksOnInvokesAsync)
                    throw new InvalidOperationException("Can't use a timer if you want all callbacks.  If you want to use the timer, set IssueCallbacksOnInvokesAsyn = false;");
            }
        }

        /// <summary>
        /// User can set if they want callbacks to occur on the 
        /// thread this object is called InvokeAsycn(), or on the thread
        /// spawned from the BeingInvoke() method.
        /// </summary>
        /// <remarks>This applies to the exception handler as well</remarks>
        public ThreadCallback TheThreadCallback { get; set; }

        /// <summary>
        /// Provides the asyncresult in a threadsafe manor
        /// </summary>
        public IAsyncResult CurrentIAsyncResult
        {
            get { lock (this._iAsynclock) return this._current; }
            private set { lock (this._iAsynclock) this._current = value; }
        }

        /// <summary>
        /// Sets IssueCallbacksOnInvokesAsync to false and
        /// wipes out the CurrentIAsyncResult so no callback fires
        /// </summary>
        /// <remarks>Subsequent calls to InvokeAsync() will
        /// set the CurrentIAsyncResult so the last InvokeAsync() will get the callback</remarks>
        public void Cancel()
        {
            this.IssueCallbacksOnInvokesAsync = false;
            this.CurrentIAsyncResult = null;
        }

        /// <summary>
        /// If true, APMHelper will issue callbacks on ALL BeginInvokes(), otherwise
        /// only the last InvokeAsync() gets the callback
        /// </summary>
        public bool IssueCallbacksOnInvokesAsync { get; set; }

        /// <summary>
        /// Ignores all outstanding calls.
        /// </summary>
        /// <remarks>You could actually start using it again</remarks>
        public void Dispose()
        {
            this.Cancel();
        }

        /// <summary>
        /// User should convert his arguments in order into args parm
        /// </summary>
        /// <param name="args">T1, T2..etc</param>
        /// <param name="userCallback">method user wants results pumped to</param>
        /// <param name="exceptionHandler">method user wants exceptions pumped into</param>
        protected void InvokeAsync(List<object> args, Action<TResult> userCallback, Action<Exception> exceptionHandler)
        {
            // if a sync context is available and user wants callback on AsyncCallerThread,
            // then callback will happen on the thread calling this method now.
            // Otherwise, the normal bg thread will call the callback
            args.Add(this._postCallback);
            // we need to pass in the pointer to the method the user wants his notification
            args.Add(new CallbackState(userCallback, exceptionHandler));
            // even though we call Invoke, we are actually calling the BeginInvoke() so this won't block
            this.CurrentIAsyncResult = (IAsyncResult)this._beginInvokeMethod.Invoke(this._function, args.ToArray());
            // if we have a timeout set then we want to be notified 
            if (this.MillisecondTimeout > 0)
            {
                this.Timer.Change(this.MillisecondTimeout, Timeout.Infinite);
            }
        }

        /// <summary>
        /// User should convert his arguments in order into args parm
        /// </summary>
        /// <param name="args">T1, T2..etc</param>
        /// <param name="userCallback">method user wants results pumped to</param>
        protected void InvokeAsync(List<object> args, Action<TResult> userCallback)
        {
            this.InvokeAsync(args, userCallback, this._defaultExceptionHandler);
        }

        /// <summary>
        /// Intercept callback to actually post to real callback to correct thread
        /// </summary>
        private void PostCallbackToCorrectThread(IAsyncResult result)
        {
            if (this.TheThreadCallback == ThreadCallback.AsyncCallerThread && null != this._context)
            {
                this._context.Post(x => this.Callback((IAsyncResult)x), result);
            }
            else
            {
                this.Callback(result);
            }
        }

        /// <summary>
        /// all async calls come through here to make sure they are valid
        /// </summary>
        /// <remarks>by this point, this method is executing on the correct thread</remarks>
        private void Callback(IAsyncResult result)
        {
            this.Timer.Change(Timeout.Infinite, Timeout.Infinite);
            var callbackState = (CallbackState)((AsyncResult)result).AsyncState;
            TResult output;
            try
            {
                // get our results
                output = (TResult)this._endInvokeMethod.Invoke(this._function, new[] { result });
            }
            catch (Exception ex)
            {
                if (result == this.CurrentIAsyncResult || this.IssueCallbacksOnInvokesAsync)
                {
                    // get our callback
                    ExecuteExceptionHandler(ex, callbackState.ExceptionHandler);
                }
                return;
            }
            if (!(result == this.CurrentIAsyncResult || this.IssueCallbacksOnInvokesAsync)) return;
            if (null == callbackState.UserCallback) return; // user might have just issued fire and forget
            // notify the user
            callbackState.UserCallback(output);
        }

        /// <summary>
        /// Provide threadsafe access to timer obj
        /// </summary>
        private Timer Timer
        {
            get { lock (this._timerLock) return this._timer; }
        }

        /// <summary>
        /// Sets timeout to infinite so we don't raise timer callback
        /// </summary>
        private void TurnTimerOff()
        {
            this.Timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// We had a timeout
        /// </summary>
        /// <param name="o"></param>
        private void TimerCallback(object o)
        {
            // might be a little noise in the timers..we put this in just to be safe.
            IAsyncResult iasyncResult = this.CurrentIAsyncResult;
            if (iasyncResult.IsCompleted) return;

            // don't want more callacks until user does another InvokeAsync()
            this.TurnTimerOff();
            // we are going to ignore any results since we had a timeout
            this.Cancel();
            // need this to get the exception handler user wanted
            var callbackState = (CallbackState)iasyncResult.AsyncState;

            var timeoutException = new System.TimeoutException("Async operation timed out");
            if (this.TheThreadCallback == ThreadCallback.AsyncCallerThread && null != this._context)
            {
                this._context.Post(x => ExecuteExceptionHandler(timeoutException, (Action<Exception>)x),
                                   callbackState.ExceptionHandler);
            }
            else
            {
                ExecuteExceptionHandler(timeoutException, callbackState.ExceptionHandler);
            }
        }

        private static void ExecuteExceptionHandler(Exception exception, Action<Exception> exHandler)
        {
            if (null != exHandler) exHandler(exception);
        }

        private static void DefaultExceptionHandler(Exception ex)
        {
            // log maybe?
            throw ex;
        }

        private sealed class CallbackState
        {
            public readonly Action<TResult> UserCallback;
            public readonly Action<Exception> ExceptionHandler;
            public CallbackState(Action<TResult> userCallback, Action<Exception> exceptionHandler)
            {
                this.UserCallback = userCallback;
                this.ExceptionHandler = exceptionHandler;
            }
        }
    }
}
