using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace MySoft.Remoting.Configuration
{
    /// <summary>
    /// Remoting客户端配置
    /// <remarks>
    /// <code>
    /// <configuration>
    ///     <configSections>
    /// 	    <sectionGroup name="mysoft.framework">
    /// 		    <section name="remotingClient" type="MySoft.Remoting.Configuration.RemotingClientConfigurationHandler, MySoft"/>
    /// 	    </sectionGroup>
    ///     </configSections>
    ///     <system.web>
    /// 	......
    ///     </system.web>
    ///     <mysoft.framework>
    /// 	    <remotingClient isCheckServer="true" interval="3000" compress="true">
    ///             <remotingHost name="NetValueClient" defaultServer="s1">
    ///                 <server name="s1" url="tcp://192.168.0.1:8888"/>
    ///                 <server name="s2" url="tcp://192.168.0.2:8888"/>
    ///                 <remoteObject name="NetValue" objectUri="SB.NetValue"/>
    ///             </remotingHost>
    /// 	    </remotingClient>
    ///     </mysoft.framework>
    /// </configuration>
    /// </code>
    /// </remarks>
    /// </summary>
    public class RemotingClientConfiguration : ConfigurationBase
    {
        /// <summary>
        /// 获取远程对象配置
        /// </summary>
        /// <returns></returns>
        public static RemotingClientConfiguration GetConfig()
        {
            string key = "mysoft.framework/remotingClient";
            RemotingClientConfiguration obj = CacheHelper.Get<RemotingClientConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as RemotingClientConfiguration;
                CacheHelper.Insert(key, obj, 60);
            }

            return obj;
        }

        private bool _IsCheckServer = false;

        /// <summary>
        /// 是否检测可用服务器
        /// </summary>
        public bool IsCheckServer
        {
            get { return _IsCheckServer; }
            set { _IsCheckServer = value; }
        }

        private double _Interval = 1000;

        /// <summary>
        /// 检测可用服务器定时器的时间间隔
        /// </summary>
        public double Interval
        {
            get { return _Interval; }
            set { _Interval = value; }
        }

        private Dictionary<string, RemotingHost> _RemotingHosts = new Dictionary<string, RemotingHost>();

        /// <summary>
        /// RemotingHost集合
        /// </summary>
        public Dictionary<string, RemotingHost> RemotingHosts
        {
            get { return _RemotingHosts; }
        }

        /// <summary>
        /// 获取远程业务对象Url（比如：tcp://127.0.0.1:8888/NetValue）
        /// </summary>
        /// <param name="serverUrl">远程服务器地址（比如：tcp://127.0.0.1:8888）</param>
        /// <param name="remoteObjectUri">远程对象Uri（如：NetValue）</param>
        /// <returns></returns>
        public string GetRemoteObjectUrl(string serverUrl, string remoteObjectUri)
        {
            string url = string.Format("{0}/{1}", serverUrl, remoteObjectUri);
            return url;
        }

        /// <summary>
        /// 从配置文件加载配置值
        /// </summary>
        /// <param name="node"></param>
        public void LoadValuesFromConfigurationXml(XmlNode node)
        {
            if (node == null) return;

            if (node.Attributes["isCheckServer"].Value != null)
                this._IsCheckServer = node.Attributes["isCheckServer"].Value == "true" ? true : false;
            if (node.Attributes["interval"].Value != null)
                this._Interval = Convert.ToDouble(node.Attributes["interval"].Value);

            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Comment) continue;

                if (n.Name == "remotingHost")
                {
                    XmlAttributeCollection ac = n.Attributes;
                    RemotingHost host = new RemotingHost();
                    host.Name = ac["name"].Value;
                    host.DefaultServer = ac["defaultServer"].Value;

                    Dictionary<string, string> modules = new Dictionary<string, string>();
                    Dictionary<string, RemotingServer> servers = new Dictionary<string, RemotingServer>();

                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.NodeType == XmlNodeType.Comment) continue;
                        XmlAttributeCollection ac1 = n1.Attributes;

                        if (n1.Name == "server")
                        {
                            RemotingServer server = new RemotingServer();
                            server.ServerName = ac1["name"].Value;
                            server.ServerUrl = ac1["url"].Value;
                            servers.Add(server.ServerName, server);
                        }

                        if (n1.Name == "remoteObject")
                        {
                            string name = ac1["name"].Value;
                            string objectUri = ac1["objectUri"].Value;

                            if (!modules.ContainsKey(name))
                            {
                                modules.Add(name, objectUri);
                            }
                        }
                    }//end foreach

                    host.Servers = servers;
                    host.Modules = modules;

                    if (!this._RemotingHosts.ContainsKey(host.Name))
                    {
                        this._RemotingHosts.Add(host.Name, host);
                    }
                }
            }
        }

    }
}
