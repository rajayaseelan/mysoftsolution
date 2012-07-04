using System;
using System.Collections;
using System.Threading;
using MySoft.IoC.Messages;
using System.Diagnostics;
using MySoft.IoC.Communication.Scs.Server;

namespace MySoft.IoC.Services.Async
{
    /// <summary>
    /// Async method caller
    /// </summary>
    internal class AsyncCaller
    {
        private IService service;
        private AsyncCallerArgs args;
        private Stopwatch watch;

        /// <summary>
        /// Call method elapsed milliseconds
        /// </summary>
        public long ElapsedMilliseconds { get { return watch.ElapsedMilliseconds; } }

        /// <summary>
        ///  nit async caller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="args"></param>
        public AsyncCaller(IService service, AsyncCallerArgs args)
        {
            this.service = service;
            this.args = args;

            this.watch = new Stopwatch();
        }

        // Asynchronous version of time-consuming method (Begin part)
        public IAsyncResult BeginDoTask(AsyncCallback callback, Object state)
        {
            var context = OperationContext.Current;

            // Create IAsyncResult object identifying the 
            // asynchronous operation
            AsyncResult<ResponseMessage> ar = new AsyncResult<ResponseMessage>(
               callback, state);

            // Use a thread pool thread to perform the operation
            ThreadPool.QueueUserWorkItem(DoTaskHelper, ar);

            return ar;  // Return the IAsyncResult to the caller
        }

        // Asynchronous version of time-consuming method (End part)
        public ResponseMessage EndDoTask(IAsyncResult asyncResult)
        {
            // We know that the IAsyncResult is really an 
            // AsyncResult<DateTime> object
            AsyncResult<ResponseMessage> ar = (AsyncResult<ResponseMessage>)asyncResult;

            // Wait for operation to complete, then return result or 
            // throw exception
            return ar.EndInvoke();
        }

        // Asynchronous version of time-consuming method (private part 
        // to set completion result/exception)
        private void DoTaskHelper(Object asyncResult)
        {
            //start
            watch.Start();

            // We know that it's really an AsyncResult<DateTime> object
            AsyncResult<ResponseMessage> ar = (AsyncResult<ResponseMessage>)asyncResult;

            try
            {
                //Set operation context
                OperationContext.Current = args.Context;

                var resMsg = service.CallService(args.Request);

                // Perform the operation; if sucessful set the result
                ar.SetAsCompleted(resMsg, false);
            }
            catch (Exception e)
            {
                // If operation fails, set the exception
                ar.SetAsCompleted(e, false);
            }
            finally
            {
                //stop
                watch.Stop();

                //Set operation context
                OperationContext.Current = null;
            }
        }
    }
}
