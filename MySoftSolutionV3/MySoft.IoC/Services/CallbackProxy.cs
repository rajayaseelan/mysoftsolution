using System;
using System.Linq;
using System.Reflection;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Threading;
using MySoft.Communication.Scs.Communication;
using System.Net.Sockets;
using MySoft.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 回调代理
    /// </summary>
    public class CallbackProxy : RemoteProxy
    {
        /// <summary>
        /// 异常处理
        /// </summary>
        public event ErrorLogEventHandler OnError;

        private object callback;
        public CallbackProxy(object callback, RemoteNode node, ILog logger)
            : base(node, logger)
        {
            this.callback = callback;
        }

        protected override void InitRequest()
        {
            ServiceRequest reqService = new ServiceRequest(node, logger);
            reqService.OnCallback += new EventHandler<ServiceMessageEventArgs>(reqService_OnCallback);
            reqService.OnDisconnected += new EventHandler(reqService_OnDisconnected);

            this.reqPool = new ServiceRequestPool(1);
            this.reqPool.Push(reqService);
        }

        void reqService_OnDisconnected(object sender, EventArgs e)
        {
            if (OnError != null) OnError(new SocketException((int)SocketError.ConnectionReset));
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
                        var method = callbackType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(p => p.ToString() == resMsg.MethodName);

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
                return typeof(CallbackProxy).FullName;
            }
        }
    }
}
