using System;
using System.Net;
using System.Runtime.Remoting.Messaging;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// 操作上下文对象
    /// </summary>
    public class OperationContext
    {
        /// <summary>
        /// 当前上下文对象
        /// </summary>
        public static OperationContext Current
        {
            get
            {
                return CallContext.HostContext as OperationContext;
            }
            internal set
            {
                CallContext.HostContext = value;
            }
        }

        /// <summary>
        /// 回调类型
        /// </summary>
        private Type callbackType;
        private IScsServerClient client;
        private AppCaller caller;
        private bool isCached = true;
        private ICacheDependent cache;

        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool IsCached
        {
            get { return isCached; }
            set { isCached = true; }
        }

        /// <summary>
        /// 缓存依赖
        /// </summary>
        public ICacheDependent Cache
        {
            get { return cache; }
            set { cache = value; }
        }

        /// <summary>
        /// 调用者
        /// </summary>
        public AppCaller Caller
        {
            get { return caller; }
            internal set { caller = value; }
        }

        /// <summary>
        /// 远程节点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (client == null) return null;
                var endPoint = client.RemoteEndPoint as ScsTcpEndPoint;
                return new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort);
            }
        }

        internal OperationContext() { }

        internal OperationContext(IScsServerClient client, Type callbackType)
        {
            this.client = client;
            this.callbackType = callbackType;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(IScsMessage message)
        {
            this.client.SendMessage(message);
        }

        /// <summary>
        /// 获取回调代理服务
        /// </summary>
        /// <typeparam name="ICallbackService"></typeparam>
        /// <returns></returns>
        public ICallbackService GetCallbackChannel<ICallbackService>()
        {
            if (callbackType == null || typeof(ICallbackService) != callbackType)
            {
                throw new IoCException("Please set the current of callback interface type!");
            }
            else
            {
                var callback = new CallbackInvocationHandler(callbackType, client, 60);
                var instance = ProxyFactory.GetInstance().Create(callback, typeof(ICallbackService), true);
                return (ICallbackService)instance;
            }
        }
    }
}
