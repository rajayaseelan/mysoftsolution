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
        /// 实例化RemoteProxy
        /// </summary>
        /// <param name="node"></param>
        /// <param name="logger"></param>
        /// <param name="subscribed"></param>
        public RemoteProxy(ServerNode node, ILog logger, bool subscribed)
        {
            this.node = node;
            this.logger = logger;

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
            //TODO
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
                //消息Id
                var messageId = Guid.NewGuid().ToString();

                //发送消息并获取结果
                using (var waitResult = new WaitResult(reqMsg))
                {
                    //同步调用
                    reqProxy.InvokeMessage(messageId, waitResult);

                    //等待信号响应
                    if (!waitResult.WaitOne(TimeSpan.FromSeconds(node.Timeout)))
                    {
                        return GetTimeoutResponse(reqMsg);
                    }

                    //返回响应的消息
                    return waitResult.Message;
                }
            }
            finally
            {
                pool.Push(reqProxy);
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