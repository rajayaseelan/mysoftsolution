using System;
using System.Collections;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Collections.Generic;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService, IDisposable
    {
        protected ILog logger;
        protected RemoteNode node;
        protected ServiceRequestPool reqPool;
        private IDictionary<Guid, WaitResult> hashtable = new Dictionary<Guid, WaitResult>();

        public RemoteProxy(RemoteNode node, ILog logger)
        {
            this.node = node;
            this.logger = logger;

            InitRequest();
        }

        /// <summary>
        /// 初始化请求
        /// </summary>
        protected virtual void InitRequest()
        {
            this.reqPool = new ServiceRequestPool(node.MaxPool);

            lock (this.reqPool)
            {
                //服务请求池化
                for (int i = 0; i < node.MaxPool; i++)
                {
                    var reqService = new ServiceRequest(node, logger, true);
                    reqService.OnCallback += reqService_OnCallback;
                    reqService.OnError += reqService_OnError;

                    this.reqPool.Push(reqService);
                }
            }
        }

        void reqService_OnError(object sender, ErrorMessageEventArgs e)
        {
            QueueError(e.Request, e.Error);
        }

        protected void QueueError(RequestMessage request, Exception error)
        {
            if (request != null)
            {
                var resMsg = new ResponseMessage
                {
                    TransactionId = request.TransactionId,
                    ServiceName = request.ServiceName,
                    MethodName = request.Message,
                    ReturnType = request.ReturnType,
                    Error = error
                };

                QueueMessage(resMsg);
            }
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="resMsg"></param>
        protected void QueueMessage(ResponseMessage resMsg)
        {
            if (hashtable.ContainsKey(resMsg.TransactionId))
            {
                var value = hashtable[resMsg.TransactionId];
                value.Message = resMsg;

                //数据响应
                value.Reset.Set();
            }
        }

        void reqService_OnCallback(object sender, ServiceMessageEventArgs args)
        {
            if (args.Result is ResponseMessage)
            {
                var resMsg = args.Result as ResponseMessage;
                QueueMessage(resMsg);
            }

            args = null;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //如果池为空，则判断是否达到最大池
            ServiceRequest reqProxy = null;

            //堆栈为空时，抛出异常
            if (this.reqPool.Count == 0)
            {
                throw new WarningException("Proxy service pool is null or empty！");
            }

            //获取一个服务请求
            reqProxy = reqPool.Pop();

            try
            {
                //发送消息
                reqProxy.SendMessage(reqMsg);

                //处理数据
                var autoEvent = new AutoResetEvent(false);
                lock (hashtable)
                {
                    hashtable[reqMsg.TransactionId] = new WaitResult { Reset = autoEvent };
                }

                var elapsedTime = TimeSpan.FromSeconds(node.Timeout);
                ResponseMessage resMsg = null;

                //启动线程来
                if (autoEvent.WaitOne(elapsedTime))
                {
                    var value = hashtable[reqMsg.TransactionId];
                    resMsg = value.Message;
                }

                //用完后移除
                lock (hashtable)
                {
                    hashtable.Remove(reqMsg.TransactionId);
                }

                return resMsg;
            }
            finally
            {
                this.reqPool.Push(reqProxy);
            }
        }

        #region IService 成员

        /// <summary>
        /// 远程节点
        /// </summary>
        public RemoteNode Node
        {
            get { return node; }
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public virtual string ServiceName
        {
            get
            {
                return string.Format("{0}_{1}", typeof(RemoteProxy).FullName, node.Key);
            }
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                while (reqPool.Count > 0)
                {
                    var reqService = reqPool.Pop();
                    reqService.Dispose();
                }
            }
            catch (Exception)
            {
            }

            this.reqPool = null;
        }
    }
}