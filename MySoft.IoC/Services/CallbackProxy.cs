using MySoft.IoC.Messages;
using MySoft.Logger;
using System;
using System.Linq;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 回调代理
    /// </summary>
    public class CallbackProxy : RemoteProxy
    {
        private object callback;

        public CallbackProxy(object callback, ServerNode node, ILog logger)
            : base(node, logger, true)
        {
            this.callback = callback;
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void MessageCallback(object sender, CallbackMessageEventArgs e)
        {
            if (callback != null)
            {
                var callbackType = callback.GetType();

                //获取接口的类型
                var interfaces = callbackType.GetInterfaces();
                if (interfaces.Length > 0)
                {
                    //判断类型是否相同
                    if (interfaces.Any(type => type.FullName == e.Message.ServiceName))
                    {
                        try
                        {
                            var method = CoreHelper.GetMethodFromType(callbackType, e.Message.MethodName);

                            //执行委托
                            method.FastInvoke(callback, e.Message.Parameters);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public override string ServiceName
        {
            get
            {
                return string.Format("{0}${1}", typeof(CallbackProxy).FullName, Guid.NewGuid());
            }
        }
    }
}
