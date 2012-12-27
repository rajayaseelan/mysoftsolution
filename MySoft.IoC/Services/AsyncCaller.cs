using System;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, bool fromServer)
            : base(service, fromServer) { }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, IDataCache cache, bool fromServer)
            : base(service, cache, fromServer) { }

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
                    //返回响应结果
                    resMsg = worker.GetResult(WaitCallback);
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
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            var worker = state as WorkerItem;

            try
            {
                //开始同步调用
                var resMsg = base.GetResponse(worker.Context, worker.Request);

                //设置响应信息
                worker.Set(resMsg);
            }
            catch (Exception ex)
            {
            }
        }
    }
}