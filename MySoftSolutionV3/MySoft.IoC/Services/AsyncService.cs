using System;
using MySoft.IoC.Messages;
using MySoft.Threading;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列服务
    /// </summary>
    public class AsyncService : IService
    {
        private IService service;
        private TimeSpan elapsedTime;

        /// <summary>
        /// 实例化QueueService
        /// </summary>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        public AsyncService(IService service, TimeSpan elapsedTime)
        {
            this.service = service;
            this.elapsedTime = elapsedTime;
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //参数信息
            var context = OperationContext.Current;

            //实例化等待对象
            using (var waitResult = new AsyncResult(context, reqMsg))
            {
                //异步调用
                ThreadPool.QueueUserWorkItem(GetResponse, waitResult);

                //等待响应
                if (!waitResult.Wait(elapsedTime))
                {
                    var exception = new WarningException(string.Format("Call service ({0}, {1}) timeout ({2}) ms.\r\nParameters => {3}"
                        , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString()))
                    {
                        ApplicationName = reqMsg.AppName,
                        ServiceName = reqMsg.ServiceName,
                        ErrorHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };

                    //上下文不为null
                    if (OperationContext.Current != null && OperationContext.Current.Caller != null)
                    {
                        var caller = OperationContext.Current.Caller;
                        if (!string.IsNullOrEmpty(caller.AppPath))
                        {
                            exception.ErrorHeader = string.Format("{0}\r\nApplication Path: {1}", exception.ErrorHeader, caller.AppPath);
                        }
                    }

                    throw exception;
                }

                //返回响应的消息
                return waitResult.Message;
            }
        }

        /// <summary>
        /// 响应请求
        /// </summary>
        private void GetResponse(object state)
        {
            //如果值为null则返回
            if (state == null) return;

            var waitResult = state as AsyncResult;

            //设置上下文
            OperationContext.Current = waitResult.Context;

            try
            {
                //调用方法
                var resMsg = service.CallService(waitResult.Request);

                //响应信号
                waitResult.Set(resMsg);
            }
            catch
            {
                //内部异常不做处理
            }
            finally
            {
                //初始化上下文
                OperationContext.Current = null;
            }
        }

        #region IService 成员

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get { return service.ServiceName; }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            service.Dispose();
        }

        #endregion
    }
}
