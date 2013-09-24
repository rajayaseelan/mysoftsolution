using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务代理类
    /// </summary>
    public class ServiceProxy : IService
    {
        private string nodeKey;
        private IList<RemoteProxy> services;
        private Random random;

        /// <summary>
        /// 服务代理类
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="services"></param>
        public ServiceProxy(string nodeKey, IList<RemoteProxy> services)
        {
            this.nodeKey = nodeKey;
            this.services = services;
            this.random = new Random();
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get
            {
                return string.Format("{0}${1}", typeof(ServiceProxy).FullName, nodeKey);
            }
        }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //要保证服务是连接正常的
            var tmpServices = services.Where(p => p.Node.Connected).ToList();

            if (tmpServices.Count == 0)
            {
                //延时5秒
                Thread.Sleep(TimeSpan.FromSeconds(5));

                //获取节点字符串
                var nodeString = string.Join("|", services.Select(p => string.Format("【{0} -> {1}:{2}】", p.Node.Key, p.Node.IP, p.Node.Port)).ToArray());

                throw new WarningException("Don't have any available service node, please check the service status! " + nodeString);
            }

            //随机获取服务
            var service = tmpServices[random.Next(0, tmpServices.Count)];

            try
            {
                //调用服务
                return service.CallService(reqMsg);
            }
            catch (SocketException ex)
            {
                //添加连接
                AddConnection(reqMsg, service.Node, ex);

                throw GetException(service.Node, ex);
            }
        }

        /// <summary>
        /// 添加连接
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="node"></param>
        /// <param name="ex"></param>
        private void AddConnection(RequestMessage reqMsg, ServerNode node, SocketException ex)
        {
            var client = new AppClient
            {
                AppVersion = reqMsg.AppVersion,
                AppName = reqMsg.AppName,
                AppPath = reqMsg.AppPath,
                HostName = reqMsg.HostName,
                IPAddress = reqMsg.IPAddress
            };

            //添加节点到连接管理器
            ConnectionManager.AddNode(client, node, ex);
        }

        /// <summary>
        /// 获取通讯异常
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private Exception GetException(ServerNode node, SocketException ex)
        {
            var message = string.Format("Can't connect to server ({0}:{1})！Server node : {2} -> ({3}) {4}"
                    , node.IP, node.Port, node.Key, ex.ErrorCode, ex.SocketErrorCode);

            return new WarningException(ex.ErrorCode, message);
        }
    }
}
