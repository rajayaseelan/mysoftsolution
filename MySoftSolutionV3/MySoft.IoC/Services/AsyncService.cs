using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    public delegate ResponseMessage AsyncCaller(OperationContext context, RequestMessage reqMsg);

    /// <summary>
    /// 队列服务
    /// </summary>
    public class AsyncService : IService
    {
        private ILog logger;
        private IService service;
        private TimeSpan elapsedTime;
        private int maxCalls;

        /// <summary>
        /// 实例化QueueService
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="maxCalls"></param>
        public AsyncService(ILog logger, IService service, TimeSpan elapsedTime, int maxCalls)
        {
            this.logger = logger;
            this.service = service;
            this.elapsedTime = elapsedTime;
            this.maxCalls = maxCalls;
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            while (ThreadManager.Count > maxCalls)
            {
                //调用项大于最大调用项，则暂停1秒
                Thread.Sleep(1000);
            }

            //实例化异步调用器
            var caller = new AsyncCaller(GetResponse);

            //开始异步调用
            var ar = caller.BeginInvoke(OperationContext.Current, reqMsg, null, caller);

            //等待响应
            if (!ar.AsyncWaitHandle.WaitOne(elapsedTime))
            {
                //结束当前线程
                ThreadManager.Cancel(reqMsg.TransactionId);

                var body = string.Format("Call service ({0}, {1}) timeout ({2}) ms.\r\nParameters => {3}"
                    , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString());

                //获取异常
                var error = IoCHelper.GetException(OperationContext.Current, reqMsg, body);

                //将异常信息写出
                logger.Write(error);

                throw error;
            }

            //返回响应的消息
            return caller.EndInvoke(ar);
        }

        /// <summary>
        /// 响应请求
        /// </summary>
        private ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                //添加当前线程
                ThreadManager.Add(reqMsg.TransactionId, Thread.CurrentThread);

                //设置上下文
                OperationContext.Current = context;

                //调用方法
                return service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                //写错误日志
                logger.Write(ex);

                return null;
            }
            finally
            {
                //移除指定的项
                ThreadManager.Remove(reqMsg.TransactionId);

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
