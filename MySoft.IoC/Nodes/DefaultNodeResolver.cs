using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 默认的服务节点解析器
    /// </summary>
    public class DefaultNodeResolver : IServerNodeResolver
    {
        private ServerConfig config;
        private bool isRemote;
        private string serverConfigPath;

        /// <summary>
        /// 实例化DefaultNodeResolver
        /// </summary>
        public DefaultNodeResolver(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path) || path.StartsWith("http://"))
                {
                    this.serverConfigPath = path;

                    if (path.StartsWith("http://"))
                        isRemote = true;
                }
                else
                    this.serverConfigPath = CoreHelper.GetFullPath(path);
            }
            else
            {
                this.serverConfigPath = CoreHelper.GetFullPath("/config/serverConfig.xml");
            }

            //初始化配置
            this.config = GetServerConfig(serverConfigPath);
        }

        #region IServerNodeResolver 成员

        /// <summary>
        /// 获取所有节点
        /// </summary>
        /// <returns></returns>
        public virtual IList<ServerNode> GetAllServerNode()
        {
            return config.Nodes.Cast<ServerNode>().ToList();
        }

        /// <summary>
        /// 获取服务器节点
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public virtual IList<ServerNode> GetServerNodes(string assemblyName, string @namespace)
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
                if (conf != null)
                {
                    //返回符合条件的第一个Key
                    var nodes = config.Nodes.Cast<ServerNode>();
                    return nodes.Where(p => string.Compare(p.Key, conf.Key, true) == 0).ToList();
                }
            }

            return new List<ServerNode>();
        }

        #endregion

        /// <summary>
        /// 获取服务节点
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        private ServerConfig GetServerConfig(string configPath)
        {
            try
            {
                if (isRemote)
                {
                    return GetConfigFromRemote(configPath);
                }
                else if (File.Exists(configPath))
                {
                    //从本地读取xml
                    var xml = File.ReadAllText(configPath, Encoding.UTF8);

                    if (!string.IsNullOrEmpty(xml))
                    {
                        //获取xml文件内容
                        return SerializationManager.DeserializeXml<ServerConfig>(xml);
                    }
                }
            }
            catch (Exception ex)
            {
                //写错误日志
                SimpleLog.Instance.WriteLogForDir("ServerConfig", ex);
            }

            return new ServerConfig();
        }

        /// <summary>
        /// 读取xml配置文件
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        private ServerConfig GetConfigFromRemote(string configPath)
        {
            try
            {
                //从远程读取
                var xml = new HttpHelper().Reader(configPath);

                if (!xml.ToLower().StartsWith("<?xml"))
                {
                    xml = string.Empty;
                }

                if (!string.IsNullOrEmpty(xml))
                {
                    //获取xml文件内容
                    return SerializationManager.DeserializeXml<ServerConfig>(xml);
                }

                return null;
            }
            catch (WebException ex)
            {
                //找到内部响应
                throw new WebException(string.Format("请求资源{0}异常。", ex.Response.ResponseUri), ex);
            }
        }
    }
}
