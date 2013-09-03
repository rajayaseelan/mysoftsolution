using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace MySoft.IoC.Configuration
{
    /// <summary>
    /// The service factory configuration.
    /// </summary>
    public class CastleFactoryConfiguration : ConfigurationBase
    {
        private IDictionary<string, ServerNode> nodes;
        private CastleFactoryType type = CastleFactoryType.Local;
        private string defaultKey, proxyServer;                                 //代理服务
        private string appname;                                                 //host名称
        private bool throwError = true;                                         //抛出异常
        private bool enableCache = true;                                        //是否缓存

        /// <summary>
        /// 实例化CastleFactoryConfiguration
        /// </summary>
        public CastleFactoryConfiguration()
        {
            this.nodes = new Dictionary<string, ServerNode>();
        }

        /// <summary>
        /// 获取远程对象配置
        /// </summary>
        /// <returns></returns>
        public static CastleFactoryConfiguration GetConfig()
        {
            string key = "mysoft.framework/castleFactory";
            CastleFactoryConfiguration obj = CacheHelper.Get<CastleFactoryConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as CastleFactoryConfiguration;
                CacheHelper.Permanent(key, obj);
            }

            return obj;
        }

        /// <summary>
        /// 从配置文件加载配置值
        /// </summary>
        /// <param name="xmlnode"></param>
        public void LoadValuesFromConfigurationXml(XmlNode xmlnode)
        {
            if (xmlnode == null) return;

            XmlAttributeCollection attribute = xmlnode.Attributes;

            if (attribute["type"] != null && attribute["type"].Value.Trim() != string.Empty)
                type = (CastleFactoryType)Enum.Parse(typeof(CastleFactoryType), attribute["type"].Value, true);

            if (attribute["throwError"] != null && attribute["throwError"].Value.Trim() != string.Empty)
                throwError = Convert.ToBoolean(attribute["throwError"].Value);

            if (attribute["default"] != null && attribute["default"].Value.Trim() != string.Empty)
                defaultKey = attribute["default"].Value;

            if (attribute["proxy"] != null && attribute["proxy"].Value.Trim() != string.Empty)
                proxyServer = attribute["proxy"].Value;

            if (attribute["appname"] != null && attribute["appname"].Value.Trim() != string.Empty)
                appname = attribute["appname"].Value;

            if (attribute["enableCache"] != null && attribute["enableCache"].Value.Trim() != string.Empty)
                enableCache = Convert.ToBoolean(attribute["enableCache"].Value);

            foreach (XmlNode child in xmlnode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment) continue;

                XmlAttributeCollection childattribute = child.Attributes;
                if (child.Name == "serverNode")
                {
                    var ip = childattribute["ip"].Value;
                    var port = Convert.ToInt32(childattribute["port"].Value);

                    var node = ServerNode.Parse(ip, port);
                    node.Key = childattribute["key"].Value;

                    //超时时间，默认为1分钟
                    if (childattribute["timeout"] != null && childattribute["timeout"].Value.Trim() != string.Empty)
                        node.Timeout = Convert.ToInt32(childattribute["timeout"].Value);

                    if (childattribute["compress"] != null && childattribute["compress"].Value.Trim() != string.Empty)
                        node.Compress = Convert.ToBoolean(childattribute["compress"].Value);

                    if (childattribute["format"] != null && childattribute["format"].Value.Trim() != string.Empty)
                        node.RespType = (ResponseType)Enum.Parse(typeof(ResponseType), childattribute["format"].Value, true);

                    //最大连接池
                    if (childattribute["maxCaller"] != null && childattribute["maxCaller"].Value.Trim() != string.Empty)
                        node.MaxCaller = Convert.ToInt32(childattribute["maxCaller"].Value);

                    //处理默认的服务
                    if (string.IsNullOrEmpty(defaultKey))
                    {
                        defaultKey = node.Key;
                    }

                    if (nodes.ContainsKey(node.Key))
                        throw new WarningException("Already exists server node 【" + node.Key + "】.");

                    nodes[node.Key] = node;
                }
            }

            if (type != CastleFactoryType.Local)
            {
                //如果app名称为空
                if (string.IsNullOrEmpty(appname))
                {
                    throw new WarningException("App name must be provided.");
                }

                //判断是否配置了服务信息
                if (string.IsNullOrEmpty(proxyServer))
                {
                    if (type != CastleFactoryType.None)
                    {
                        if (nodes.Count == 0)
                            throw new WarningException("Not configure any server node.");

                        //判断是否包含默认的服务
                        if (!nodes.ContainsKey(defaultKey))
                        {
                            throw new WarningException("Not find the default service node 【" + defaultKey + "】.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public CastleFactoryType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets or sets the app name.
        /// </summary>
        /// <value>The host name.</value>
        public string AppName
        {
            get { return appname; }
            set { appname = value; }
        }

        /// <summary>
        /// Gets or sets the default
        /// </summary>
        /// <value>The default.</value>
        public string DefaultKey
        {
            get { return defaultKey; }
            set { defaultKey = value; }
        }

        /// <summary>
        /// Gets or sets the proxy
        /// </summary>
        /// <value>The proxy.</value>
        public string ProxyServer
        {
            get { return proxyServer; }
            set { proxyServer = value; }
        }

        /// <summary>
        /// Gets or sets the throwError
        /// </summary>
        /// <value>The throwError.</value>
        public bool ThrowError
        {
            get { return throwError; }
            set { throwError = value; }
        }

        /// <summary>
        /// Gets or sets the enableCache
        /// </summary>
        /// <value>The enableCache.</value>
        public bool EnableCache
        {
            get { return enableCache; }
            set { enableCache = value; }
        }

        /// <summary>
        /// Gets or sets the nodes
        /// </summary>
        /// <value>The nodes.</value>
        public IDictionary<string, ServerNode> Nodes
        {
            get { return nodes; }
        }
    }
}
