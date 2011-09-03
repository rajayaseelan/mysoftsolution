using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 连接信息
    /// </summary>
    [Serializable]
    public class ConnectInfo
    {
        private string ip;
        /// <summary>
        /// 连接的IP
        /// </summary>
        public string IP
        {
            get { return ip; }
            set { ip = value; }
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
