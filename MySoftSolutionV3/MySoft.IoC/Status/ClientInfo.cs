using System;
using System.Collections.Generic;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 客户端连接信息
    /// </summary>
    [Serializable]
    public class ClientInfo : AppClient
    {
        /// <summary>
        /// 服务端IP
        /// </summary>
        public string ServerIPAddress { get; set; }

        /// <summary>
        /// 服务端Port
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// 连接数
        /// </summary>
        public int Count { get; set; }
    }
}
