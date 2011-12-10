using System;
using System.Net.Sockets;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 回调代理
    /// </summary>
    public class CallbackProxy : RemoteProxy, IServerConnection
    {
        /// <summary>
        /// This event is raised when client connected to server.
        /// </summary>
        public event EventHandler OnConnected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        public event EventHandler OnDisconnected;

        private object callback;
        public CallbackProxy(object callback, RemoteNode node, ILog logger)
            : base(node, logger)
        {
            this.callback = callback;
        }

        /// <summary>
        /// 初始化请求
        /// </summary>
        protected override void InitRequest()
        {
            ServiceRequest reqService = new ServiceRequest(node, logger, false);
            reqService.OnCallback += new EventHandler<ServiceMessageEventArgs>(reqService_OnCallback);
            reqService.OnConnected += OnConnected;
            reqService.OnDisconnected += OnDisconnected;

            this.reqPool = new ServiceRequestPool(1);
            this.reqPool.Push(reqService);
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void reqService_OnCallback(object sender, ServiceMessageEventArgs args)
        {
            if (args.Result is ResponseMessage)
            {
                base.QueueMessage(args.Result as ResponseMessage);
            }
            else if (args.Result is CallbackMessage)
            {
                var resMsg = args.Result as CallbackMessage;

                if (callback != null)
                {
                    var callbackType = callback.GetType();
                    if (resMsg.ServiceType.IsAssignableFrom(callbackType))
                    {
                        var method = CoreHelper.GetMethodFromType(callbackType, resMsg.MethodName);

                        //执行委托
                        DynamicCalls.GetMethodInvoker(method).Invoke(callback, resMsg.Parameters);
                    }
                }
            }

            args = null;
        }

        public override string ServiceName
        {
            get
            {
                return string.Format("{0}_{1}", typeof(CallbackProxy).FullName, Guid.NewGuid());
            }
        }
    }
}
