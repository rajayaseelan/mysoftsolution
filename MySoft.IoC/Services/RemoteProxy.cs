using System;
using System.Collections.Generic;
using MySoft.IoC.Communication;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService
    {
        private IDictionary<Guid, WaitResult> hashtable = new Dictionary<Guid, WaitResult>();

        protected ServiceRequestPool reqPool;
        private volatile int poolSize;
        protected IServiceContainer container;
        protected ServerNode node;

        /// <summary>
        /// 实例化RemoteProxy
        /// </summary>
        /// <param name="node"></param>
        /// <param name="container"></param>
        public RemoteProxy(ServerNode node, IServiceContainer container)
        {
            this.node = node;
            this.container = container;

            InitServiceRequest();
        }

        /// <summary>
        /// 初始化请求
        /// </summary>
        protected virtual void InitServiceRequest()
        {
            this.reqPool = new ServiceRequestPool(node.MaxPool);

            lock (this.reqPool)
            {
                this.poolSize = node.MinPool;

                //服务请求池化，使用最小的池初始化
                for (int i = 0; i < node.MinPool; i++)
                {
                    this.reqPool.Push(CreateServiceRequest());
                }
            }
        }

        /// <summary>
        /// 创建一个服务请求项
        /// </summary>
        /// <returns></returns>
        private ServiceRequest CreateServiceRequest()
        {
            var reqService = new ServiceRequest(node, container, false);
            reqService.OnCallback += reqService_OnCallback;
            reqService.OnError += reqService_OnError;

            return reqService;
        }

        void reqService_OnError(object sender, ErrorMessageEventArgs e)
        {
            QueueError(e.Request, e.Error);
        }

        protected void QueueError(RequestMessage reqMsg, Exception error)
        {
            if (reqMsg != null)
            {
                var resMsg = IoCHelper.GetResponse(reqMsg, error);

                QueueMessage(resMsg);
            }
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="resMsg"></param>
        protected void QueueMessage(ResponseMessage resMsg)
        {
            lock (hashtable)
            {
                if (hashtable.ContainsKey(resMsg.TransactionId))
                {
                    var waitResult = hashtable[resMsg.TransactionId];

                    //数据响应
                    waitResult.Set(resMsg);
                }
            }
        }

        void reqService_OnCallback(object sender, ServiceMessageEventArgs args)
        {
            if (args.Result is ResponseMessage)
            {
                var resMsg = args.Result as ResponseMessage;

                QueueMessage(resMsg);
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public virtual ResponseMessage CallService(RequestMessage reqMsg)
        {
            //获取一个请求
            ServiceRequest reqProxy = null;

            try
            {
                //处理数据
                using (var waitResult = new WaitResult(reqMsg))
                {
                    lock (hashtable)
                    {
                        hashtable[reqMsg.TransactionId] = waitResult;
                    }

                    //获取一个请求
                    reqProxy = GetServiceRequest();

                    //发送消息
                    reqProxy.SendMessage(reqMsg);

                    //定义响应消息
                    ResponseMessage resMsg = null;

                    var elapsedTime = TimeSpan.FromSeconds(node.Timeout);

                    //等待信号响应
                    if (!waitResult.Wait(elapsedTime))
                    {
                        var title = string.Format("【{0}:{1}】 => Call remote service ({2}, {3}) timeout ({4}) ms.\r\nParameters => {5}"
                           , node.IP, node.Port, reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString());

                        //获取异常
                        resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
                    }
                    else
                    {
                        resMsg = waitResult.Message;
                    }

                    return resMsg;
                }
            }
            finally
            {
                //加入队列
                if (reqProxy != null)
                {
                    reqPool.Push(reqProxy);
                }

                lock (hashtable)
                {
                    //用完后移除
                    hashtable.Remove(reqMsg.TransactionId);
                }
            }
        }

        /// <summary>
        /// 获取一个服务请求
        /// </summary>
        /// <returns></returns>
        protected virtual ServiceRequest GetServiceRequest()
        {
            var reqProxy = reqPool.Pop();

            if (reqProxy == null)
            {
                if (poolSize < node.MaxPool)
                {
                    lock (reqPool)
                    {
                        //一次性创建10个请求池
                        for (int i = 0; i < 10; i++)
                        {
                            if (poolSize < node.MaxPool)
                            {
                                poolSize++;

                                //创建一个新的请求
                                reqPool.Push(CreateServiceRequest());
                            }
                        }

                        //增加后再从池里弹出一个
                        reqProxy = reqPool.Pop();
                    }
                }
                else
                {
                    throw new WarningException(string.Format("Service request pool beyond the {0} limit.", node.MaxPool));
                }
            }

            return reqProxy;
        }

        #region IService 成员

        /// <summary>
        /// 远程节点
        /// </summary>
        public ServerNode Node
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
    }
}