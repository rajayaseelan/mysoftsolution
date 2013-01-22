using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 操作上下文对象
    /// </summary>
    public class OperationContext : IDisposable
    {
        /// <summary>
        /// 当前上下文对象
        /// </summary>
        public static OperationContext Current
        {
            get
            {
                var name = typeof(OperationContext).FullName;
                return CallContext.GetData(name) as OperationContext;
            }
            set
            {
                var name = typeof(OperationContext).FullName;
                if (value == null)
                    CallContext.FreeNamedDataSlot(name);
                else
                    CallContext.SetData(name, value);
            }
        }

        /// <summary>
        /// 回调类型
        /// </summary>
        private Type callbackType;
        private IScsServerClient channel;
        private IContainer container;
        private AppCaller caller;
        private static object syncRoot = new object();

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
        public IScsServerClient Channel
        {
            get { return channel; }
        }

        internal OperationContext() { }

        internal OperationContext(IScsServerClient channel, Type callbackType)
        {
            this.channel = channel;
            this.callbackType = callbackType;
        }

        /// <summary>
        /// 获取回调代理服务
        /// </summary>
        /// <typeparam name="ICallbackService"></typeparam>
        /// <returns></returns>
        public ICallbackService GetCallbackChannel<ICallbackService>()
            where ICallbackService : class
        {
            if (channel == null)
            {
                return default(ICallbackService);
            }

            if (callbackType == null || typeof(ICallbackService) != callbackType || !typeof(ICallbackService).IsInterface)
            {
                throw new WarningException("Please set the current of callback interface type.");
            }
            else
            {
                lock (syncRoot)
                {
                    var handler = new CallbackInvocationHandler(callbackType, channel);
                    var dynamicProxy = ProxyFactory.GetInstance().Create(handler, typeof(ICallbackService), true);

                    return (ICallbackService)dynamicProxy;
                }
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            this.channel = null;
            this.container = null;
            this.caller = null;
        }

        #endregion
    }
}
