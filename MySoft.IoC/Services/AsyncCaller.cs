using System;
using System.Collections;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Threading;

namespace MySoft.IoC.Services
{
    internal class AsyncCaller
    {
        private ILog logger;
        private IService service;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public AsyncCaller(ILog logger, IService service)
        {
            this.logger = logger;
            this.service = service;
        }

        /// <summary>
        /// 同步调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage SyncCall(OperationContext context, RequestMessage reqMsg)
        {
            return GetResponse(service, context, reqMsg);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage AsyncCall(OperationContext context, RequestMessage reqMsg)
        {
            using (var wr = new WaitResult(reqMsg))
            {
                ManagedThreadPool.QueueUserWorkItem(GetResponse, new ArrayList { wr, context, reqMsg });

                var elapsedTime = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_CALL_TIMEOUT);

                if (!wr.Wait(elapsedTime))
                {
                    var body = string.Format("Remote client【{0}】call service ({1},{2}) timeout {4} ms.\r\nParameters => {3}",
                        reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), (int)elapsedTime.TotalMilliseconds);

                    //获取异常
                    var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

                    logger.WriteError(error);

                    var title = string.Format("Call remote service ({0},{1}) timeout {2} ms.",
                                reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds);

                    //处理异常
                    var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));

                    wr.Set(resMsg);
                }

                return wr.Message;
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="state"></param>
        private void GetResponse(object state)
        {
            var arr = state as ArrayList;

            var wr = arr[0] as WaitResult;
            var context = arr[1] as OperationContext;
            var reqMsg = arr[2] as RequestMessage;

            //获取响应的消息
            var resMsg = GetResponse(service, context, reqMsg);

            wr.Set(resMsg);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            try
            {
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                //将异常信息写出
                logger.WriteError(ex);

                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }
    }
}
