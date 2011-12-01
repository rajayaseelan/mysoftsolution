using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MySoft.IoC.Configuration
{
    /// <summary>
    /// 远程节点
    /// </summary>
    [Serializable]
    public class RemoteNode
    {
        private string ip;
        private int port;
        private string key;
        private bool encrypt = false;
        private bool compress = false;
        private int timeout = ServiceConfig.DEFAULT_CLIENT_TIMEOUT;
        private int maxpool = ServiceConfig.DEFAULT_CLIENTPOOL_MAXNUMBER;

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
        /// <value>The minpool.</value>
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
        /// 返回一个远程节点
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static RemoteNode Parse(string ip, int port)
        {
            return new RemoteNode { Key = string.Format("{0}:{1}", ip, port), IP = ip, Port = port };
        }

        /// <summary>
        /// 返回一个远程节点
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RemoteNode Parse(string value)
        {
            var strs = value.Split(':');
            return Parse(strs[0], Convert.ToInt32(strs[1]));
        }
    }
}
