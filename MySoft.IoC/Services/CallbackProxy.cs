using System;
using System.Linq;
using System.Net.Sockets;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 回调代理
    /// </summary>
    public class CallbackProxy : RemoteProxy
    {
        private object callback;

        public CallbackProxy(object callback, ServerNode node, ILog logger)
            : base(node, logger)
        {
            this.callback = callback;
        }

        /// <summary>
        /// 初始化请求项
        /// </summary>
        protected override void InitServiceRequest()
        {
            this.reqPool = new ServiceRequestPool(1);

            lock (this.reqPool)
            {
                var reqService = new ServiceRequest(node, logger, false);
                reqService.OnCallback += reqService_OnCallback;
                reqService.OnError += reqService_OnError;
                reqService.Disconnected += reqService_Disconnected;

                this.reqPool.Push(reqService);
            }
        }

        /// <summary>
        /// 获取一个服务请求
        /// </summary>
        /// <returns></returns>
        protected override ServiceRequest GetServiceRequest()
        {
            if (reqPool.Count > 0)
                return reqPool.Pop();
            else
                throw new WarningException(string.Format("Service request pool beyond the {0} limit.", 1));
        }

        void reqService_OnError(object sender, ErrorMessageEventArgs e)
        {
            base.QueueError(e.Request, e.Error);
        }

        void reqService_Disconnected(object sender, EventArgs e)
        {
            this.logger.WriteError(new SocketException((int)SocketError.NotConnected));
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
                var resMsg = args.Result as ResponseMessage;
                base.QueueMessage(resMsg);
            }
            else if (args.Result is CallbackMessage)
            {
                var callbackMsg = args.Result as CallbackMessage;

                if (callback != null)
                {
                    var callbackType = callback.GetType();

                    //获取接口的类型
                    var interfaces = callbackType.GetInterfaces();
                    if (interfaces.Length > 0)
                    {
                        //判断类型是否相同
                        if (interfaces.Any(type => type.FullName == callbackMsg.ServiceName))
                        {
                            var method = CoreHelper.GetMethodFromType(callbackType, callbackMsg.MethodName);

                            //执行委托
                            DynamicCalls.GetMethodInvoker(method).Invoke(callback, callbackMsg.Parameters);
                        }
                    }
                }
            }
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
