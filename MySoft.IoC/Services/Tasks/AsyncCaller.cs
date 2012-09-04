using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services.Tasks
{
    /// <summary>
    /// Async method caller
    /// </summary>
    public class AsyncCaller
    {
        private ILog logger;
        private IService service;
        private OperationContext context;
        private RequestMessage reqMsg;

        /// <summary>
        /// Init async caller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public AsyncCaller(ILog logger, IService service, OperationContext context, RequestMessage reqMsg)
        {
            this.logger = logger;
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
            //Set operation context
            OperationContext.Current = context;

            // We know that it's really an AsyncResult<DateTime> object
            AsyncResult<ResponseMessage> ar = (AsyncResult<ResponseMessage>)asyncResult;

            var mre = new ManualResetEvent(false);
            var timeout = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_SERVER_CALL_TIMEOUT);

            // Register wait for single object
            ThreadPool.RegisterWaitForSingleObject(mre, TimerCallback, Thread.CurrentThread, timeout, true);

            try
            {
                // Perform the operation; if sucessful set the result
                var resMsg = service.CallService(reqMsg);

                ar.SetAsCompleted(resMsg, false);
            }
            catch (ThreadAbortException e)
            {
                Thread.ResetAbort();

                string body = string.Format("Remote client【{0}】call service ({1},{2}) timeout {4} ms, the request is aborted.\r\nParameters => {3}",
                        reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), timeout);

                ar.SetAsCompleted(new TimeoutException(body), false);

                //获取异常
                var error = IoCHelper.GetException(OperationContext.Current, reqMsg, new TimeoutException(body));

                logger.WriteError(error);
            }
            catch (Exception e)
            {
                // If operation fails, set the exception
                ar.SetAsCompleted(e, false);
            }
            finally
            {
                mre.Set();

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
                    var ts = SimpleThreadState(thread.ThreadState);

                    if (ts == ThreadState.WaitSleepJoin || ts == ThreadState.Running)
                    {
                        thread.Abort();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private ThreadState SimpleThreadState(ThreadState ts)
        {
            return ts & (ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin |
                         ThreadState.Stopped);
        }
    }
}