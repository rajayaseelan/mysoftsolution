using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService, IServerConnect
    {
        #region IServerConnect 成员

        /// <summary>
        /// 连接服务器
        /// </summary>
        public event EventHandler<ConnectEventArgs> OnConnected;

        /// <summary>
        /// 断开服务器
        /// </summary>
        public event EventHandler<ConnectEventArgs> OnDisconnected;

        #endregion

        /// <summary>
        /// 结果队列
        /// </summary>
        private IDictionary<string, WaitResult> hashtable = new Dictionary<string, WaitResult>();

        protected ServiceRequestPool reqPool;
        private volatile int poolSize;
        private ILog logger;
        private ServerNode node;

        /// <summary>
        /// 实例化RemoteProxy
        /// </summary>
        /// <param name="node"></param>
        /// <param name="container"></param>
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
                    this.reqPool.Push(CreateServiceRequest(false));
                }
            }
        }

        /// <summary>
        /// 创建一个服务请求项
        /// </summary>
        /// <param name="subscribed"></param>
        /// <returns></returns>
        protected ServiceRequest CreateServiceRequest(bool subscribed)
        {
            var reqService = new ServiceRequest(node, subscribed);
            reqService.OnCallback += OnMessageCallback;
            reqService.OnError += OnMessageError;
            reqService.OnConnected += (sender, args) =>
            {
                if (OnConnected != null) OnConnected(sender, args);
            };

            reqService.OnDisconnected += (sender, args) =>
            {
                if (OnDisconnected != null) OnDisconnected(sender, args);
            };

            return reqService;
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMessageCallback(object sender, ServiceMessageEventArgs e)
        {
            if (e.Result is ResponseMessage)
            {
                var resMsg = e.Result as ResponseMessage;

                QueueMessage(e.MessageId, resMsg);
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMessageError(object sender, ErrorMessageEventArgs e)
        {
            if (e.Request != null)
            {
                var resMsg = IoCHelper.GetResponse(e.Request, e.Error);

                QueueMessage(e.MessageId, resMsg);
            }
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="resMsg"></param>
        private void QueueMessage(string messageId, ResponseMessage resMsg)
        {
            lock (hashtable)
            {
                if (hashtable.ContainsKey(messageId))
                {
                    var waitResult = hashtable[messageId];

                    //数据响应
                    waitResult.Set(resMsg);
                }
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
            var reqProxy = GetServiceRequest();

            //消息Id
            var messageId = reqMsg.TransactionId.ToString();

            try
            {
                //处理数据
                using (var waitResult = new WaitResult(reqMsg))
                {
                    lock (hashtable)
                    {
                        hashtable[messageId] = waitResult;
                    }

                    //发送消息
                    reqProxy.SendRequest(messageId, reqMsg);

                    //等待信号响应
                    if (!waitResult.WaitOne(TimeSpan.FromSeconds(node.Timeout)))
                    {
                        return GetTimeoutResponse(reqMsg);
                    }

                    return waitResult.Message;
                }
            }
            finally
            {
                lock (hashtable)
                {
                    //用完后移除
                    hashtable.Remove(messageId);
                }

                //加入队列
                reqPool.Push(reqProxy);
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg)
        {
            var title = string.Format("【{0}:{1}】 => Call remote service ({2}, {3}) timeout ({4}) ms.\r\nParameters => {5}"
               , node.IP, node.Port, reqMsg.ServiceName, reqMsg.MethodName, node.Timeout * 1000, reqMsg.Parameters.ToString());

            //获取异常
            return IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
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
                                reqPool.Push(CreateServiceRequest(false));
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
        public ServerNode Node { get { return node; } }

        /// <summary>
        /// 服务名称
        /// </summary>
        public virtual string ServiceName
        {
            get
            {
                return string.Format("{0}${1}", typeof(RemoteProxy).FullName, node);
            }
        }

        #endregion
    }
}