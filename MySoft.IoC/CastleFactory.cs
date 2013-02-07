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
    public class CastleFactory : IServerConnect, ILogable, IErrorLogable
    {
        //线程同步锁；
        private static Hashtable hashtable = new Hashtable();
        private static CastleFactory singleton = null;

        #region Create Service Factory

        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private IDictionary<string, IService> proxies;
        private IDataCache cache;
        private SyncCaller caller;
        private IServiceLog logger;

        /// <summary>
        /// Gets the service container.
        /// </summary>
        /// <value>The service container.</value>
        public IContainer Container { get { return container; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CastleFactory"/> class.
        /// </summary>
        /// <param name="config">The container.</param>
        protected CastleFactory(CastleFactoryConfiguration config)
        {
            this.config = config;
            this.container = new SimpleServiceContainer(config.Type);

            InitCaller(this.cache);

            container.OnLog += (log, type) =>
            {
                if (this.OnLog != null) OnLog(log, type);
            };
            container.OnError += error =>
            {
                if (OnError != null) OnError(error);
                else SimpleLog.Instance.WriteLog(error);
            };

            this.proxies = new Dictionary<string, IService>();

            if (config.Nodes.Count > 0)
            {
                foreach (var p in config.Nodes)
                {
                    if (p.Value.MaxPool < 10) throw new WarningException("Minimum pool size 10.");
                    if (p.Value.MaxPool > 500) throw new WarningException("Maximum pool size 500.");

                    RemoteProxy proxy = null;
                    if (p.Value.RespType == ResponseType.Json)
                        proxy = new InvokeProxy(p.Value, container);
                    else
                        proxy = new RemoteProxy(p.Value, container);

                    proxy.OnConnected += (sender, args) =>
                    {
                        if (OnConnected != null) OnConnected(sender, args);
                    };
                    proxy.OnDisconnected += (sender, args) =>
                    {
                        if (OnDisconnected != null) OnDisconnected(sender, args);
                    };

                    this.proxies[p.Key.ToLower()] = proxy;
                }
            }

        }

        /// <summary>
        /// 初始化调用器
        /// </summary>
        /// <param name="cache"></param>
        private void InitCaller(IDataCache cache)
        {
            //实例化异步服务
            if (config.EnableCache)
                this.caller = new SyncCaller(config.MaxCaller, false, cache);
            else
                this.caller = new SyncCaller(config.MaxCaller, false);
        }

        #region 创建单例

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public static CastleFactory Create()
        {
            return CreateInternal(true);
        }

        /// <summary>
        /// 创建内部服务
        /// </summary>
        /// <param name="exists"></param>
        /// <returns></returns>
        internal static CastleFactory CreateInternal(bool exists)
        {
            if (singleton == null)
            {
                lock (hashtable.SyncRoot)
                {
                    if (singleton == null)
                    {
                        var config = CastleFactoryConfiguration.GetConfig();

                        if (config == null)
                        {
                            if (exists)
                                throw new WarningException("Not find configuration section castleFactory.");
                            else
                                config = new CastleFactoryConfiguration
                                {
                                    AppName = "InternalServer",
                                    Type = CastleFactoryType.Remote,
                                    ThrowError = true
                                };
                        }

                        singleton = new CastleFactory(config);
                    }
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
            return GetServerNode(config.DefaultKey);
        }

        /// <summary>
        /// 通过nodeKey查找节点
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        public ServerNode GetServerNode(string nodeKey)
        {
            if (proxies.Count == 0)
            {
                throw new WarningException("Not find any server node.");
            }

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
        public void RegisterCache(IDataCache cache)
        {
            this.cache = cache;

            InitCaller(cache);
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
            where IServiceInterfaceType : class
        {
            return GetChannel<IServiceInterfaceType>(config.DefaultKey);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="nodeKey">The node key.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(string nodeKey)
            where IServiceInterfaceType : class
        {
            //获取本地服务
            IService service = GetLocalService<IServiceInterfaceType>();

            ServerNode node = null;

            if (service == null)
            {
                var nodes = GetCacheServerNodes(nodeKey, typeof(IServiceInterfaceType).FullName);

                if (nodes.Count == 0)
                {
                    throw new WarningException(string.Format("Did not find the server node【{0}】.", nodeKey));
                }

                var index = new Random().Next(0, nodes.Count);
                node = nodes[index];
            }

            return GetChannel<IServiceInterfaceType>(service, node);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(ServerNode node)
            where IServiceInterfaceType : class
        {
            //获取本地服务
            IService service = GetLocalService<IServiceInterfaceType>();

            return GetChannel<IServiceInterfaceType>(service, node);
        }

        /// <summary>
        /// 获取通道服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <param name="service"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private IServiceInterfaceType GetChannel<IServiceInterfaceType>(IService service, ServerNode node)
            where IServiceInterfaceType : class
        {
            //是否缓存服务
            var isCacheService = true;

            if (service == null)
            {
                if (node == null)
                {
                    throw new WarningException("Server node can't for empty.");
                }

                if (proxies.ContainsKey(node.Key.ToLower()))
                {
                    service = proxies[node.Key.ToLower()];
                }
                else
                {
                    RemoteProxy proxy = null;
                    if (node.RespType == ResponseType.Json)
                        proxy = new InvokeProxy(node, container);
                    else
                        proxy = new RemoteProxy(node, container);

                    proxy.OnConnected += (sender, args) =>
                    {
                        if (OnConnected != null) OnConnected(sender, args);
                    };
                    proxy.OnDisconnected += (sender, args) =>
                    {
                        if (OnDisconnected != null) OnDisconnected(sender, args);
                    };

                    service = proxy;

                    if (config.AppName == "InternalServer")
                        isCacheService = true;
                    else
                        isCacheService = false;
                }
            }

            //返回通道服务
            return GetProxyChannel<IServiceInterfaceType>(service, isCacheService);
        }

        /// <summary>
        /// Create service channel.
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <param name="proxy"></param>
        /// <param name="isCacheService"></param>
        /// <returns></returns>
        private IServiceInterfaceType GetProxyChannel<IServiceInterfaceType>(IService proxy, bool isCacheService)
            where IServiceInterfaceType : class
        {
            Type serviceType = typeof(IServiceInterfaceType);
            string serviceKey = string.Format("{0}${1}", serviceType.FullName, proxy.ServiceName);

            lock (hashtable.SyncRoot)
            {
                var handler = new ServiceInvocationHandler<IServiceInterfaceType>(config, container, proxy, caller, logger);
                var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

                //不缓存，直接返回服务
                if (!isCacheService) return (IServiceInterfaceType)dynamicProxy;

                if (!hashtable.ContainsKey(serviceKey))
                {
                    hashtable[serviceKey] = dynamicProxy;
                }
            }

            return (IServiceInterfaceType)hashtable[serviceKey];
        }

        #endregion

        #region 回调订阅

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(object callback)
            where IPublishService : class
        {
            return GetChannel<IPublishService>(config.DefaultKey, callback);
        }

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="nodeKey"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(string nodeKey, object callback)
            where IPublishService : class
        {
            var nodes = GetCacheServerNodes(nodeKey, typeof(IPublishService).FullName);

            if (nodes.Count == 0)
            {
                throw new WarningException(string.Format("Did not find the server node【{0}】.", nodeKey));
            }

            var index = new Random().Next(0, nodes.Count);
            return GetChannel<IPublishService>(nodes[index], callback);
        }

        /// <summary>
        /// 获取回调发布服务
        /// </summary>
        /// <typeparam name="IPublishService"></typeparam>
        /// <param name="node"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IPublishService GetChannel<IPublishService>(ServerNode node, object callback)
            where IPublishService : class
        {
            if (node == null)
            {
                throw new WarningException("Server node can't for empty.");
            }

            if (callback == null) throw new IoCException("Callback cannot be the null.");
            var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(typeof(IPublishService));
            if (contract != null && contract.CallbackType != null)
            {
                if (!contract.CallbackType.IsAssignableFrom(callback.GetType()))
                {
                    throw new IoCException("Callback must assignable from " + callback.GetType().FullName + ".");
                }
            }
            else
                throw new IoCException("Callback type cannot be the null.");

            //实例化代理回调
            CallbackProxy proxy = new CallbackProxy(callback, node, container);

            proxy.OnConnected += (sender, args) =>
            {
                if (OnConnected != null) OnConnected(sender, args);
            };
            proxy.OnDisconnected += (sender, args) =>
            {
                if (OnDisconnected != null) OnDisconnected(sender, args);
            };

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
            return Invoke(config.DefaultKey, message);
        }

        /// <summary>
        /// 调用分布式服务
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData Invoke(string nodeKey, InvokeMessage message)
        {
            //获取本地服务
            IService service = GetLocalService(message.ServiceName);

            ServerNode node = null;

            if (service == null)
            {
                var nodes = GetCacheServerNodes(nodeKey, message.ServiceName);

                if (nodes.Count == 0)
                {
                    throw new WarningException(string.Format("Did not find the server node【{0}】.", nodeKey));
                }

                var index = new Random().Next(0, nodes.Count);
                node = nodes[index];
            }

            return Invoke(service, node, message);
        }

        /// <summary>
        /// 调用分布式服务
        /// </summary>
        /// <param name="node"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvokeData Invoke(ServerNode node, InvokeMessage message)
        {
            //获取本地服务
            IService service = GetLocalService(message.ServiceName);

            return Invoke(service, node, message);
        }

        /// <summary>
        /// 响应服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="node"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private InvokeData Invoke(IService service, ServerNode node, InvokeMessage message)
        {
            if (service == null)
            {
                if (node == null)
                {
                    throw new WarningException("Server node can't for empty.");
                }

                //获取服务节点
                if (proxies.ContainsKey(node.Key.ToLower()))
                {
                    service = proxies[node.Key.ToLower()];
                }
                else
                {
                    RemoteProxy proxy = null;
                    if (node.RespType == ResponseType.Json)
                        proxy = new InvokeProxy(node, container);
                    else
                        proxy = new RemoteProxy(node, container);

                    proxy.OnConnected += (sender, args) =>
                    {
                        if (OnConnected != null) OnConnected(sender, args);
                    };
                    proxy.OnDisconnected += (sender, args) =>
                    {
                        if (OnDisconnected != null) OnDisconnected(sender, args);
                    };

                    service = proxy;
                }
            }

            //获取服务内容
            return GetInvokeData(service, message);
        }

        #endregion

        #region Private Service

        /// <summary>
        /// 获取调用的数据
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private InvokeData GetInvokeData(IService service, InvokeMessage message)
        {
            //调用分布式服务
            using (var caller = new InvokeCaller(config, container, service, this.caller))
            {
                return caller.InvokeResponse(message);
            }
        }

        /// <summary>
        /// 获取本地服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        private IService GetLocalService<IServiceInterfaceType>()
            where IServiceInterfaceType : class
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
                var attr = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(serviceType);
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                if (!markedWithServiceContract)
                {
                    throw ex;
                }
            }

            //获取本地服务
            return GetLocalService(serviceType.FullName);
        }

        /// <summary>
        /// 获取本地服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private IService GetLocalService(string serviceName)
        {
            //定义本地服务
            IService service = default(IService);

            //如果是本地配置，则抛出异常
            if (config.Type != CastleFactoryType.Remote)
            {
                var serviceKey = "Service_" + serviceName;

                //本地服务
                if (container.Kernel.HasComponent(serviceKey))
                {
                    //返回本地服务
                    service = container.Resolve<IService>(serviceKey);
                }

                if (service == null)
                {
                    if (config.Type == CastleFactoryType.Local)
                    {
                        throw new WarningException(string.Format("The local not find matching service ({0}).", serviceName));
                    }
                }
            }

            return service;
        }

        /// <summary>
        /// 获取服务节点
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private IList<ServerNode> GetCacheServerNodes(string nodeKey, string serviceName)
        {
            lock (hashtable.SyncRoot)
            {
                //获取服务节点
                if (proxies.Count > 0)
                {
                    var node = GetServerNode(nodeKey);

                    if (node != null)
                    {
                        return new ServerNode[] { node };
                    }
                };

                if (!string.IsNullOrEmpty(config.ProxyServer))
                {
                    var cacheKey = string.Format("{0}${1}", config.ProxyServer, nodeKey);

                    //从缓存中获取节点
                    return CacheHelper<IList<ServerNode>>.Get(LocalCacheType.File, cacheKey, TimeSpan.FromMinutes(5)
                                                        , state =>
                                                        {
                                                            var arr = state as ArrayList;
                                                            var key = Convert.ToString(arr[0]);
                                                            var name = Convert.ToString(arr[1]);

                                                            //获取节点
                                                            return GetServerNodesFromChannel(key, name);

                                                        }, new ArrayList { nodeKey, serviceName });
                }

                //返回空列表
                return new ServerNode[0];
            }
        }

        /// <summary>
        /// 获取服务节点列表
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private IList<ServerNode> GetServerNodesFromChannel(string nodeKey, string serviceName)
        {
            var arr = config.ProxyServer.Split('|');

            var server = ServerNode.Parse(arr[0]);
            server.MinPool = 1;
            server.MaxPool = 1;
            server.Timeout = 30;

            if (arr.Length > 1)
            {
                server.Compress = Convert.ToBoolean(arr[1]);
            }

            if (server != null)
            {
                return GetChannel<IStatusService>(server).GetServerNodes(nodeKey, serviceName);
            }

            //返回空列表
            return new ServerNode[0];
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

        #region ITcpConnection 成员

        /// <summary>
        /// OnConnected event
        /// </summary>
        public event EventHandler<ConnectEventArgs> OnConnected;

        /// <summary>
        /// OnDisconnected event
        /// </summary>
        public event EventHandler<ConnectEventArgs> OnDisconnected;

        #endregion
    }
}
