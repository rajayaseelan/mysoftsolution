using MySoft.IoC.Messages;
using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class RemoteProxy : IService, IServerConnect, IServiceCallback
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

        private IDictionary<string, WaitResult> hashtable;
        private Semaphore semaphore;
        private ServiceRequestPool pool;
        private ServerNode node;
        private ILog logger;

        /// <summary>
        /// 实例化RemoteProxy
        /// </summary>
        /// <param name="node"></param>
        /// <param name="logger"></param>
        /// <param name="subscribed"></param>
        public RemoteProxy(ServerNode node, ILog logger, bool subscribed)
        {
            this.node = node;
            this.logger = logger;
            this.hashtable = new Dictionary<string, WaitResult>();
            this.semaphore = new Semaphore(node.MaxPool, node.MaxPool);

            if (subscribed)
            {
                this.pool = new ServiceRequestPool(1);

                //加入队列
                pool.Push(new ServiceRequest(this, node, true));
            }
            else
            {
                this.pool = new ServiceRequestPool(node.MaxPool);

                //最大池为100
                for (int i = 0; i < node.MaxPool; i++)
                {
                    //加入队列
                    pool.Push(new ServiceRequest(this, node, false));
                }
            }
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void Connected(object sender, ConnectEventArgs e)
        {
            if (OnConnected != null) OnConnected(sender, e);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void Disconnected(object sender, ConnectEventArgs e)
        {
            if (OnDisconnected != null) OnDisconnected(sender, e);
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void MessageCallback(object sender, CallbackMessageEventArgs e)
        {
            //TODO
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void MessageCallback(object sender, ResponseMessageEventArgs e)
        {
            lock (hashtable)
            {
                if (hashtable.ContainsKey(e.MessageId))
                {
                    hashtable[e.MessageId].Set(e.Message);
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
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                var reqProxy = pool.Pop();

                if (reqProxy == null)
                {
                    throw new WarningException("Service request exceeds the maximum number of pool.");
                }

                try
                {
                    //消息Id
                    var messageId = Guid.NewGuid().ToString();

                    //获取响应信息
                    return GetResponse(reqProxy, messageId, reqMsg);
                }
                finally
                {
                    pool.Push(reqProxy);
                }
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="reqProxy"></param>
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(ServiceRequest reqProxy, string messageId, RequestMessage reqMsg)
        {
            using (var waitResult = new WaitResult())
            {
                //添加信号量对象
                lock (hashtable) hashtable[messageId] = waitResult;

                try
                {
                    //发送请求
                    reqProxy.SendRequest(messageId, reqMsg);

                    //等待信号响应
                    if (!waitResult.WaitOne(TimeSpan.FromSeconds(node.Timeout)))
                    {
                        throw GetTimeoutException(reqMsg);
                    }

                    //返回响应的消息
                    return waitResult.Message;
                }
                finally
                {
                    //移除信号量对象
                    lock (hashtable) hashtable.Remove(messageId);
                }
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private Exception GetTimeoutException(RequestMessage reqMsg)
        {
            var title = string.Format("【{0}:{1}】 => Call remote service ({2}, {3}) timeout ({4}) ms.\r\nParameters => {5}"
               , node.IP, node.Port, reqMsg.ServiceName, reqMsg.MethodName, node.Timeout * 1000, reqMsg.Parameters.ToString());

            //获取异常
            return new TimeoutException(title);
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