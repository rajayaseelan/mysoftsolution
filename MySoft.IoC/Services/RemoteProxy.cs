using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.Logger;

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

        private ServiceRequestPool pool;
        private ServerNode node;
        private ILog logger;

        /// <summary>
        /// 结果队列
        /// </summary>
        private readonly IDictionary<string, WaitResult> hashtable;

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

            if (subscribed)
            {
                this.pool = new ServiceRequestPool(1);

                //加入队列
                pool.Push(new ServiceRequest(this, node, true));
            }
            else
            {
                this.pool = new ServiceRequestPool(ServiceConfig.DEFAULT_CLIENT_MAXPOOL);

                //最大池为100
                for (int i = 0; i < ServiceConfig.DEFAULT_CLIENT_MAXPOOL; i++)
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
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        public virtual void MessageCallback(string messageId, CallbackMessage message)
        {
            return;
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        public void MessageCallback(string messageId, ResponseMessage message)
        {
            lock (hashtable)
            {
                //设置响应值
                if (hashtable.ContainsKey(messageId))
                {
                    hashtable[messageId].Set(message);
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
            var reqProxy = pool.Pop();

            if (reqProxy == null)
            {
                throw new WarningException("Service request exceeds the maximum number of pool.");
            }

            try
            {
                //发送请求
                return GetResponse(reqProxy, reqMsg);
            }
            finally
            {
                pool.Push(reqProxy);
            }
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="reqProxy"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(ServiceRequest reqProxy, RequestMessage reqMsg)
        {
            var messageId = reqMsg.TransactionId.ToString();

            //发送消息并获取结果
            using (var waitResult = new WaitResult())
            {
                try
                {
                    lock (hashtable)
                    {
                        //请求列表
                        hashtable[messageId] = waitResult;
                    }

                    //发送消息
                    reqProxy.SendMessage(messageId, reqMsg);

                    //等待信号响应
                    if (!waitResult.WaitOne(TimeSpan.FromSeconds(node.Timeout)))
                    {
                        return GetTimeoutResponse(reqMsg);
                    }

                    //返回响应的消息
                    return waitResult.Message;
                }
                finally
                {
                    lock (hashtable)
                    {
                        //移除列表
                        hashtable.Remove(messageId);
                    }
                }
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