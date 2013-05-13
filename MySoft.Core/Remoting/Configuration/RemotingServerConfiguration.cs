using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Remoting;
using System.Xml;

namespace MySoft.Remoting.Configuration
{
    /// <summary>
    /// Remoting服务端配置
    /// <example>
    /// <code>
    /// <configuration>
    ///     <configSections>
    /// 	    <sectionGroup name="mysoft.framework">
    /// 		    <section name="remotingServer" type="MySoft.Remoting.Configuration.RemotingServerConfigurationHandler, MySoft"/>
    /// 	    </sectionGroup>
    ///     </configSections>
    ///     <system.web>
    /// 	......
    ///     </system.web>
    ///     <mysoft.framework>
    /// 	    <remotingServer>
    ///             <server channelType="tcp" serverAddress="127.0.0.1" port="8888" compress="true"/>
    /// 		    <remoteObject name="数据同步" assemblyName="Simple" className="Simple.SyncData" mode="singleton" />
    /// 	    </remotingServer>
    ///     </mysoft.framework>
    /// </configuration>
    /// </code>
    /// </example>
    /// </summary>
    public class RemotingServerConfiguration : ConfigurationBase
    {
        /// <summary>
        /// 获取远程对象配置
        /// </summary>
        /// <returns></returns>
        public static RemotingServerConfiguration GetConfig()
        {
            string key = "mysoft.framework/remotingServer";
            RemotingServerConfiguration obj = CacheHelper.Get<RemotingServerConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as RemotingServerConfiguration;
                CacheHelper.Permanent(key, obj);
            }

            return obj;
        }

        List<ServiceModule> _Modules = new List<ServiceModule>();

        /// <summary>
        /// 获取远程对象业务模块集合（部署的URL信息，协议，IP及端口）
        /// </summary>
        public List<ServiceModule> Modules
        {
            get { return _Modules; }
        }

        private RemotingChannelType _ChannelType = RemotingChannelType.Tcp;

        /// <summary>
        /// 通道类型
        /// </summary>
        public RemotingChannelType ChannelType
        {
            get { return _ChannelType; }
        }

        private string _ServerAddress;

        /// <summary>
        /// 服务器地址（IP或计算机名）
        /// </summary>
        public string ServerAddress
        {
            get { return _ServerAddress; }
        }

        private int _Port = 8888;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        /// <summary>
        /// Remoting服务器Url（如：tcp://127.0.0.1:8888）
        /// </summary>
        public string ServerUrl
        {
            get
            {
                return string.Format("{0}://{1}:{2}", _ChannelType, _ServerAddress, _Port);
            }
        }

        /// <summary>
        /// 获取远程业务对象Url（如：tcp://127.0.0.1:8888/NetValue）
        /// </summary>
        /// <param name="remoteObjectUri">远程对象Uri（如：NetValue）</param>
        /// <returns></returns>
        public string GetRemoteObjectUrl(string remoteObjectUri)
        {
            string url = string.Format("{0}://{1}:{2}/{3}", _ChannelType, _ServerAddress, _Port, remoteObjectUri);
            return url;
        }

        /// <summary>
        /// 从配置文件加载配置值
        /// </summary>
        /// <param name="node"></param>
        public void LoadValuesFromConfigurationXml(XmlNode node)
        {
            if (node == null) return;

            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Comment) continue;

                if (n.Name == "server")
                {
                    XmlAttributeCollection ac = n.Attributes;
                    this._ChannelType = ac["channelType"].Value.ToLower() == "tcp" ? RemotingChannelType.Tcp : RemotingChannelType.Http;
                    this._ServerAddress = ac["serverAddress"].Value;
                    this._Port = Convert.ToInt32(ac["port"].Value);
                }

                if (n.Name == "remoteObject")
                {
                    XmlAttributeCollection ac = n.Attributes;
                    ServiceModule module = new ServiceModule();
                    module.Name = ac["name"].Value;
                    module.AssemblyName = ac["assemblyName"].Value;
                    module.ClassName = ac["className"].Value;

                    if (string.IsNullOrEmpty(ac["mode"].Value))
                    {
                        module.Mode = ac["mode"].Value.ToLower() == "singleton" ? WellKnownObjectMode.Singleton : WellKnownObjectMode.SingleCall;
                    }

                    this._Modules.Add(module);
                }
            }
        }

    }
}
