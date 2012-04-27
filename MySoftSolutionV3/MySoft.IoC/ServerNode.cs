using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务器节点
    /// </summary>
    [Serializable]
    public class ServerNode
    {
        private string ip;
        private int port;
        private string key;
        private bool encrypt = false;
        private bool compress = false;
        private int timeout = ServiceConfig.DEFAULT_CLIENT_TIMEOUT;
        private int maxpool = ServiceConfig.DEFAULT_CLIENT_MAXPOOL;
        private TransferType format = TransferType.Binary;

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The name.</value>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        /// <summary>
        /// Gets or sets the ip.
        /// </summary>
        /// <value>The server.</value>
        public string IP
        {
            get { return ip; }
            set { ip = value; }
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
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout ：单位（秒）.</value>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Gets or sets the maxpool.
        /// </summary>
        /// <value>The maxpool.</value>
        public int MaxPool
        {
            get { return maxpool; }
            set { maxpool = value; }
        }

        /// <summary>
        /// Gets or sets the format
        /// </summary>
        /// <value>The format.</value>
        public TransferType Format
        {
            get { return format; }
            set { format = value; }
        }

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
    }
}
