using MySoft.IoC.Status;
using System.Net;
using System.Collections.Generic;
using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 消息监听
    /// </summary>
    class MessageListener
    {
        private EndPoint _endPoint;
        /// <summary>
        /// 远程节点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return _endPoint; }
        }

        private SubscibeOptions _options;
        /// <summary>
        /// 订阅选项
        /// </summary>
        public SubscibeOptions Options
        {
            get { return _options; }
        }

        private Type[] _subscibeTypes;
        /// <summary>
        /// 订阅的类型
        /// </summary>
        public Type[] SubscibeTypes
        {
            get { return _subscibeTypes; }
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
        }

        /// <summary>
        /// 初始化消息监听器
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="innerListener"></param>
        /// <param name="options"></param>
        public MessageListener(EndPoint endPoint, IStatusListener innerListener, SubscibeOptions options, Type[] subscibeTypes)
            : this(endPoint, innerListener)
        {
            _options = options;

            if (subscibeTypes == null)
                _subscibeTypes = new Type[0];
            else
                _subscibeTypes = subscibeTypes;
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="connected"></param>
        public void Notify(EndPoint endPoint, bool connected)
        {
            _innerListener.Push(endPoint, connected);
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="endPoint"></param>
        public void Notify(EndPoint endPoint, AppClient appClient)
        {
            _innerListener.Push(endPoint, appClient);
        }

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="status"></param>
        public void Notify(ServerStatus status)
        {
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
