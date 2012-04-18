using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 消息基类
    /// </summary>
    [Serializable]
    public class ServiceMessage
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }
    }

    /// <summary>
    /// 调用消息
    /// </summary>
    [Serializable]
    public sealed class CallMessage : ServiceMessage
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 主机名
        /// </summary>
        public string HostName { get; set; }
    }
}
