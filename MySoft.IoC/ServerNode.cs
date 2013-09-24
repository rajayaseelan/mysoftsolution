using System;
using System.Xml.Serialization;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务器节点
    /// </summary>
    [Serializable]
    [XmlRoot("serverNode")]
    public class ServerNode
    {
        private string ip = "127.0.0.1";
        private int port = 8888;
        private string key = "default";
        private bool compress = false;
        private int maxCaller = ServiceConfig.DEFAULT_CLIENT_MAXCALLER;
        private int timeout = ServiceConfig.DEFAULT_CLIENT_TIMEOUT;
        private ResponseType resptype = ResponseType.Binary;

        public ServerNode()
        {
            this.Connected = true;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The name.</value>
        [XmlAttribute("key")]
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    return string.Format("{0}:{1}", ip, port);
                }

                return key;
            }
            set { key = value; }
        }

        /// <summary>
        /// Gets or sets the ip.
        /// </summary>
        /// <value>The server.</value>
        [XmlAttribute("ip")]
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        [XmlAttribute("port")]
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// Gets or sets the compress.
        /// </summary>
        /// <value>The format.</value>
        [XmlElement("compress")]
        public bool Compress
        {
            get { return compress; }
            set { compress = value; }
        }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout ：单位（秒）.</value>
        [XmlElement("timeout")]
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Gets or sets the maxCaller.
        /// </summary>
        [XmlElement("maxCaller")]
        public int MaxCaller
        {
            get { return maxCaller; }
            set { maxCaller = value; }
        }

        /// <summary>
        /// Gets or sets the resptype
        /// </summary>
        /// <value>The resptype.</value>
        [XmlElement("resptype")]
        public ResponseType RespType
        {
            get { return resptype; }
            set { resptype = value; }
        }

        /// <summary>
        /// 是否可连接
        /// </summary>
        internal bool Connected { get; set; }

        /// <summary>
        /// 返回一个远程节点
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static ServerNode Parse(string ip, int port)
        {
            return new ServerNode { Key = string.Format("{0}:{1}", ip, port), IP = ip, Port = port };
        }

        /// <summary>
        /// 返回一个远程节点
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ServerNode Parse(string value)
        {
            var strs = value.Split(':');
            return Parse(strs[0], Convert.ToInt32(strs[1]));
        }

        /// <summary>
        /// 返回字符串形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", ip, port);
        }
    }
}
