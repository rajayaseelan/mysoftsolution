using System;
using Hik.Communication.Scs.Client;

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
        public object Result { get; set; }

        /// <summary>
        /// 请求消息
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// 返回通讯的Client对象
        /// </summary>
        public IScsClient Client { get; set; }
    }

    /// <summary>
    /// 错误消息事件参数
    /// </summary>
    public class ErrorMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 请求信息
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public Exception Error { get; set; }
    }
}
