using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Logger;
using MySoft.IoC.Messages;
using MySoft.IoC.Status;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 发现服务代理
    /// </summary>
    public class DiscoverProxy : IService, IDisposable
    {
        private CastleFactory factory;
        private IList<RemoteProxy> proxies;
        private IDictionary<string, RemoteProxy> services;
        public DiscoverProxy(CastleFactory factory, ILog logger)
        {
            this.factory = factory;
            this.proxies = factory.Proxies;
            this.services = new Dictionary<string, RemoteProxy>();
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
            RemoteProxy proxy = null;

            //判断是否为StatusService服务
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
            {
                throw new WarningException("State services can't use discover service way!");
            }

            //如果能找到服务
            if (this.services.ContainsKey(reqMsg.ServiceName))
            {
                proxy = this.services[reqMsg.ServiceName];
            }
            else
            {
                lock (services)
                {
                    //找到代理服务
                    foreach (var p in proxies)
                    {
                        try
                        {
                            //自定义实现一个RemoteNode
                            var node = new RemoteNode
                            {
                                IP = p.Node.IP,
                                Port = p.Node.Port,
                                Compress = p.Node.Compress,
                                Encrypt = p.Node.Encrypt,
                                Key = Guid.NewGuid().ToString(),
                                MaxPool = 1,
                                Timeout = 30
                            };

                            var service = factory.GetChannel<IStatusService>(node);

                            //检测是否存在服务
                            if (service.ContainsService(reqMsg.ServiceName))
                            {
                                proxy = p;
                                this.services[reqMsg.ServiceName] = p;
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }

            //如果代理服务不存在
            if (proxy == null)
            {
                throw new WarningException(string.Format("Did not find the discover agent service {0}!", reqMsg.ServiceName));
            }

            return proxy.CallService(reqMsg);
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            this.services = null;
            this.proxies = null;
        }

        #endregion
    }
}
