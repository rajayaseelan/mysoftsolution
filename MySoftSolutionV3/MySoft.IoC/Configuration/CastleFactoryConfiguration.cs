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
        private IDictionary<string, RemoteNode> nodes = new Dictionary<string, RemoteNode>();
        private CastleFactoryType type = CastleFactoryType.Local;
        private string defaultKey;              //默认服务
        private string appName;                 //host名称
        private bool throwerror = true;         //抛出异常
        private int times = 3;                  //调用次数
        private bool json = false;              //是否json输入输出

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
        /// <param name="node"></param>
        public void LoadValuesFromConfigurationXml(XmlNode node)
        {
            if (node == null) return;

            XmlAttributeCollection xmlnode = node.Attributes;

            if (xmlnode["type"] != null && xmlnode["type"].Value.Trim() != string.Empty)
                type = (CastleFactoryType)Enum.Parse(typeof(CastleFactoryType), xmlnode["type"].Value, true);

            if (xmlnode["times"] != null && xmlnode["times"].Value.Trim() != string.Empty)
                times = Convert.ToInt32(xmlnode["times"].Value);

            if (xmlnode["throwerror"] != null && xmlnode["throwerror"].Value.Trim() != string.Empty)
                throwerror = Convert.ToBoolean(xmlnode["throwerror"].Value);

            if (xmlnode["json"] != null && xmlnode["json"].Value.Trim() != string.Empty)
                json = Convert.ToBoolean(xmlnode["json"].Value);

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

                    //超时时间，默认为1分钟
                    if (childnode["timeout"] != null && childnode["timeout"].Value.Trim() != string.Empty)
                        remoteNode.Timeout = Convert.ToInt32(childnode["timeout"].Value);

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
        /// Gets or sets the times
        /// </summary>
        public int Times
        {
            get { return times; }
            set { times = value; }
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
        /// Gets or sets the json
        /// </summary>
        /// <value>The throwerror.</value>
        public bool Json
        {
            get { return json; }
            set { json = value; }
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
