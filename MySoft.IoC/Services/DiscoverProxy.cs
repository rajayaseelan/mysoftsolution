using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 发现服务代理
    /// </summary>
    public class DiscoverProxy : IService
    {
        private CastleFactory factory;
        private Random random;
        private IList<ServerNode> nodes;
        private ILog logger;
        private IDictionary<string, IList<IService>> services;

        /// <summary>
        /// 实例化DiscoverProxy
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="nodes"></param>
        /// <param name="logger"></param>
        public DiscoverProxy(CastleFactory factory, IList<ServerNode> nodes, ILog logger)
        {
            this.factory = factory;
            this.nodes = nodes;
            this.logger = logger;
            this.random = new Random();
            this.services = new Dictionary<string, IList<IService>>();
        }

        #region IService 成员

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get
            {
                return typeof(DiscoverProxy).FullName;
            }
        }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            IList<IService> proxies = new List<IService>();

            //判断是否为StatusService服务
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
            {
                throw new WarningException("Status service can't use discover service way!");
            }

            //如果能找到服务
            if (services.ContainsKey(reqMsg.ServiceName))
            {
                proxies = services[reqMsg.ServiceName];
            }
            else
            {
                //找到代理服务
                lock (services)
                {
                    foreach (var node in nodes)
                    {
                        //获取服务
                        var service = factory.GetChannel<IStatusService>(node);

                        //检测是否存在服务
                        if (service.ContainsService(reqMsg.ServiceName))
                        {
                            IService proxy = null;

                            if (node.RespType == ResponseType.Json)
                                proxy = new InvokeProxy(node, logger);
                            else
                                proxy = new RemoteProxy(node, logger);

                            proxies.Add(proxy);
                        }
                    }

                    //缓存代理服务
                    if (proxies.Count > 0)
                        services[reqMsg.ServiceName] = proxies;
                    else
                        throw new WarningException(string.Format("Did not find the proxy service {0}!", reqMsg.ServiceName));
                }
            }

            //随机获取一个代理，实现分布式处理
            return proxies[random.Next(proxies.Count)].CallService(reqMsg);
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            this.services.Clear();
        }

        #endregion
    }
}
