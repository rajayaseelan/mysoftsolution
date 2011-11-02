using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// App客户端信息
    /// </summary>
    [Serializable]
    public class ClientInfo
    {
        private string appName;
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName
        {
            get { return appName; }
            set { appName = value; }
        }

        private IList<ConnectionInfo> connections;
        /// <summary>
        /// 客户端连接信息
        /// </summary>
        public IList<ConnectionInfo> Connections
        {
            get { return connections; }
            set { connections = value; }
        }

        public ClientInfo()
        {
            this.connections = new List<ConnectionInfo>();
        }
    }

    /// <summary>
    /// 连接信息
    /// </summary>
    [Serializable]
    public class ConnectionInfo
    {
        private string ipAddress;
        /// <summary>
        /// 连接的IP
        /// </summary>
        public string IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        private string hostName;
        /// <summary>
        /// 主机名称
        /// </summary>
        public string HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }

        private int count;
        /// <summary>
        /// 连接数
        /// </summary>
        public int Count
        {
            get { return count; }
            set { count = value; }
        }
    }
}
