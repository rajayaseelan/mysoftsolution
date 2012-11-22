using System;
using System.Linq;
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
                this.reqPool.Push(CreateServiceRequest(true));
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

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnMessageCallback(object sender, ServiceMessageEventArgs args)
        {
            if (args.Result is CallbackMessage)
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
                            try
                            {
                                var method = CoreHelper.GetMethodFromType(callbackType, callbackMsg.MethodName);

                                //执行委托
                                method.FastInvoke(callback, callbackMsg.Parameters);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }
            else
            {
                //调用基类处理
                base.OnMessageCallback(sender, args);
            }
        }

        public override string ServiceName
        {
            get
            {
                return string.Format("{0}${1}", typeof(CallbackProxy).FullName, Guid.NewGuid());
            }
        }
    }
}
