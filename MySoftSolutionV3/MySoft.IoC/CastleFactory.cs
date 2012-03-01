using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.IoC.Aspect;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
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
        private DiscoverProxy discoverProxy;
        private IDictionary<string, IService> proxies = new Dictionary<string, IService>();

        /// <summary>
        /// 所有代理信息
        /// </summary>
        internal IList<RemoteProxy> Proxies
        {
            get
            {
                return proxies.Values.Cast<RemoteProxy>().ToList();
            }
        }

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
            this.discoverProxy = new DiscoverProxy(this, container);
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
                var sContainer = new SimpleServiceContainer(config.Type);
                sContainer.OnLog += (log, type) =>
                {
                    if (instance != null && instance.OnLog != null)
                        instance.OnLog(log, type);
                };
                sContainer.OnError += error =>
                {
                    if (instance != null && instance.OnError != null)
                        instance.OnError(error);
                };

                instance = new CastleFactory(config, sContainer);

                if (config.Nodes.Count > 0)
                {
                    foreach (var node in config.Nodes)
                    {
                        if (node.Value.MaxPool < 1) throw new WarningException("Minimum pool size 1！");
                        if (node.Value.MaxPool > 1000) throw new WarningException("Maximum pool size 1000！");

                        var proxy = new RemoteProxy(node.Value, sContainer);
                        instance.proxies[node.Key.ToLower()] = proxy;
                    }
                }
            }

            return instance;
        }

        #endregion

        #endregion

        #region Get Service

        /// <summary>
        /// 获取默认的节点
        /// </summary>
        /// <returns></returns>
        public RemoteNode GetDefaultNode()
        {
            return GetRemoteNodes().FirstOrDefault(p => string.Compare(p.Key, config.Default, true) == 0);
        }

        /// <summary>
        /// 通过nodeKey查找节点
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        public RemoteNode GetRemoteNode(string nodeKey)
        {
            return GetRemoteNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
        }

        /// <summary>
        /// 获取所有远程节点
        /// </summary>
        /// <returns></returns>
        public IList<RemoteNode> GetRemoteNodes()
        {
            return this.Proxies.Select(p => p.Node).ToList();
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <returns>The service implemetation instance.</returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>()
        {
            var service = GetLocalService<IServiceInterfaceType>();
            if (service != null) return service;

            return GetChannel<IServiceInterfaceType>(discoverProxy, true);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="nodeKey">The node key.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(string nodeKey)
        {
            nodeKey = GetNodeKey(nodeKey);
            var node = GetRemoteNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
            if (node == null)
            {
                throw new WarningException(string.Format("Did not find the node {0}!", nodeKey));
            }

            return GetChannel<IServiceInterfaceType>(node);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(RemoteNode node)
        {
            var service = GetLocalService<IServiceInterfaceType>();
            if (service != null) return service;

            IService proxy = null;
            var isCacheService = true;
            if (singleton.proxies.ContainsKey(node.Key.ToLower()))
                proxy = singleton.proxies[node.Key.ToLower()];
            else
            {
                proxy = new RemoteProxy(node, container);
                isCacheService = false;
            }

            return GetChannel<IServiceInterfaceType>(proxy, isCacheService);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <returns></returns>
        private IServiceInterfaceType GetChannel<IServiceInterfaceType>(IService proxy, bool isCacheService)
        {
            Type serviceType = typeof(IServiceInterfaceType);
            string serviceKey = string.Format("CastleFactory_{0}_{1}", proxy.ServiceName, serviceType.FullName);
            var service = CacheHelper.Get<IServiceInterfaceType>(serviceKey);

            if (service == null)
            {
                lock (lockObject)
                {
                    IProxyInvocationHandler handler = null;
                    if (config.Json)
                        handler = new JsonInvocationHandler(this.config, this.container, proxy, serviceType);
                    else
                        handler = new ServiceInvocationHandler(this.config, this.container, proxy, serviceType);
                    var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

                    service = (IServiceInterfaceType)dynamicProxy;
                    if (isCacheService) CacheHelper.Permanent(serviceKey, service);
                }
            }

            return service;
        }

        #endregion

        #region ILogable Members

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;

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
        /// <param name="nodeKey"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(string nodeKey, object callback)
        {
            nodeKey = GetNodeKey(nodeKey);
            var node = GetRemoteNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
            if (node == null)
            {
                throw new WarningException(string.Format("Did not find the node {0}!", nodeKey));
            }
            return GetChannel<IPublishService>(node, callback);
        }

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="node"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(RemoteNode node, object callback)
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
            return GetChannel<IPublishService>(proxy, false);
        }

        #endregion

        #region Invoke 方式调用

        /// <summary>
        /// 调用分布式服务
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData Invoke(InvokeMessage message)
        {
            IService service = GetLocalService(message);
            if (service == null)
            {
                //本地服务为null，则使用发现服务调用
                service = discoverProxy;
            }

            return GetInvokeData(message, service);
        }

        /// <summary>
        /// 调用分布式服务
        /// </summary>
        /// <param name="message"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData Invoke(string nodeKey, InvokeMessage message)
        {
            nodeKey = GetNodeKey(nodeKey);
            var node = GetRemoteNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
            if (node == null)
            {
                throw new WarningException(string.Format("Did not find the node {0}!", nodeKey));
            }

            return Invoke(node, message);
        }

        public InvokeData Invoke(RemoteNode node, InvokeMessage message)
        {
            IService service = GetLocalService(message);
            if (service == null)
            {
                if (singleton.proxies.ContainsKey(node.Key.ToLower()))
                    service = singleton.proxies[node.Key.ToLower()];
                else
                {
                    service = new RemoteProxy(node, container);
                }
            }

            return GetInvokeData(message, service);
        }

        #region Private Service

        /// <summary>
        /// 获取本地服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        private IServiceInterfaceType GetLocalService<IServiceInterfaceType>()
        {
            Type serviceType = typeof(IServiceInterfaceType);

            //定义异常
            Exception ex = new ArgumentException("Generic parameter type - 【" + serviceType.FullName
                   + "】 must be an interface marked with ServiceContractAttribute.");

            if (!serviceType.IsInterface)
            {
                throw ex;
            }
            else
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(serviceType);
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                if (!markedWithServiceContract)
                {
                    throw ex;
                }
            }

            //如果是本地配置，则抛出异常
            if (config.Type == CastleFactoryType.Local || config.Type == CastleFactoryType.LocalRemote)
            {
                //本地服务
                if (container.Kernel.HasComponent(serviceType))
                {
                    lock (lockObject)
                    {
                        //返回本地服务
                        var handler = new LocalInvocationHandler(config, container, serviceType);
                        var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

                        return (IServiceInterfaceType)dynamicProxy;
                    }
                }

                if (config.Type == CastleFactoryType.Local)
                    throw new WarningException(string.Format("Local not find service ({0}).", serviceType.FullName));
            }

            return default(IServiceInterfaceType);
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
            if (string.IsNullOrEmpty(nodeKey) || !singleton.proxies.ContainsKey(nodeKey.ToLower()))
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

        /// <summary>
        /// 获取调用的数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        private InvokeData GetInvokeData(InvokeMessage message, IService service)
        {
            //调用分布式服务
            var appClient = new AppClient
            {
                AppName = config.AppName,
                HostName = DnsHelper.GetHostName(),
                IPAddress = DnsHelper.GetIPAddress()
            };

            var caller = new InvokeCaller(appClient, service);
            return caller.CallMethod(message);
        }

        /// <summary>
        /// 获取本地服务
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private IService GetLocalService(InvokeMessage message)
        {
            IService service = null;
            string serviceKey = "Service_" + message.ServiceName;
            if (config.Type == CastleFactoryType.Local || config.Type == CastleFactoryType.LocalRemote)
            {
                if (container.Kernel.HasComponent(serviceKey))
                {
                    service = container.Resolve<IService>(serviceKey);
                }

                if (service == null && config.Type == CastleFactoryType.Local)
                    throw new WarningException(string.Format("Local not find service ({0}).", message.ServiceName));
            }

            return service;
        }

        #endregion

        #endregion
    }
}
