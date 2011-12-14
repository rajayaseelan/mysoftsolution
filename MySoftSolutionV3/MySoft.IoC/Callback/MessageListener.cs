using MySoft.IoC.Status;
using System.Net;
using System.Collections.Generic;
using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 消息监听
    /// </summary>
    internal class MessageListener
    {
        private DateTime _pushTime;
        /// <summary>
        /// 推送时间
        /// </summary>
        public DateTime PushTime
        {
            get { return _pushTime; }
        }

        private EndPoint _endPoint;
        /// <summary>
        /// 远程节点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return _endPoint; }
        }

        private SubscribeOptions _options;
        /// <summary>
        /// 订阅选项
        /// </summary>
        public SubscribeOptions Options
        {
            get { return _options; }
        }

        private IList<string> _subscribeTypes;
        /// <summary>
        /// 订阅的类型
        /// </summary>
        public IList<string> SubscribeTypes
        {
            get { return _subscribeTypes; }
        }

        private IStatusListener _innerListener;

        /// <summary>
        /// 初始化消息监听器
        /// </summary>
        /// <param name="endPoint"></param>
        public MessageListener(EndPoint endPoint, IStatusListener innerListener)
        {
            _endPoint = endPoint;
            _innerListener = innerListener;
            _pushTime = DateTime.Now;
        }

        /// <summary>
        /// 初始化消息监听器
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="innerListener"></param>
        /// <param name="options"></param>
        public MessageListener(EndPoint endPoint, IStatusListener innerListener, SubscribeOptions options, string[] subscribeTypes)
            : this(endPoint, innerListener)
        {
            _options = options;

            if (subscribeTypes == null)
                _subscribeTypes = new List<string>();
            else
                _subscribeTypes = new List<string>(subscribeTypes);
        }

        /// <summary>
        /// 推送客户端连接信息（只有第一次订阅的时候推送）
        /// </summary>
        /// <param name="clientInfos"></param>
        public void Notify(IList<ClientInfo> clientInfos)
        {
            _innerListener.Push(clientInfos);
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="connectInfo"></param>
        public void Notify(ConnectInfo connectInfo)
        {
            _innerListener.Push(connectInfo);
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="appClient"></param>
        public void Notify(string ipAddress, int port, AppClient appClient)
        {
            _innerListener.Change(ipAddress, port, appClient);
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="status"></param>
        public void Notify(ServerStatus status)
        {
            _pushTime = DateTime.Now;
            _innerListener.Push(status);
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="callError"></param>
        public void Notify(CallError callError)
        {
            _innerListener.Push(callError);
        }

        /// <summary>
        /// 超时信息
        /// </summary>
        /// <param name="callTimeout"></param>
        public void Notify(CallTimeout callTimeout)
        {
            _innerListener.Push(callTimeout);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool eq = base.Equals(obj);
            if (!eq)
            {
                MessageListener lstn = obj as MessageListener;
                var endPoint = lstn._endPoint as IPEndPoint;
                var currEndPoint = this._endPoint as IPEndPoint;

                if (endPoint.Address.Equals(currEndPoint.Address)
                    && endPoint.Port == currEndPoint.Port)
                {
                    eq = true;
                }
            }
            return eq;
        }
    }
}
