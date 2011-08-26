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
    public class RemoteNode
    {
        private string ip;
        private int port;
        private string key;
        private int maxpool = ServiceConfig.DEFAULT_CLIENTPOOL_NUMBER;

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
        /// Gets or sets the maxpool.
        /// </summary>
        /// <value>The maxpool.</value>
        public int MaxPool
        {
            get { return maxpool; }
            set { maxpool = value; }
        }
    }
}
