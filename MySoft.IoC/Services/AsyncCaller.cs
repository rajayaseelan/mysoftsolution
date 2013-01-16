using System;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步方法调用
    /// </summary>
    /// <param name="context"></param>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncMethodCaller(OperationContext context, RequestMessage reqMsg);

    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private TimeSpan timeout;
        private AsyncMethodCaller caller;
        private const int TIMEOUT = 5 * 60; //超时时间为300秒

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, bool fromServer)
            : base(service, fromServer)
        {
            this.timeout = TimeSpan.FromSeconds(TIMEOUT);
            this.caller = new AsyncMethodCaller(base.GetResponse);
        }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, IDataCache cache, bool fromServer)
            : base(service, cache, fromServer)
        {
            this.timeout = TimeSpan.FromSeconds(TIMEOUT);
            this.caller = new AsyncMethodCaller(base.GetResponse);
        }

        /// <summary>
        /// 异常调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        protected override ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            //异步调用
            using (var worker = new WorkerItem(context, reqMsg))
            {
                ResponseMessage resMsg = null;

                try
                {
                    using (var flowControl = ExecutionContext.SuppressFlow())
                    {
                        //开始异步请求
                        caller.BeginInvoke(context, reqMsg, AsyncCallback, worker);
                    }

                    //返回响应结果
                    resMsg = worker.GetResult(timeout);
                }
                catch (Exception ex)
                {
                    //处理异常响应
                    resMsg = IoCHelper.GetResponse(reqMsg, ex);
                }

                return resMsg;
            }
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var worker = ar.AsyncState as WorkerItem;

            try
            {
                //开始同步调用
                var resMsg = caller.EndInvoke(ar);

                //设置响应信息
                worker.Set(resMsg);
            }
            catch (Exception ex) { }
            finally
            {
                ar.AsyncWaitHandle.Close();
            }
        }
    }
}