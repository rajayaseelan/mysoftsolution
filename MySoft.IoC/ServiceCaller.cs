using Amib.Threading;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    internal class ServiceCaller : IDisposable
    {
        private readonly IServiceContainer container;
        private readonly CastleServiceConfiguration config;
        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, AsyncCaller> asyncCallers;
        private SmartThreadPool smart;
        private Semaphore semaphore;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        public ServiceCaller(CastleServiceConfiguration config, IServiceContainer container)
        {
            this.config = config;
            this.container = container;

            this.callbackTypes = new Dictionary<string, Type>();
            this.asyncCallers = new Dictionary<string, AsyncCaller>();
            this.semaphore = new Semaphore(config.MaxCaller, config.MaxCaller);

            var stp = new STPStartInfo
            {
                ThreadPoolName = "ServiceCaller",
                ThreadPriority = ThreadPriority.Highest,
                IdleTimeout = 1000,
                MaxWorkerThreads = Math.Max(30, config.MaxCaller),
                MinWorkerThreads = 10,
                StartSuspended = true
            };

            this.smart = new SmartThreadPool(stp);
            this.smart.Start();

            //初始化服务
            InitTypes(container, config);

            //初始化调用器
            var services = container.Kernel.ResolveAll<IService>();
            var timeout = TimeSpan.FromSeconds(config.Timeout);

            InitCaller(services, timeout);
        }

        private void InitTypes(IServiceContainer container, CastleServiceConfiguration config)
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
        /// 初始化调用器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="timeout"></param>
        private void InitCaller(IService[] services, TimeSpan timeout)
        {
            foreach (var service in services)
            {
                var caller = new AsyncCaller(smart, service, timeout);
                asyncCallers[service.ServiceName] = caller;
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="appCaller"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage HandleResponse(IScsServerClient channel, AppCaller appCaller, RequestMessage reqMsg)
        {
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                //解析服务
                var caller = GetAsyncCaller(appCaller);

                //获取上下文
                var context = GetOperationContext(channel, appCaller);

                return caller.Invoke(context, reqMsg);
            }
            catch (Exception ex)
            {
                //获取异常响应信息
                return IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private OperationContext GetOperationContext(IScsServerClient channel, AppCaller appCaller)
        {
            //实例化当前上下文
            Type callbackType = null;

            if (callbackTypes.ContainsKey(appCaller.ServiceName))
            {
                callbackType = callbackTypes[appCaller.ServiceName];
            }

            return new OperationContext(channel, callbackType)
            {
                Container = container,
                Caller = appCaller
            };
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="appCaller"></param>
        /// <returns></returns>
        private AsyncCaller GetAsyncCaller(AppCaller appCaller)
        {
            //判断服务是否存在
            if (asyncCallers.ContainsKey(appCaller.ServiceName))
            {
                return asyncCallers[appCaller.ServiceName];
            }
            else
            {
                string body = string.Format("The server not find matching service ({0}).", appCaller.ServiceName);

                //获取异常
                throw IoCHelper.GetException(appCaller, body);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            try
            {
                asyncCallers.Clear();
                callbackTypes.Clear();

                smart.Cancel(true);
                smart.Shutdown();
            }
            catch (Exception ex) { }
            finally
            {
                smart.Dispose();
            }
        }

        #endregion
    }
}
