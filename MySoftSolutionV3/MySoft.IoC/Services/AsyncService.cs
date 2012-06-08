using System;
using MySoft.IoC.Messages;
using MySoft.Threading;
using System.Threading;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列服务
    /// </summary>
    public class AsyncService : IService
    {
        private ILog logger;
        private IService service;
        private TimeSpan elapsedTime;

        /// <summary>
        /// 实例化QueueService
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        public AsyncService(ILog logger, IService service, TimeSpan elapsedTime)
        {
            this.logger = logger;
            this.service = service;
            this.elapsedTime = elapsedTime;
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //实例化等待对象
            using (var waitResult = new AsyncResult(OperationContext.Current, reqMsg))
            {
                //异步调用
                ThreadPool.QueueUserWorkItem(GetResponse, waitResult);

                //等待响应
                if (!waitResult.Wait(elapsedTime))
                {
                    //结束当前线程
                    waitResult.Cancel();

                    var body = string.Format("Call service ({0}, {1}) timeout ({2}) ms.\r\nParameters => {3}"
                        , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString());

                    //获取异常
                    throw IoCHelper.GetException(OperationContext.Current, reqMsg, body);
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

            try
            {
                var waitResult = state as AsyncResult;

                //设置当前线程
                waitResult.Set(Thread.CurrentThread);

                //设置上下文
                OperationContext.Current = waitResult.Context;

                //调用方法
                var resMsg = service.CallService(waitResult.Request);

                //响应信号
                waitResult.Set(resMsg);
            }
            catch (Exception ex)
            {
                //TODO
                logger.Write(ex);
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
