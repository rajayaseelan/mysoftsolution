using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 默认的服务节点解析器
    /// </summary>
    public class DefaultNodeResolver : IServerNodeResolver
    {
        private ServerConfig config;
        private string serverConfigPath = "/config/serverConfig.xml";

        /// <summary>
        /// 实例化DefaultNodeResolver
        /// </summary>
        public DefaultNodeResolver()
        {
            this.config = InitServerConfig();

            var path = ConfigurationManager.AppSettings["serverConfigPath"];
            if (!string.IsNullOrEmpty(path))
            {
                this.serverConfigPath = path;
            }

            //检测服务节点
            try { CheckServerNode(); }
            catch { }

            //5分钟检测一次
            var timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                (sender as Timer).Stop();

                //检测服务节点
                CheckServerNode();
            }
            catch (Exception ex)
            {
                //写错误日志
                SimpleLog.Instance.WriteLogForDir("serverConfig", ex);
            }
            finally
            {
                (sender as Timer).Start();
            }
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
                Func<NodeConfig, bool> where = null;

                if (!string.IsNullOrEmpty(assemblyName))
                {
                    where = new Func<NodeConfig, bool>(p => !string.IsNullOrEmpty(p.AssemblyName));
                    func = new Func<NodeConfig, bool>(p => string.Compare(p.AssemblyName, assemblyName, true) == 0);
                }
                else if (!string.IsNullOrEmpty(@namespace))
                {
                    where = new Func<NodeConfig, bool>(p => !string.IsNullOrEmpty(p.Namespace));
                    func = new Func<NodeConfig, bool>(p => string.Compare(p.Namespace, @namespace, true) == 0);
                }

                //获取节点配置
                var conf = configs.Where(where).FirstOrDefault(func);
                if (conf == null) return new List<ServerNode>();

                //返回符合条件的第一个Key
                var nodes = config.Nodes.Cast<ServerNode>();
                return nodes.Where(p => string.Compare(p.Key, conf.Key, true) == 0).ToList();
            }

            return new List<ServerNode>();
        }

        #endregion

        /// <summary>
        /// 读取xml配置文件
        /// </summary>
        /// <param name="fromRemote"></param>
        /// <returns></returns>
        protected virtual ServerConfig GetXmlFileConfig(bool fromRemote)
        {
            //配置文件
            var fileName = CoreHelper.GetFullPath(serverConfigPath);
            var xml = string.Empty;

            if (fromRemote || !File.Exists(fileName))
            {
                try
                {
                    //从远程读取
                    var url = string.Format("{0}{1}", "http://www.fund123.cn", serverConfigPath);
                    xml = new HttpHelper().Reader(url);

                    if (!xml.ToLower().StartsWith("<?xml"))
                    {
                        xml = string.Empty;
                    }
                }
                catch (WebException ex)
                {
                    //找到内部响应
                    throw new WebException(string.Format("请求资源{0}异常。", ex.Response.ResponseUri), ex);
                }
            }
            else
            {
                //从本地读取xml
                xml = File.ReadAllText(fileName, Encoding.UTF8);
            }

            if (!string.IsNullOrEmpty(xml))
            {
                //获取xml文件内容
                return SerializationManager.DeserializeXml<ServerConfig>(xml);
            }

            return null;
        }

        /// <summary>
        /// 初始化服务节点
        /// </summary>
        /// <returns></returns>
        private ServerConfig InitServerConfig()
        {
            var config = new ServerConfig();

            try
            {
                //从文件读取xml
                var tmpConfig = GetXmlFileConfig(false);

                if (tmpConfig != null)
                {
                    config.Configs = tmpConfig.Configs;
                    config.Version = tmpConfig.Version;

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

        /// <summary>
        /// 检测服务节点
        /// </summary>
        private void CheckServerNode()
        {
            //判断版本号，如果版本大于当前版本，则替换之
            var tmpConfig = GetXmlFileConfig(true);

            if (tmpConfig != null)
            {
                if (tmpConfig.Nodes.Count > 0
                    && string.Compare(tmpConfig.Version, config.Version, true) > 0)
                {
                    this.config = tmpConfig;
                }
            }
        }
    }
}
