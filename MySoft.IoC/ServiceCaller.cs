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
    internal class ServiceCaller
    {
        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, AsyncCaller> asyncCallers;
        private IServiceContainer container;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        public ServiceCaller(CastleServiceConfiguration config, IServiceContainer container)
        {
            this.container = container;
            this.callbackTypes = new Dictionary<string, Type>();
            this.asyncCallers = new Dictionary<string, AsyncCaller>();

            //初始化服务
            Init(container, config);
        }

        private void Init(IServiceContainer container, CastleServiceConfiguration config)
        {
            callbackTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);

            var types = container.GetServiceTypes<ServiceContractAttribute>();
            var timeout = TimeSpan.FromSeconds(config.Timeout);

            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }

                IService service = null;
                string serviceKey = "Service_" + type.FullName;

                if (container.Kernel.HasComponent(serviceKey))
                {
                    service = container.Resolve<IService>(serviceKey);

                    //实例化AsyncCaller
                    if (config.EnableCache)
                        asyncCallers[type.FullName] = new AsyncCaller(service, timeout, null, true);
                    else
                        asyncCallers[type.FullName] = new AsyncCaller(service, timeout, true);
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool InvokeResponse(IScsServerClient channel, CallerContext e)
        {
            //获取上下文
            using (var context = GetOperationContext(channel, e.Caller))
            {
                try
                {
                    //解析服务
                    var asyncCaller = GetAsyncCaller(e.Caller);

                    //异步调用服务
                    e.Message = asyncCaller.Run(context, e.Request);
                }
                catch (Exception ex)
                {
                    //获取异常响应
                    e.Message = IoCHelper.GetResponse(e.Request, ex);
                }

                return e.Message != null;
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
        /// Gets the asyncCaller.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private AsyncCaller GetAsyncCaller(AppCaller caller)
        {
            lock (asyncCallers)
            {
                if (!asyncCallers.ContainsKey(caller.ServiceName))
                {
                    string body = string.Format("The server【{1}({2})】not find matching service ({0})."
                        , caller.ServiceName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                    //获取异常
                    throw IoCHelper.GetException(caller, body);
                }

                return asyncCallers[caller.ServiceName];
            }
        }
    }
}
