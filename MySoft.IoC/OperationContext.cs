using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

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
                string name = string.Format("OperationContext_{0}", Thread.CurrentThread.ManagedThreadId);
                return CallContext.GetData(name) as OperationContext;
            }
            set
            {
                string name = string.Format("OperationContext_{0}", Thread.CurrentThread.ManagedThreadId);
                CallContext.SetData(name, value);
            }
        }

        /// <summary>
        /// 回调类型
        /// </summary>
        private Type callbackType;
        private IScsServerClient client;
        private IContainer container;
        private AppCaller caller;

        /// <summary>
        /// 容器对象
        /// </summary>
        public IContainer Container
        {
            get { return container; }
            internal set { container = value; }
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
        /// 远程客户端
        /// </summary>
        public IScsServerClient ServerClient
        {
            get { return client; }
        }

        internal OperationContext() { }

        internal OperationContext(IScsServerClient client, Type callbackType)
        {
            this.client = client;
            this.callbackType = callbackType;
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
                throw new IoCException("Please set the current of callback interface type.");
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
