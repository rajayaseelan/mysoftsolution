using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MySoft.Logger;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 默认的服务节点解析器
    /// </summary>
    public class DefaultNodeResolver : IServerNodeResolver
    {
        private ServerConfig config;

        /// <summary>
        /// 实例化DefaultNodeResolver
        /// </summary>
        public DefaultNodeResolver()
        {
            this.config = InitServerConfig();
        }

        #region IServerNodeResolver 成员

        /// <summary>
        /// 服务节点解析
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public virtual IList<ServerNode> GetServerNodes(string nodeKey, string serviceName)
        {
            if (string.IsNullOrEmpty(nodeKey))
            {
                nodeKey = config.DefaultKey;
            }

            if (config.Nodes.Count() > 0)
            {
                if (!config.Nodes.Any(p => string.Compare(p.Key, nodeKey, true) == 0))
                    nodeKey = config.DefaultKey;

                return config.Nodes.Where(p => string.Compare(p.Key, nodeKey, true) == 0).ToList();
            }

            return new List<ServerNode>();
        }

        /// <summary>
        /// 初始化服务节点
        /// </summary>
        /// <returns></returns>
        private ServerConfig InitServerConfig()
        {
            var fileName = CoreHelper.GetFullPath("/config/serverNode.config");
            var config = new ServerConfig();

            if (File.Exists(fileName))
            {
                try
                {
                    var xml = File.ReadAllText(fileName, Encoding.UTF8);
                    if (!string.IsNullOrEmpty(xml))
                    {
                        var tmpConfig = SerializationManager.DeserializeXml<ServerConfig>(xml);
                        config.DefaultKey = tmpConfig.DefaultKey;

                        var list = new List<ServerNode>();

                        foreach (var node in tmpConfig.Nodes)
                        {
                            try
                            {
                                IPAddress address;
                                if (IPAddress.TryParse(node.IP, out address))
                                {
                                    if (address.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        list.Add(node);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }

                        if (list.Count > 0)
                        {
                            if (string.IsNullOrEmpty(config.DefaultKey))
                            {
                                config.DefaultKey = list[0].Key;
                            }

                            //处理nodes
                            config.Nodes = list.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //写错误日志
                    SimpleLog.Instance.WriteLogForDir("serverNode", ex);
                }
            }

            return config;
        }

        #endregion
    }
}
