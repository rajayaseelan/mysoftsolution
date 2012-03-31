using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Collections;
using System.Text;
using System.Collections.Generic;

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
            var waitResult = new AsyncResult(context, reqMsg);

            //异步调用
            ThreadPool.QueueUserWorkItem(GetResponse, waitResult);

            //等待响应
            if (!waitResult.Wait(elapsedTime))
            {
                throw new WarningException(string.Format("Call service ({0}, {1}) timeout ({2}) ms.\r\nParameters => {3}"
                    , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString()));
            }

            //返回响应的消息
            return waitResult.Message;
        }

        /// <summary>
        /// 响应请求
        /// </summary>
        private void GetResponse(object state)
        {
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
