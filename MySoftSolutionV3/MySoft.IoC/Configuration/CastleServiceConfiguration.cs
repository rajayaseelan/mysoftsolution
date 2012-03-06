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
        private int httpport = 8080;
        private bool httpenabled = false;
        private string assembly;
        private bool encrypt = false;
        private bool compress = false;
        private int minuteCalls = ServiceConfig.DEFAULT_MINUTE_CALL_NUMBER;         //默认为每分钟调用1000次，超过报异常
        private int recordNums = ServiceConfig.DEFAULT_RECORD_NUMBER;              //默认记录3600次   //记录条数，默认为3600条，1小时记录

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

            if (xmlnode["host"] != null && xmlnode["host"].Value.Trim() != string.Empty)
                host = xmlnode["host"].Value;

            if (xmlnode["port"] != null && xmlnode["port"].Value.Trim() != string.Empty)
                port = Convert.ToInt32(xmlnode["port"].Value);

            if (xmlnode["encrypt"] != null && xmlnode["encrypt"].Value.Trim() != string.Empty)
                encrypt = Convert.ToBoolean(xmlnode["encrypt"].Value);

            if (xmlnode["compress"] != null && xmlnode["compress"].Value.Trim() != string.Empty)
                compress = Convert.ToBoolean(xmlnode["compress"].Value);

            if (xmlnode["recordNums"] != null && xmlnode["recordNums"].Value.Trim() != string.Empty)
                recordNums = Convert.ToInt32(xmlnode["recordNums"].Value);

            if (xmlnode["minuteCalls"] != null && xmlnode["minuteCalls"].Value.Trim() != string.Empty)
                minuteCalls = Convert.ToInt32(xmlnode["minuteCalls"].Value);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment) continue;

                XmlAttributeCollection childnode = child.Attributes;
                if (child.Name == "httpServer")
                {
                    httpport = Convert.ToInt32(childnode["port"].Value);
                    httpenabled = Convert.ToBoolean(childnode["enabled"].Value);
                }
                else if (child.Name == "serverCache")
                {
                    if (childnode["asselbly"] != null && childnode["assembly"].Value.Trim() != string.Empty)
                        assembly = childnode["assembly"].Value;
                }
            }
        }

        #region Http参数

        /// <summary>
        /// Gets or sets the httpport
        /// </summary>
        public int HttpPort
        {
            get { return httpport; }
            set { httpport = value; }
        }

        /// <summary>
        /// Gets or sets the httpenabled
        /// </summary>
        public bool HttpEnabled
        {
            get { return httpenabled; }
            set { httpenabled = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the assembly
        /// </summary>
        public string AssemblyName
        {
            get { return assembly; }
            set { assembly = value; }
        }

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
        /// Gets or sets the encrypt.
        /// </summary>
        /// <value>The encrypt.</value>
        public bool Encrypt
        {
            get { return encrypt; }
            set { encrypt = value; }
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
        /// Gets or sets the recordNums
        /// </summary>
        /// <value>The recordNums.</value>
        public int RecordNums
        {
            get { return recordNums; }
            set { recordNums = value; }
        }

        /// <summary>
        /// Gets or sets the minuteCalls
        /// </summary>
        public int MinuteCalls
        {
            get { return minuteCalls; }
            set { minuteCalls = value; }
        }
    }
}
