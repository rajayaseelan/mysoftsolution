using System;
using System.Collections;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService
    {
        protected ILog logger;
        protected ServerNode node;
        protected ServiceRequestPool reqPool;
        private volatile int poolSize;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化RemoteProxy
        /// </summary>
        /// <param name="node"></param>
        /// <param name="logger"></param>
        public RemoteProxy(ServerNode node, ILog logger)
        {
            this.node = node;
            this.logger = logger;

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
            var reqService = new ServiceRequest(node, logger, true);
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
                var resMsg = new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = reqMsg.ReturnType,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
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
                var waitResult = hashtable[resMsg.TransactionId] as WaitResult;

                //数据响应
                waitResult.Set(resMsg);
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
                    hashtable[reqMsg.TransactionId] = waitResult;

                    //获取一个请求
                    reqProxy = GetServiceRequest();

                    //发送消息
                    reqProxy.SendMessage(reqMsg);

                    //等待信号响应
                    var elapsedTime = TimeSpan.FromSeconds(node.Timeout);

                    if (!waitResult.Wait(elapsedTime))
                    {
                        throw new WarningException(string.Format("【{0}:{1}】 => Call remote service ({2}, {3}) timeout ({4}) ms.\r\nParameters => {5}"
                           , node.IP, node.Port, reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds, reqMsg.Parameters.ToString()));
                    }

                    return waitResult.Message;
                }
            }
            finally
            {
                //加入队列
                if (reqProxy != null)
                {
                    reqPool.Push(reqProxy);
                }

                //用完后移除
                hashtable.Remove(reqMsg.TransactionId);
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
                        //一次性创建5个请求池
                        for (int i = 0; i < 5; i++)
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