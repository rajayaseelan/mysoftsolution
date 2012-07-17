using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.IoC.Communication.Scs.Server;

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
        private IScsServerClient client;
        private IService service;
        private AsyncMethodCaller caller;
        private AutoResetEvent autoEvent;

        /// <summary>
        /// Init async caller
        /// </summary>
        /// <param name="client"></param>
        /// <param name="service"></param>
        public AsyncCaller(IScsServerClient client, IService service)
        {
            this.client = client;
            this.service = service;
            this.autoEvent = new AutoResetEvent(false);
        }

        // Asynchronous version of time-consuming method (Begin part)
        public IAsyncResult BeginDoTask(AsyncCallerArgs args, AsyncCallback callback, Object state)
        {
            this.caller = new AsyncMethodCaller(WorkCallback);

            //Cancel callback
            ThreadPool.QueueUserWorkItem(TimerCallback, args.ReqMsg);

            // Return the IAsyncResult to the caller
            return caller.BeginInvoke(args, callback, state);
        }

        // Asynchronous version of time-consuming method (End part)
        public ResponseMessage EndDoTask(IAsyncResult ar)
        {
            // Wait for operation to complete, then return result or 
            var resMsg = caller.EndInvoke(ar);

            ar.AsyncWaitHandle.Close();

            caller = null;

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

        /// <summary>
        /// Timer callback
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallback(object state)
        {
            var reqMsg = state as RequestMessage;

            try
            {
                //Sleep timeout
                if (!autoEvent.WaitOne(TimeSpan.FromSeconds((reqMsg.Timeout * 1.0) / 2)))
                {
                    //Cancel thread
                    ThreadManager.Cancel(reqMsg.TransactionId);

                    Console.WriteLine("Thread manager count: {0}!", ThreadManager.Count);
                }
            }
            catch (Exception ex)
            {
                //Remove thread
                ThreadManager.Remove(reqMsg.TransactionId);
            }
        }

        // Asynchronous version of time-consuming method (private part 
        // to set completion result/exception)
        private ResponseMessage WorkCallback(AsyncCallerArgs args)
        {
            var reqMsg = args.ReqMsg;

            //Set operation context
            OperationContext.Current = args.Context;

            try
            {
                ThreadManager.Add(reqMsg.TransactionId, Thread.CurrentThread);

                // Perform the operation; if sucessful set the result
                return service.CallService(reqMsg);
            }
            finally
            {
                ThreadManager.Remove(reqMsg.TransactionId);

                autoEvent.Set();

                //Set operation context
                OperationContext.Current = null;
            }
        }
    }
}