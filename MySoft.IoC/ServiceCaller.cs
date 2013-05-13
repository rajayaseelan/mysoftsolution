using System;
using System.Collections.Generic;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    internal class ServiceCaller : IDisposable
    {
        private IDictionary<string, Type> callbackTypes;
        private IServiceContainer container;
        private AsyncCaller caller;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="caller"></param>
        public ServiceCaller(CastleServiceConfiguration config, IServiceContainer container, AsyncCaller caller)
        {
            this.callbackTypes = new Dictionary<string, Type>();
            this.container = container;
            this.caller = caller;

            //初始化服务
            Init(container, config);
        }

        private void Init(IServiceContainer container, CastleServiceConfiguration config)
        {
            callbackTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);
            var types = container.GetServiceTypes<ServiceContractAttribute>();

            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public void InvokeResponse(IScsServerClient channel, IDataContext e)
        {
            //获取上下文
            using (var context = GetOperationContext(channel, e.Caller))
            {
                //解析服务
                var service = ParseService(e.Caller);

                //异步调用服务
                var item = caller.Run(service, context, e.Request);

                //设置响应值
                e.Buffer = item.Buffer;
                e.Count = item.Count;
                e.Message = item.Message;
            }
        }

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private OperationContext GetOperationContext(IScsServerClient channel, AppCaller caller)
        {
            //实例化当前上下文
            Type callbackType = null;

            lock (callbackTypes)
            {
                if (callbackTypes.ContainsKey(caller.ServiceName))
                {
                    callbackType = callbackTypes[caller.ServiceName];
                }
            }

            return new OperationContext(channel, callbackType)
            {
                Container = container,
                Caller = caller
            };
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private IService ParseService(AppCaller caller)
        {
            string serviceKey = "Service_" + caller.ServiceName;

            //判断服务是否存在
            if (container.Kernel.HasComponent(serviceKey))
            {
                return container.Resolve<IService>(serviceKey);
            }
            else
            {
                string body = string.Format("The server not find matching service ({0}).", caller.ServiceName);

                //获取异常
                throw IoCHelper.GetException(caller, body);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            callbackTypes.Clear();
            callbackTypes = null;
        }

        #endregion
    }
}
