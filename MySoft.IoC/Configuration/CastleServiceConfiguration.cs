using System;
using System.Configuration;
using System.Xml;

namespace MySoft.IoC.Configuration
{
    /// <summary>
    /// The service factory configuration.
    /// </summary>
    public class CastleServiceConfiguration : ConfigurationBase
    {
        private string host = "any";
        private int port = 8888;
        private int httpPort = 8080;
        private bool httpEnabled = false;
        private Type apiResolverType;
        private Type nodeResolverType;
        private bool compress = false;
        private int recordHours = ServiceConfig.DEFAULT_RECORD_HOUR;            //默认记录6小时
        private int timeout = ServiceConfig.DEFAULT_SERVER_CALL_TIMEOUT;        //超时时间为60秒
        private int maxCaller = ServiceConfig.DEFAULT_SERVER_MAX_CALLER;        //默认并发数为20
        private bool enableCache = false;                                       //是否缓存

        /// <summary>
        /// 获取远程对象配置
        /// </summary>
        /// <returns></returns>
        public static CastleServiceConfiguration GetConfig()
        {
            string key = "mysoft.framework/castleService";
            CastleServiceConfiguration obj = CacheHelper.Get<CastleServiceConfiguration>(key);
            if (obj == null)
            {
                var tmp = ConfigurationManager.GetSection(key);
                obj = tmp as CastleServiceConfiguration;
                CacheHelper.Insert(key, obj, 60);
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

            if (attribute["host"] != null && attribute["host"].Value.Trim() != string.Empty)
                host = attribute["host"].Value;

            if (attribute["port"] != null && attribute["port"].Value.Trim() != string.Empty)
                port = Convert.ToInt32(attribute["port"].Value);

            if (attribute["compress"] != null && attribute["compress"].Value.Trim() != string.Empty)
                compress = Convert.ToBoolean(attribute["compress"].Value);

            if (attribute["recordHours"] != null && attribute["recordHours"].Value.Trim() != string.Empty)
                recordHours = Convert.ToInt32(attribute["recordHours"].Value);

            if (attribute["timeout"] != null && attribute["timeout"].Value.Trim() != string.Empty)
                timeout = Convert.ToInt32(attribute["timeout"].Value);

            if (attribute["maxCaller"] != null && attribute["maxCaller"].Value.Trim() != string.Empty)
                maxCaller = Convert.ToInt32(attribute["maxCaller"].Value);

            if (attribute["enableCache"] != null && attribute["enableCache"].Value.Trim() != string.Empty)
                enableCache = Convert.ToBoolean(attribute["enableCache"].Value);

            foreach (XmlNode child in xmlnode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment) continue;

                XmlAttributeCollection childattribute = child.Attributes;
                if (child.Name == "httpServer")
                {
                    httpPort = Convert.ToInt32(childattribute["port"].Value);
                    httpEnabled = Convert.ToBoolean(childattribute["enabled"].Value);
                }
                else if (child.Name == "apiResolver") //加载api解析器
                {
                    try
                    {
                        var typeName = childattribute["type"].Value;
                        apiResolverType = Type.GetType(typeName);
                    }
                    catch (Exception ex)
                    {
                        //TODO
                    }
                }
                else if (child.Name == "nodeResolver") //加载node解析器
                {
                    try
                    {
                        var typeName = childattribute["type"].Value;
                        nodeResolverType = Type.GetType(typeName);
                    }
                    catch (Exception ex)
                    {
                        //TODO
                    }
                }
            }
        }

        #region Http参数

        /// <summary>
        /// Gets or sets the httpport
        /// </summary>
        public int HttpPort
        {
            get { return httpPort; }
            set { httpPort = value; }
        }

        /// <summary>
        /// Gets or sets the httpenabled
        /// </summary>
        public bool HttpEnabled
        {
            get { return httpEnabled; }
            set { httpEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the apiResolverType
        /// </summary>
        public Type ApiResolverType
        {
            get { return apiResolverType; }
            set { apiResolverType = value; }
        }

        /// <summary>
        /// Gets or sets the nodeResolverType
        /// </summary>
        public Type NodeResolverType
        {
            get { return nodeResolverType; }
            set { nodeResolverType = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>The host.</value>
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// Gets or sets the compress.
        /// </summary>
        /// <value>The format.</value>
        public bool Compress
        {
            get { return compress; }
            set { compress = value; }
        }

        /// <summary>
        /// Gets or sets the recordHours
        /// </summary>
        /// <value>The recordHours.</value>
        public int RecordHours
        {
            get { return recordHours; }
            set { recordHours = value; }
        }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Gets or sets the maxCaller
        /// </summary>
        public int MaxCaller
        {
            get { return maxCaller; }
            set { maxCaller = value; }
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
    }
}
