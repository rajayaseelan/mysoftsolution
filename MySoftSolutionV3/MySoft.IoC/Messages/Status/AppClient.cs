using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    [Serializable]
    public class AppClient
    {
        /// <summary>
        /// 应用路径
        /// </summary>
        public string AppPath { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string HostName { get; set; }
    }
}
