using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.IoC.Services.Tasks;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Async method caller
    /// </summary>
    public class AsyncCaller
    {
        private IService service;
        private OperationContext context;
        private RequestMessage reqMsg;

        /// <summary>
        /// Init async caller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public AsyncCaller(IService service, OperationContext context, RequestMessage reqMsg)
        {
            this.service = service;
            this.context = context;
            this.reqMsg = reqMsg;
        }

        // Asynchronous version of time-consuming method (Begin part)
        public IAsyncResult BeginDoTask(AsyncCallback callback, Object state)
        {
            // Create IAsyncResult object identifying the 
            // asynchronous operation
            AsyncResult<ResponseMessage> ar = new AsyncResult<ResponseMessage>(
              callback, state);

            // Use a thread pool thread to perform the operation
            ThreadPool.QueueUserWorkItem(DoTaskHelper, ar);

            return ar;  // Return the IAsyncResult to the caller
        }

        // Asynchronous version of time-consuming method (End part)
        public ResponseMessage EndDoTask(IAsyncResult ar)
        {
            // We know that the IAsyncResult is really an 
            // AsyncResult<DateTime> object
            AsyncResult<ResponseMessage> asyncResult = (AsyncResult<ResponseMessage>)ar;

            // Wait for operation to complete, then return result or 
            // throw exception
            return asyncResult.EndInvoke();
        }

        // to set completion result/exception)
        private void DoTaskHelper(Object asyncResult)
        {
            // We know that it's really an AsyncResult<DateTime> object
            AsyncResult<ResponseMessage> ar = (AsyncResult<ResponseMessage>)asyncResult;

            //Set operation context
            OperationContext.Current = context;
            ManualResetEvent reset = new ManualResetEvent(false);

            try
            {
                // Register wait for single object
                var milliseconds = TimeSpan.FromSeconds(reqMsg.Timeout).TotalMilliseconds;
                ThreadPool.RegisterWaitForSingleObject(reset, TimerCallback, Thread.CurrentThread, (int)milliseconds / 2, true);

                // Perform the operation; if sucessful set the result
                var resMsg = service.CallService(reqMsg);
                ar.SetAsCompleted(resMsg, false);
            }
            catch (Exception e)
            {
                // If operation fails, set the exception
                ar.SetAsCompleted(e, false);
            }
            finally
            {
                reset.Set();

                //Set operation context null
                OperationContext.Current = null;
            }
        }

        /// <summary>
        /// Call method timer callback
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timedOut"></param>
        private void TimerCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                try
                {
                    var thread = state as Thread;
                    thread.Abort();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}