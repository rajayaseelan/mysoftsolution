using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    [Serializable]
    public class AppClient
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }
    }

    /// <summary>
    /// 客户端信息
    /// </summary>
    [Serializable]
    public class ScsClientMessage : ScsMessage
    {
        private AppClient client;
        /// <summary>
        /// 客户端信息
        /// </summary>
        public AppClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// 客户端信息
        /// </summary>
        /// <param name="client"></param>
        public ScsClientMessage(AppClient client)
        {
            this.client = client;
        }
    }
}
