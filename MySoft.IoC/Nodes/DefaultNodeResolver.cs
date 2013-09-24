using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        /// 获取所有节点
        /// </summary>
        /// <returns></returns>
        public IList<ServerNode> GetAllServerNode()
        {
            return config.Nodes.Cast<ServerNode>().ToList();
        }

        /// <summary>
        /// 获取服务器节点
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public IList<ServerNode> GetServerNodes(string assemblyName, string @namespace)
        {
            if (config.Nodes.Count == 0) return new List<ServerNode>();

            if (!string.IsNullOrEmpty(assemblyName) || !string.IsNullOrEmpty(@namespace))
            {
                //如果存在节点
                var configs = config.Configs.Cast<NodeConfig>();
                Func<NodeConfig, bool> func = null;

                if (!string.IsNullOrEmpty(assemblyName))
                {
                    func = new Func<NodeConfig, bool>(p => string.Compare(p.AssemblyName, assemblyName, true) == 0);
                }
                else if (!string.IsNullOrEmpty(@namespace))
                {
                    func = new Func<NodeConfig, bool>(p => string.Compare(p.Namespace, @namespace, true) == 0);
                }

                //获取节点配置
                var conf = configs.FirstOrDefault(func);
                if (conf == null) return new List<ServerNode>();

                //返回符合条件的第一个Key
                var nodes = config.Nodes.Cast<ServerNode>();
                return nodes.Where(p => string.Compare(p.Key, conf.Key, true) == 0).ToList();
            }

            return new List<ServerNode>();
        }

        /// <summary>
        /// 读取xml文件内容
        /// </summary>
        /// <returns></returns>
        protected virtual string GetXmlFileString()
        {
            //配置文件
            var fileName = CoreHelper.GetFullPath("/config/serverConfig.xml");

            if (!File.Exists(fileName))
            {
                try
                {
                    //从远程读取
                    return new HttpHelper().Reader("http://inc.fund123.cn/config/serverConfig.xml");
                }
                catch (WebException ex)
                {
                    //找到内部响应
                    throw new WebException(string.Format("请求资源{0}异常。", ex.Response.ResponseUri), ex);
                }
            }
            else
            {
                return File.ReadAllText(fileName, Encoding.UTF8);
            }
        }

        /// <summary>
        /// 初始化服务节点
        /// </summary>
        /// <returns></returns>
        protected virtual ServerConfig InitServerConfig()
        {
            var config = new ServerConfig();

            try
            {
                //获取xml文件内容
                var xml = GetXmlFileString();

                if (!string.IsNullOrEmpty(xml))
                {
                    var tmpConfig = SerializationManager.DeserializeXml<ServerConfig>(xml);
                    config.Configs = tmpConfig.Configs;

                    foreach (ServerNode node in tmpConfig.Nodes)
                    {
                        IPAddress address;
                        if (IPAddress.TryParse(node.IP, out address))
                        {
                            if (address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                config.Nodes.Add(node);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //写错误日志
                SimpleLog.Instance.WriteLogForDir("serverConfig", ex);
            }

            return config;
        }

        #endregion
    }
}
