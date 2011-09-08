using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using MySoft.Communication.Scs.Client;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 服务响应事件参数
    /// </summary>
    public class ServiceMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 响应的消息
        /// </summary>
        public ResponseMessage Result { get; set; }

        /// <summary>
        /// 返回通讯的Client对象
        /// </summary>
        public IScsClient Client { get; set; }
    }
}
