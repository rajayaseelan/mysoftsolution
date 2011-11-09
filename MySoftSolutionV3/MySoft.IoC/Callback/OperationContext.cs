using System;
using System.Runtime.Remoting.Messaging;
using MySoft.Communication.Scs.Communication.EndPoints;
using MySoft.Communication.Scs.Server;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;
using MySoft.Communication.Scs.Communication.Messages;

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
            set
            {
                CallContext.HostContext = value;
            }
        }

        /// <summary>
        /// 回调类型
        /// </summary>
        private Type callbackType;
        private IScsServerClient client;
        /// <summary>
        /// 客户端节点
        /// </summary>
        public IScsServerClient ServerClient
        {
            get { return client; }
        }

        /// <summary>
        /// 远程节点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                var endPoint = client.RemoteEndPoint as ScsTcpEndPoint;
                return new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort);
            }
        }

        internal OperationContext(IScsServerClient client)
        {
            this.client = client;
        }

        internal OperationContext(IScsServerClient client, Type callbackType)
            : this(client)
        {
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
                var callback = new CallbackInvocationHandler(callbackType, client);
                var instance = ProxyFactory.GetInstance().Create(callback, typeof(ICallbackService), true);
                return (ICallbackService)instance;
            }
        }
    }
}
