using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.Cache;
using MySoft.IoC.Aspect;
using MySoft.IoC.Configuration;
using MySoft.IoC.Services;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// The service factory.
    /// </summary>
    public class CastleFactory : ILogable, IErrorLogable
    {
        private static object lockObject = new object();
        private static CastleFactory singleton = null;

        #region Create Service Factory

        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private IDictionary<string, IService> proxies = new Dictionary<string, IService>();

        /// <summary>
        /// Gets the service container.
        /// </summary>
        /// <value>The service container.</value>
        public IServiceContainer ServiceContainer
        {
            get
            {
                return container;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CastleFactory"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        protected CastleFactory(CastleFactoryConfiguration config, IServiceContainer container)
        {
            if (config == null) config = new CastleFactoryConfiguration();
            this.config = config;
            this.container = container;
        }

        #region 创建单例

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public static CastleFactory Create()
        {
            if (singleton == null)
            {
                lock (lockObject)
                {
                    if (singleton == null)
                    {
                        var config = CastleFactoryConfiguration.GetConfig();
                        singleton = CreateNew(config);
                    }
                }
            }

            return singleton;
        }

        /// <summary>
        /// Creates this instance. Used in a multithreaded environment
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name">service name</param>
        /// <returns>The service factoru new instance.</returns>
        private static CastleFactory CreateNew(CastleFactoryConfiguration config)
        {
            CastleFactory instance = null;

            //本地匹配节
            if (config == null || config.Type == CastleFactoryType.Local)
            {
                instance = new CastleFactory(config, new SimpleServiceContainer(CastleFactoryType.Local));
            }
            else
            {
                IServiceContainer container = new SimpleServiceContainer(config.Type);
                container.OnLog += (log, type) => { if (instance.OnLog != null) instance.OnLog(log, type); };
                container.OnError += (exception) => { if (instance.OnError != null) instance.OnError(exception); };

                instance = new CastleFactory(config, container);

                if (config.Nodes.Count > 0)
                {
                    foreach (var node in config.Nodes)
                    {
                        if (node.Value.MaxPool < 1) throw new WarningException("Minimum pool size 1！");
                        if (node.Value.MaxPool > 500) throw new WarningException("Maximum pool size 500！");

                        var proxy = new RemoteProxy(node.Value, container);
                        instance.proxies[node.Key.ToLower()] = proxy;
                    }
                }
            }

            return instance;
        }

        #endregion

        #endregion

        #region 注入缓存

        /// <summary>
        /// 注册缓存依赖
        /// </summary>
        /// <param name="cache"></param>
        public void RegisterCacheDependent(ICacheDependent cache)
        {
            this.container.Cache = cache;
        }

        #endregion

        #region Get Service

        /// <summary>
        /// Gets local the service.
        /// </summary>
        /// <returns>The service implemetation instance.</returns>
        public IServiceInterfaceType ResolveService<IServiceInterfaceType>()
        {
            //本地服务
            if (container.Kernel.HasComponent(typeof(IServiceInterfaceType)))
            {
                lock (lockObject)
                {
                    var service = container[typeof(IServiceInterfaceType)];

                    //返回拦截服务
                    return AspectManager.GetService<IServiceInterfaceType>(service);
                }
            }

            return default(IServiceInterfaceType);
        }

        /// <summary>
        /// 获取所有远程节点
        /// </summary>
        /// <returns></returns>
        public RemoteNode[] GetRemoteNodes()
        {
            return singleton.proxies.Select(p => p.Value)
                            .Cast<RemoteProxy>().Select(p => p.Node)
                            .ToArray();
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <returns>The service implemetation instance.</returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>()
        {
            return GetChannel<IServiceInterfaceType>(config.Default);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="nodeKey">The node key.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(string nodeKey)
        {
            return GetChannel<IServiceInterfaceType>(nodeKey, null);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(RemoteNode node)
        {
            return GetChannel<IServiceInterfaceType>(node, null);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <returns></returns>
        private IServiceInterfaceType GetChannel<IServiceInterfaceType>(RemoteNode node, RemoteProxy proxy)
        {
            if (!singleton.config.Nodes.ContainsKey(node.Key))
                singleton.config.Nodes[node.Key] = node;

            return GetChannel<IServiceInterfaceType>(node.Key, proxy);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="nodeKey">The node key.</param>
        /// <returns></returns>
        private IServiceInterfaceType GetChannel<IServiceInterfaceType>(string nodeKey, RemoteProxy proxy)
        {
            Exception ex = new ArgumentException("Generic parameter type - 【" + typeof(IServiceInterfaceType).FullName
                + "】 must be an interface marked with ServiceContractAttribute.");

            if (!typeof(IServiceInterfaceType).IsInterface)
            {
                throw ex;
            }
            else
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(typeof(IServiceInterfaceType));
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                attr = null;

                if (!markedWithServiceContract)
                {
                    throw ex;
                }
            }

            //如果是本地配置，则抛出异常
            if (config.Type == CastleFactoryType.Local)
            {
                //本地服务
                if (container.Kernel.HasComponent(typeof(IServiceInterfaceType)))
                {
                    lock (lockObject)
                    {
                        var service = container[typeof(IServiceInterfaceType)];

                        //返回拦截服务
                        return AspectManager.GetService<IServiceInterfaceType>(service);
                    }
                }

                throw new WarningException(string.Format("Local not find service ({0}).", typeof(IServiceInterfaceType).FullName));
            }
            else
            {
                Type serviceType = typeof(IServiceInterfaceType);
                string serviceKey = string.Format("CastleFactory_{0}_{1}", nodeKey, serviceType);
                IServiceInterfaceType iocService = CacheHelper.Get<IServiceInterfaceType>(serviceKey);
                if (iocService == null)
                {
                    lock (lockObject)
                    {
                        IService service = container;
                        object instance = null;
                        try { instance = container[serviceType]; }
                        catch { }
                        if (!container.Kernel.HasComponent(serviceType) || instance == null)
                        {
                            if (proxy == null)
                            {
                                nodeKey = GetNodeKey(nodeKey);
                                service = singleton.proxies[nodeKey.ToLower()];
                            }
                            else
                            {
                                service = proxy;
                            }
                        }

                        var handler = new ServiceInvocationHandler(this.config, this.container, service, serviceType);
                        var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

                        iocService = (IServiceInterfaceType)dynamicProxy;
                        CacheHelper.Permanent(serviceKey, iocService);

                        handler = null;
                        dynamicProxy = null;
                    }
                }

                return iocService;
            }
        }

        /// <summary>
        /// 获取节点名称
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        private string GetNodeKey(string nodeKey)
        {
            if (singleton.proxies.Count == 0)
            {
                throw new WarningException("Not find any service node！");
            }

            if (string.IsNullOrEmpty(nodeKey)) nodeKey = config.Default;
            string oldNodeKey = nodeKey;

            //如果不存在当前配置节，则使用默认配置节
            if (!singleton.proxies.ContainsKey(nodeKey.ToLower()))
            {
                nodeKey = "default";
            }

            if (!singleton.proxies.ContainsKey(nodeKey.ToLower()))
            {
                if (oldNodeKey == nodeKey)
                    throw new WarningException("Not find the service node [" + nodeKey + "]！");
                else
                    throw new WarningException("Not find the service node [" + oldNodeKey + "] or [" + nodeKey + "]！");
            }
            return nodeKey;
        }

        #endregion

        #region ILogable Members

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;

        #endregion

        #region IErrorLogable Members

        /// <summary>
        /// OnError event.
        /// </summary>
        public event ErrorLogEventHandler OnError;

        #endregion

        #region 回调订阅

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="callback"></param>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(object callback)
        {
            return GetChannel<IPublishService>(callback, config.Default);
        }

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="callback"></param>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(object callback, string nodeKey)
        {
            nodeKey = GetNodeKey(nodeKey);
            var node = GetRemoteNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
            return GetChannel<IPublishService>(callback, node);
        }

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="callback"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(object callback, RemoteNode node)
        {
            if (callback == null) throw new IoCException("Callback cannot be the null!");
            var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(typeof(IPublishService));
            if (contract != null && contract.CallbackType != null)
            {
                if (!contract.CallbackType.IsAssignableFrom(callback.GetType()))
                {
                    throw new IoCException("Callback must assignable from " + callback.GetType().FullName + "!");
                }
            }
            else
                throw new IoCException("Callback type cannot be the null!");

            CallbackProxy proxy = new CallbackProxy(callback, node, this.ServiceContainer);
            proxy.OnError += new ErrorLogEventHandler(proxy_OnError);
            return GetChannel<IPublishService>(node, proxy);
        }

        void proxy_OnError(Exception exception)
        {
            if (singleton.OnError != null)
                singleton.OnError(exception);
        }

        #endregion
    }
}
