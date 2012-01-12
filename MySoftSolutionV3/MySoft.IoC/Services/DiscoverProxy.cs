using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 发现服务代理
    /// </summary>
    public class DiscoverProxy : IService, IDisposable
    {
        private CastleFactory factory;
        private IDictionary<string, IList<RemoteProxy>> services;

        /// <summary>
        /// 实例化DiscoverProxy
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        public DiscoverProxy(CastleFactory factory, ILog logger)
        {
            this.factory = factory;
            this.services = new Dictionary<string, IList<RemoteProxy>>();
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
            IList<RemoteProxy> proxies = new List<RemoteProxy>();

            //判断是否为StatusService服务
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
            {
                throw new WarningException("State services can't use discover service way!");
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
                    //代理数为1时，直接处理
                    if (factory.Proxies.Count == 1)
                    {
                        proxies.Add(factory.Proxies[0]);
                        services[reqMsg.ServiceName] = proxies;
                    }
                    else
                    {
                        foreach (var proxy in factory.Proxies)
                        {
                            try
                            {
                                //自定义实现一个RemoteNode
                                var node = new RemoteNode
                                {
                                    IP = proxy.Node.IP,
                                    Port = proxy.Node.Port,
                                    Compress = proxy.Node.Compress,
                                    Encrypt = proxy.Node.Encrypt,
                                    Key = Guid.NewGuid().ToString(),
                                    MaxPool = 1,
                                    Timeout = 30
                                };

                                var service = factory.GetChannel<IStatusService>(node);

                                //检测是否存在服务
                                if (service.ContainsService(reqMsg.ServiceName))
                                {
                                    proxies.Add(proxy);
                                }
                            }
                            catch (WarningException ex)
                            {
                                throw ex;
                            }
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
            var rndIndex = new Random(Guid.NewGuid().GetHashCode()).Next(proxies.Count);
            return proxies[rndIndex].CallService(reqMsg);
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            this.services.Clear();
            this.services = null;
        }

        #endregion
    }
}
