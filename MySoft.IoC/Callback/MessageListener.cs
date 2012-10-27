using System;
using System.Collections.Generic;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Callback
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

        private IScsServerClient _channel;
        /// <summary>
        /// 远程客户端
        /// </summary>
        public IScsServerClient Channel
        {
            get { return _channel; }
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
        public IList<string> Types
        {
            get { return _subscribeTypes; }
        }

        private IList<string> _appNames;
        /// <summary>
        /// 订阅的应用
        /// </summary>
        public IList<string> Apps
        {
            get { return _appNames; }
        }

        private IStatusListener _innerListener;

        /// <summary>
        /// 初始化消息监听器
        /// </summary>
        /// <param name="channel"></param>
        public MessageListener(IScsServerClient channel, IStatusListener innerListener)
        {
            this._channel = channel;
            this._innerListener = innerListener;
            this._pushTime = DateTime.Now;
            this._appNames = new List<string>();
            this._appNames = new List<string>();
        }

        /// <summary>
        /// 初始化消息监听器
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="innerListener"></param>
        /// <param name="options"></param>
        public MessageListener(IScsServerClient channel, IStatusListener innerListener, SubscribeOptions options, string[] subscribeTypes)
            : this(channel, innerListener)
        {
            this._options = options;

            if (subscribeTypes == null)
                this._subscribeTypes = new List<string>();
            else
                this._subscribeTypes = new List<string>(subscribeTypes);
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
    }
}
