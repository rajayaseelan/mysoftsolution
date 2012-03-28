using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列服务
    /// </summary>
    public class QueueService : IService
    {
        private IService service;
        private TimeSpan elapsedTime;
        private string queueKey;
        private Queue<QueueResult> queue;
        private bool isRunning;

        /// <summary>
        /// 实例化QueueService
        /// </summary>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="queueKey"></param>
        public QueueService(IService service, TimeSpan elapsedTime, string queueKey)
        {
            this.service = service;
            this.elapsedTime = elapsedTime;
            this.queueKey = queueKey;
            this.queue = new Queue<QueueResult>();
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //判断是否运行
            if (this.isRunning)
            {
                var waitResult = new QueueResult(reqMsg);
                lock (queue)
                {
                    queue.Enqueue(waitResult);
                }

                //等待响应
                if (!waitResult.Wait(elapsedTime))
                {
                    throw new WarningException(string.Format("Call service ({0}, {1}) timeout ({2}) ms."
                        , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds));
                }

                return waitResult.Message;
            }

            //初始化
            this.isRunning = true;

            //返回调用值
            var resMsg = service.CallService(reqMsg);

            //响应请求
            if (queue.Count > 0)
            {
                this.SetResponse(resMsg);
            }

            //重置状态
            this.isRunning = false;

            return resMsg;
        }

        /// 响应请求
        /// </summary>
        /// <param name="resMsg"></param>
        private void SetResponse(ResponseMessage resMsg)
        {
            Console.WriteLine("Queue Length: {0}\r\n=> Queue Key: {1}", queue.Count, queueKey);

            //响应数据
            while (queue.Count > 0)
            {
                var waitResult = queue.Dequeue();

                //处理返回消息
                waitResult.Message = resMsg;

                //响应信号
                waitResult.Set();
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
