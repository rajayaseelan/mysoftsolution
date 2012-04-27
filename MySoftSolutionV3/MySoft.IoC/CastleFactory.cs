using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySoft.Cache;
using MySoft.IoC.Configuration;
using MySoft.IoC.Logger;
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
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
        private static CastleFactory singleton = null;

        #region Create Service Factory

        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private IDictionary<string, IService> proxies;
        private ICacheStrategy cache;
        private IServiceLog logger;

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
        /// <param name="config">The container.</param>
        protected CastleFactory(CastleFactoryConfiguration config)
        {
            this.config = config;
            this.container = new SimpleServiceContainer(config.Type);

            container.OnLog += (log, type) =>
            {
                if (this.OnLog != null) this.OnLog(log, type);
            };
            container.OnError += error =>
            {
                if (this.OnError != null) this.OnError(error);
            };

            this.proxies = new Dictionary<string, IService>();

            if (config.Nodes.Count > 0)
            {
                foreach (var p in config.Nodes)
                {
                    if (p.Value.MaxPool < 10) throw new WarningException("Minimum pool size 10！");
                    if (p.Value.MaxPool > 500) throw new WarningException("Maximum pool size 500！");

                    IService proxy = null;
                    if (p.Value.Format == TransferType.Json)
                        proxy = new InvokeProxy(p.Value, container);
                    else
                        proxy = new RemoteProxy(p.Value, container);

                    this.proxies[p.Key.ToLower()] = proxy;
                }
            }
        }

        #region 创建单例

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public static CastleFactory Create()
        {
            lock (hashtable.SyncRoot)
            {
                if (singleton == null)
                {
                    var config = CastleFactoryConfiguration.GetConfig();

                    if (config == null)
                        throw new WarningException("Not find configuration section castleFactory！");

                    singleton = new CastleFactory(config);
                }
            }

            return singleton;
        }

        #endregion

        #endregion

        #region Get Service

        /// <summary>
        /// 获取默认的节点
        /// </summary>
        /// <returns></returns>
        public ServerNode GetDefaultNode()
        {
            return GetServerNodes().FirstOrDefault(p => string.Compare(p.Key, config.Default, true) == 0);
        }

        /// <summary>
        /// 通过nodeKey查找节点
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        public ServerNode GetServerNode(string nodeKey)
        {
            return GetServerNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
        }

        /// <summary>
        /// 获取所有远程节点
        /// </summary>
        /// <returns></returns>
        public IList<ServerNode> GetServerNodes()
        {
            return proxies.Values.Cast<RemoteProxy>().Select(p => p.Node).ToList();
        }

        /// <summary>
        /// 注册缓存
        /// </summary>
        /// <param name="cache"></param>
        public void RegisterCache(ICacheStrategy cache)
        {
            //设置区域名称为应用名称
            cache.SetRegionName(config.AppName);

            this.cache = cache;
        }

        /// <summary>
        /// 注册日志依赖
        /// </summary>
        /// <param name="logger"></param>
        public void RegisterLogger(IServiceLog logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <returns>The service implemetation instance.</returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>()
        {
            if (proxies.Count == 0)
            {
                //获取本地服务
                var service = GetLocalService<IServiceInterfaceType>();
                if (service != null) return service;

                throw new WarningException(string.Format("Did not find the service {0}!", typeof(IServiceInterfaceType).FullName));
            }

            return GetChannel<IServiceInterfaceType>(config.Default);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="nodeKey">The node key.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(string nodeKey)
        {
            nodeKey = GetNodeKey(nodeKey);
            var node = GetServerNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
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
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(ServerNode node)
        {
            if (node == null)
                throw new WarningException("Server node can't for empty!");

            //获取本地服务
            var service = GetLocalService<IServiceInterfaceType>();

            if (service == null)
            {
                IService proxy = null;
                var isCacheService = true;
                if (singleton.proxies.ContainsKey(node.Key.ToLower()))
                    proxy = singleton.proxies[node.Key.ToLower()];
                else
                {
                    if (node.Format == TransferType.Json)
                        proxy = new InvokeProxy(node, container);
                    else
                        proxy = new RemoteProxy(node, container);

                    isCacheService = false;
                }

                service = GetProxyChannel<IServiceInterfaceType>(proxy, isCacheService);
            }

            //返回服务
            return service;
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <param name="proxy"></param>
        /// <param name="isCacheService"></param>
        /// <returns></returns>
        private IServiceInterfaceType GetProxyChannel<IServiceInterfaceType>(IService proxy, bool isCacheService)
        {
            Type serviceType = typeof(IServiceInterfaceType);
            string serviceKey = string.Format("{0}${1}", serviceType.FullName, proxy.ServiceName);

            lock (hashtable.SyncRoot)
            {
                var handler = new ServiceInvocationHandler(this.config, this.container, proxy, serviceType, cache, logger);
                var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

                if (!isCacheService) //不缓存，直接返回服务
                    return (IServiceInterfaceType)dynamicProxy;

                if (!hashtable.ContainsKey(serviceKey))
                {
                    hashtable[serviceKey] = dynamicProxy;
                }
            }

            return (IServiceInterfaceType)hashtable[serviceKey];
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
            var node = GetServerNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
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
        public IPublishService GetChannel<IPublishService>(ServerNode node, object callback)
        {
            if (node == null)
                throw new WarningException("Server node can't for empty!");

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
            return GetProxyChannel<IPublishService>(proxy, false);
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
            if (proxies.Count == 0)
            {
                //获取本地服务
                IService service = GetLocalService(message);
                if (service != null) return GetInvokeData(message, service);

                throw new WarningException(string.Format("Did not find the service {0}!", message.ServiceName));
            }

            return Invoke(config.Default, message);
        }

        /// <summary>
        /// 调用分布式服务
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData Invoke(string nodeKey, InvokeMessage message)
        {
            nodeKey = GetNodeKey(nodeKey);
            var node = GetServerNodes().FirstOrDefault(p => string.Compare(p.Key, nodeKey, true) == 0);
            if (node == null)
            {
                throw new WarningException(string.Format("Did not find the node {0}!", nodeKey));
            }

            return Invoke(node, message);
        }

        /// <summary>
        /// 调用分布式服务
        /// </summary>
        /// <param name="node"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData Invoke(ServerNode node, InvokeMessage message)
        {
            if (node == null)
                throw new WarningException("Server node can't for empty!");

            //获取本地服务
            IService service = GetLocalService(message);

            if (service == null)
            {
                if (singleton.proxies.ContainsKey(node.Key.ToLower()))
                    service = singleton.proxies[node.Key.ToLower()];
                else
                {
                    if (node.Format == TransferType.Json)
                        service = new InvokeProxy(node, container);
                    else
                        service = new RemoteProxy(node, container);
                }
            }

            //获取服务内容
            return GetInvokeData(message, service);
        }

        #endregion

        #region Private Service

        /// <summary>
        /// 获取调用的数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        private InvokeData GetInvokeData(InvokeMessage message, IService service)
        {
            //调用分布式服务
            var caller = new InvokeCaller(config.AppName, service);
            return caller.CallMethod(message);
        }

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
            if (config.Type != CastleFactoryType.Remote)
            {
                //本地服务
                if (container.Kernel.HasComponent(serviceType))
                {
                    lock (hashtable.SyncRoot)
                    {
                        if (!hashtable.ContainsKey(serviceType))
                        {
                            //返回本地服务
                            var service = container.Resolve<IService>("Service_" + serviceType.FullName);
                            var handler = new LocalInvocationHandler(config, container, service, serviceType, cache, logger);
                            var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

                            hashtable[serviceType] = dynamicProxy;
                        }
                    }

                    return (IServiceInterfaceType)hashtable[serviceType];
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

            //如果不存在当前配置节，则使用默认配置节
            if (string.IsNullOrEmpty(nodeKey) || !singleton.proxies.ContainsKey(nodeKey.ToLower()))
            {
                nodeKey = config.Default;
            }

            return nodeKey;
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
            if (config.Type != CastleFactoryType.Remote)
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
    }
}
