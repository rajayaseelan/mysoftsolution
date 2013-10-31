using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Nodes;
using MySoft.IoC.Services;
using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC
{
    /// <summary>
    /// The service factory.
    /// </summary>
    public class CastleFactory : IServerConnect, ILogable, IErrorLogable
    {
        private static IDictionary<string, object> hashtable = new Dictionary<string, object>();
        private static readonly object syncRoot = new object();
        private static CastleFactory singleton = null;

        #region Create Service Factory

        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private IDictionary<string, IService> proxies;
        private IServiceCall call;
        private IServerNodeResolver nodeResolver;

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
            this.container = new SimpleServiceContainer(config.ServerType);
            this.proxies = new Dictionary<string, IService>();

            if (config.ServerType == CastleFactoryType.Proxy)
            {
                //判断是否配置了NodeResolverType
                this.nodeResolver = Create<IServerNodeResolver>(config.ResolverType) ?? new DefaultNodeResolver();
            }

            container.OnLog += (log, type) =>
            {
                if (this.OnLog != null) OnLog(log, type);
            };

            container.OnError += error =>
            {
                if (OnError != null) OnError(error);
                else SimpleLog.Instance.WriteLog(error);
            };
        }

        /// <summary>
        /// 创建指定类型的实例
        /// </summary>
        /// <typeparam name="InterfaceType"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        private InterfaceType Create<InterfaceType>(Type type)
            where InterfaceType : class
        {
            try
            {
                if (type != null && typeof(InterfaceType).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(type) as InterfaceType;
                }
            }
            catch
            {
            }

            return default(InterfaceType);
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
                lock (syncRoot)
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
                                    ServerType = CastleFactoryType.Remote,
                                    ThrowError = true
                                };
                        }

                        singleton = new CastleFactory(config);
                    }
                }
            }

            return singleton;
        }

        /// <summary>
        /// 获取所有远程节点
        /// </summary>
        /// <returns></returns>
        public IList<ServerNode> GetServerNodes()
        {
            IList<ServerNode> nodes = new List<ServerNode>();

            if (config.ServerType == CastleFactoryType.Proxy && nodeResolver != null)
            {
                nodes = nodeResolver.GetAllServerNode();
            }

            if (nodes.Count == 0)
            {
                nodes = config.Nodes.Values.ToList();
            }

            return nodes;
        }

        #endregion

        #endregion

        #region Get Service

        /// <summary>
        /// 注册接口调用依赖
        /// </summary>
        /// <param name="call"></param>
        public void Register(IServiceCall call)
        {
            this.call = call;
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

            //获取节点
            var type = typeof(IServiceInterfaceType);
            var nodes = GetServerNodesFromConfig(service, nodeKey, type.Assembly.FullName, type.FullName);

            return GetChannel<IServiceInterfaceType>(service, nodes);
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
        /// <param name="nodes"></param>
        /// <returns></returns>
        private IServiceInterfaceType GetChannel<IServiceInterfaceType>(IService service, params ServerNode[] nodes)
            where IServiceInterfaceType : class
        {
            if (service == null)
            {
                //获取代理服务
                service = GetProxyService(nodes);
            }

            //返回通道服务
            return GetProxyChannel<IServiceInterfaceType>(service, true);
        }

        /// <summary>
        /// 获取代理服务
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private IService GetProxyService(params ServerNode[] nodes)
        {
            if (nodes.Count() == 0)
            {
                throw new WarningException("Proxy server node can't for empty.");
            }

            var nodeKey = nodes.First().Key.ToLower();

            if (!proxies.ContainsKey(nodeKey))
            {
                lock (syncRoot)
                {
                    if (!proxies.ContainsKey(nodeKey))
                    {
                        var remoteProxies = new List<RemoteProxy>();

                        foreach (var node in nodes)
                        {
                            RemoteProxy proxy = null;
                            if (node.RespType == ResponseType.Json)
                                proxy = new InvokeProxy(node, container);
                            else
                                proxy = new RemoteProxy(node, container, false);

                            proxy.OnConnected += (sender, args) =>
                            {
                                if (OnConnected != null) OnConnected(sender, args);
                            };

                            proxy.OnDisconnected += (sender, args) =>
                            {
                                if (OnDisconnected != null) OnDisconnected(sender, args);
                            };

                            remoteProxies.Add(proxy);
                        }

                        //实例化服务代理
                        proxies[nodeKey] = new ServiceProxy(nodeKey, remoteProxies);
                    }
                }
            }

            return proxies[nodeKey];
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

            if (isCacheService)
            {
                if (!hashtable.ContainsKey(serviceKey))
                {
                    lock (hashtable)
                    {
                        if (!hashtable.ContainsKey(serviceKey))
                        {
                            //创建代理服务
                            var dynamicProxy = CreateProxyHandler<IServiceInterfaceType>(proxy, serviceType);
                            hashtable[serviceKey] = dynamicProxy;
                        }
                    }
                }

                return (IServiceInterfaceType)hashtable[serviceKey];
            }
            else
            {
                //不缓存，直接返回服务
                return CreateProxyHandler<IServiceInterfaceType>(proxy, serviceType);
            }
        }

        /// <summary>
        /// 获取代理服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <param name="proxy"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private IServiceInterfaceType CreateProxyHandler<IServiceInterfaceType>(IService proxy, Type serviceType)
        {
            var handler = new ServiceInvocationHandler<IServiceInterfaceType>(this.config, this.container, this.call, proxy);
            return (IServiceInterfaceType)ProxyFactory.GetInstance().Create(handler, serviceType, true);
        }

        #endregion

        #region 回调订阅

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

            if (callback == null)
            {
                throw new IoCException("Callback cannot be the null.");
            }

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
            var proxy = new CallbackProxy(callback, node, container);

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

            //获取节点
            var nodes = GetServerNodesFromConfig(service, nodeKey, string.Empty, message.ServiceName);

            return Invoke(service, message, nodes);
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

            return Invoke(service, message, node);
        }

        /// <summary>
        /// 响应服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private InvokeData Invoke(IService service, InvokeMessage message, params ServerNode[] nodes)
        {
            if (service == null)
            {
                //获取代理服务
                service = GetProxyService(nodes);
            }

            //调用分布式服务
            var caller = new InvokeCaller(this.config, this.container, this.call, service);

            //返回响应信息
            return caller.InvokeResponse(message);
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
            if (config.ServerType != CastleFactoryType.Remote)
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
                    if (config.ServerType == CastleFactoryType.Local)
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
        /// <param name="service"></param>
        /// <param name="nodeKey"></param>
        /// <param name="assemblyName"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private ServerNode[] GetServerNodesFromConfig(IService service, string nodeKey, string assemblyName, string serviceName)
        {
            IList<ServerNode> nodes = new List<ServerNode>();

            //如果存在本地服务，则跳过
            if (service != null) return nodes.ToArray();

            if (config.ServerType == CastleFactoryType.Proxy && nodeResolver != null)
            {
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = assemblyName.Split(',')[0];
                }

                var @namespace = string.Empty;

                if (!string.IsNullOrEmpty(serviceName))
                {
                    //取命名空间
                    var arr = serviceName.Split('.');
                    @namespace = string.Join(".", arr, 0, arr.Length - 1);
                }

                //获取服务节点
                nodes = nodeResolver.GetServerNodes(assemblyName, @namespace);
            }

            //获取服务节点
            if (nodes.Count == 0)
            {
                //获取匹配的节点
                var tmpNodes = GetServerNodes();

                if (tmpNodes.Count == 0)
                {
                    throw new WarningException("Not find any server node.");
                }

                nodes = tmpNodes.Where(p => string.Compare(p.Key, nodeKey, true) == 0).ToList();
            };

            //如果没有合适的节点，返回null
            if (nodes.Count == 0)
            {
                throw new WarningException(string.Format("Did not find the server node【{0}】.", nodeKey));
            }

            //返回空列表
            return nodes.ToArray();
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
