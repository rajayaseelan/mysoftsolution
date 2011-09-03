using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using MySoft.IoC;

namespace MySoft.IoC.Configuration
{
    /// <summary>
    /// The service factory configuration.
    /// </summary>
    public class CastleFactoryConfiguration : ConfigurationBase
    {
        private IDictionary<string, RemoteNode> nodes = new Dictionary<string, RemoteNode>();
        private CastleFactoryType type = CastleFactoryType.Local;
        private string defaultKey;              //默认服务
        private string appName;                 //host名称
        private bool throwerror = true;         //抛出异常
        private double logtime = ServiceConfig.DEFAULT_LOGTIME_NUMBER;       //超时多长输出日志，默认为1秒
        private double timeout = ServiceConfig.DEFAULT_TIMEOUT_NUMBER;       //默认超时时间        30秒
        private double cachetime = ServiceConfig.DEFAULT_CACHETIME_NUMBER;   //默认缓存时间        60秒

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
                CacheHelper.Insert(key, obj, 60);
            }

            return obj;
        }

        /// <summary>
        /// 从配置文件加载配置值
        /// </summary>
        /// <param name="node"></param>
        public void LoadValuesFromConfigurationXml(XmlNode node)
        {
            if (node == null) return;

            XmlAttributeCollection xmlnode = node.Attributes;

            if (xmlnode["type"] != null && xmlnode["type"].Value.Trim() != string.Empty)
                type = (CastleFactoryType)Enum.Parse(typeof(CastleFactoryType), xmlnode["type"].Value, true);

            if (xmlnode["timeout"] != null && xmlnode["timeout"].Value.Trim() != string.Empty)
                timeout = Convert.ToDouble(xmlnode["timeout"].Value);

            if (xmlnode["cachetime"] != null && xmlnode["cachetime"].Value.Trim() != string.Empty)
                cachetime = Convert.ToDouble(xmlnode["cachetime"].Value);

            if (xmlnode["logtime"] != null && xmlnode["logtime"].Value.Trim() != string.Empty)
                logtime = Convert.ToDouble(xmlnode["logtime"].Value);

            if (xmlnode["throwerror"] != null && xmlnode["throwerror"].Value.Trim() != string.Empty)
                throwerror = Convert.ToBoolean(xmlnode["throwerror"].Value);

            if (xmlnode["default"] != null && xmlnode["default"].Value.Trim() != string.Empty)
                defaultKey = xmlnode["default"].Value;

            if (xmlnode["appname"] != null && xmlnode["appname"].Value.Trim() != string.Empty)
                appName = xmlnode["appname"].Value;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment) continue;

                XmlAttributeCollection childnode = child.Attributes;
                if (child.Name == "node")
                {
                    RemoteNode remoteNode = new RemoteNode();
                    remoteNode.Key = childnode["key"].Value;
                    remoteNode.IP = childnode["ip"].Value;
                    remoteNode.Port = Convert.ToInt32(childnode["port"].Value);

                    //最大连接池
                    if (childnode["maxpool"] != null && childnode["maxpool"].Value.Trim() != string.Empty)
                        remoteNode.MaxPool = Convert.ToInt32(childnode["maxpool"].Value);

                    if (childnode["encrypt"] != null && childnode["encrypt"].Value.Trim() != string.Empty)
                        remoteNode.Encrypt = Convert.ToBoolean(childnode["encrypt"].Value);

                    if (childnode["compress"] != null && childnode["compress"].Value.Trim() != string.Empty)
                        remoteNode.Compress = Convert.ToBoolean(childnode["compress"].Value);

                    //处理默认的服务
                    if (string.IsNullOrEmpty(defaultKey))
                    {
                        defaultKey = remoteNode.Key;
                    }

                    nodes.Add(remoteNode.Key, remoteNode);
                }
            }

            if (type == CastleFactoryType.Remote)
            {
                //如果app名称为空
                if (string.IsNullOrEmpty(appName))
                {
                    throw new WarningException("App name must be provided！");
                }

                //判断是否配置了服务信息
                if (nodes.Count == 0)
                {
                    throw new WarningException("Not configure any service node！");
                }

                //判断是否包含默认的服务
                if (!nodes.ContainsKey(defaultKey))
                {
                    throw new WarningException("Not find the default service node [" + defaultKey + "]！");
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
            get { return appName; }
            set { appName = value; }
        }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        /// <value>The timeout.</value>
        public double Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Gets or sets the cachetime
        /// </summary>
        /// <value>The cachetime.</value>
        public double CacheTime
        {
            get { return cachetime; }
            set { cachetime = value; }
        }

        /// <summary>
        /// Gets or sets the logtime
        /// </summary>
        /// <value>The logtime.</value>
        public double LogTime
        {
            get { return logtime; }
            set { logtime = value; }
        }

        /// <summary>
        /// Gets or sets the default
        /// </summary>
        /// <value>The default.</value>
        public string Default
        {
            get { return defaultKey; }
            set { defaultKey = value; }
        }

        /// <summary>
        /// Gets or sets the throwerror
        /// </summary>
        /// <value>The throwerror.</value>
        public bool ThrowError
        {
            get { return throwerror; }
            set { throwerror = value; }
        }

        /// <summary>
        /// Gets or sets the nodes
        /// </summary>
        /// <value>The nodes.</value>
        public IDictionary<string, RemoteNode> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }
    }
}
