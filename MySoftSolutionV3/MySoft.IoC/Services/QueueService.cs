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
    public class QueueService : IService
    {
        private IService service;
        private ILog logger;
        private TimeSpan elapsedTime;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化QueueService
        /// </summary>
        /// <param name="service"></param>
        /// <param name="logger"></param>
        /// <param name="elapsedTime"></param>
        public QueueService(IService service, ILog logger, TimeSpan elapsedTime)
        {
            this.service = service;
            this.logger = logger;
            this.elapsedTime = elapsedTime;
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //实例化等待对象
            var waitResult = new QueueResult(reqMsg);

            //参数信息
            var context = OperationContext.Current;
            var jsonString = context.Caller.Parameters;
            var queueKey = ServiceConfig.FormatJson(jsonString);

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(queueKey))
                {
                    var queue = new Queue<QueueResult>();
                    hashtable[queueKey] = queue;

                    //加入队列中
                    lock (queue)
                    {
                        queue.Enqueue(waitResult);
                    }

                    //异步调用
                    ThreadPool.QueueUserWorkItem(GetResponse, new ArrayList { context, reqMsg, queueKey });
                }
                else
                {
                    var queue = hashtable[queueKey] as Queue<QueueResult>;

                    //加入队列中
                    lock (queue)
                    {
                        queue.Enqueue(waitResult);
                    }
                }
            }

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
            var arr = state as ArrayList;

            var context = arr[0] as OperationContext;
            var reqMsg = arr[1] as RequestMessage;
            var queueKey = arr[2] as string;

            //设置上下文
            OperationContext.Current = context;

            //调用方法
            var resMsg = service.CallService(reqMsg);

            if (hashtable.ContainsKey(queueKey))
            {
                var queue = hashtable[queueKey] as Queue<QueueResult>;
                hashtable.Remove(queueKey);

                //处理队列消息
                if (queue.Count > 1)
                {
                    Console.WriteLine("Queue Length: {0}\t=> Queue Key: {1}", queue.Count, queueKey);
                }

                //响应消息
                SetResponse(resMsg, queue);
            }
        }

        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="queue"></param>
        private void SetResponse(ResponseMessage resMsg, Queue<QueueResult> queue)
        {
            //响应数据
            while (queue.Count > 0)
            {
                var waitResult = queue.Dequeue();

                //响应信号
                waitResult.Set(resMsg);
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
