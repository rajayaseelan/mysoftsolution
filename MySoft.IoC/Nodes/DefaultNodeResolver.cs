using MySoft.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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
            var path = ConfigurationManager.AppSettings["serverConfigPath"];
            if (!string.IsNullOrEmpty(path))
            {
                this.serverConfigPath = path;
            }

            //初始化配置
            this.config = GetServerConfig();

            //检测更新
            CheckConfigUpdate();

            //10分钟检测一次
            var timer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                (sender as Timer).Stop();

                //检测更新
                CheckConfigUpdate();
            }
            finally
            {
                (sender as Timer).Start();
            }
        }

        /// <summary>
        /// 检测更新
        /// </summary>
        private void CheckConfigUpdate()
        {
            try
            {
                var xml = string.Empty;

                //检测服务节点
                if (CheckServerNode(serverConfigPath, out xml))
                {
                    //配置文件
                    var fileName = CoreHelper.GetFullPath(serverConfigPath);
                    var dir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    //写配置文件
                    File.WriteAllText(fileName, xml);
                }
            }
            catch (Exception ex)
            {
                //写错误日志
                SimpleLog.Instance.WriteLogForDir("serverConfig", ex);
            }
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
        /// <returns></returns>
        protected virtual ServerConfig GetServerConfig()
        {
            try
            {
                //配置文件
                var fileName = CoreHelper.GetFullPath(serverConfigPath);

                if (File.Exists(fileName))
                {
                    //从本地读取xml
                    var xml = File.ReadAllText(fileName, Encoding.UTF8);

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
                SimpleLog.Instance.WriteLogForDir("serverConfig", ex);
            }

            return new ServerConfig();
        }

        /// <summary>
        /// 检测服务节点
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        private bool CheckServerNode(string configPath, out string xml)
        {
            //判断版本号，如果版本大于当前版本，则替换之
            var tmpConfig = GetConfigFromRemote(configPath, out xml);

            if (tmpConfig != null)
            {
                if (tmpConfig.Nodes.Count > 0
                    && string.Compare(tmpConfig.Version, config.Version, true) > 0)
                {
                    this.config = tmpConfig;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 读取xml配置文件
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        private ServerConfig GetConfigFromRemote(string configPath, out string xml)
        {
            xml = string.Empty;

            try
            {
                //从远程读取
                var url = string.Format("{0}{1}", "http://www.fund123.cn", configPath);
                xml = new HttpHelper().Reader(url);

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
