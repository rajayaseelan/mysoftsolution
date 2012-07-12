using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Async method caller delegate
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate ResponseMessage AsyncMethodCaller(AsyncCallerArgs args);

    /// <summary>
    /// Async method caller
    /// </summary>
    public class AsyncCaller
    {
        private IService service;
        private AsyncMethodCaller caller;

        /// <summary>
        /// Init async caller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="args"></param>
        public AsyncCaller(IService service)
        {
            this.service = service;
        }

        // Asynchronous version of time-consuming method (Begin part)
        public IAsyncResult BeginDoTask(AsyncCallerArgs args, AsyncCallback callback, Object state)
        {
            this.caller = new AsyncMethodCaller(WorkCallback);

            // Return the IAsyncResult to the caller
            return caller.BeginInvoke(args, callback, state);
        }

        // Asynchronous version of time-consuming method (End part)
        public ResponseMessage EndDoTask(IAsyncResult ar)
        {
            // Wait for operation to complete, then return result or 
            var resMsg = caller.EndInvoke(ar);

            ar.AsyncWaitHandle.SafeWaitHandle.Dispose();
            ar.AsyncWaitHandle.Close();

            return resMsg;
        }

        /// <summary>
        /// Get thread state
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private ThreadState GetThreadState(ThreadState ts)
        {
            return ts & (ThreadState.Aborted | ThreadState.AbortRequested |
                         ThreadState.Stopped | ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin);
        }

        // Asynchronous version of time-consuming method (private part 
        // to set completion result/exception)
        private ResponseMessage WorkCallback(object state)
        {
            var args = state as AsyncCallerArgs;

            var context = args.Context;
            var reqMsg = args.ReqMsg;

            //Set operation context
            OperationContext.Current = context;

            try
            {
                // Use a thread pool thread to perform the operation
                var elapsedTime = TimeSpan.FromSeconds((reqMsg.Timeout * 1.0) / 2);

                AutoResetEvent e = new AutoResetEvent(false);

                try
                {
                    //Register cancel callback
                    ThreadPool.RegisterWaitForSingleObject(e, TimerCallback, Thread.CurrentThread, elapsedTime, true);

                    // Perform the operation; if sucessful set the result
                    return service.CallService(reqMsg);
                }
                finally
                {
                    e.Set();
                    e.SafeWaitHandle.Dispose();
                    e = null;
                }
            }
            finally
            {
                //Set operation context
                OperationContext.Current = null;
            }
        }

        /// <summary>
        /// Cancel callback
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timedOut"></param>
        private void TimerCallback(object state, bool timedOut)
        {
            if (!timedOut) return;
            if (state == null) return;

            try
            {
                var thread = state as Thread;

                //中止当前线程
                var ts = GetThreadState(thread.ThreadState);

                if (ts == ThreadState.WaitSleepJoin)
                {
                    thread.Interrupt();
                }
                else if (ts == ThreadState.Running)
                {
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
        }
    }
}