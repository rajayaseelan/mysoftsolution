using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 服务响应事件参数
    /// </summary>
    public class CallbackMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 响应的消息
        /// </summary>
        public CallbackMessage Message { get; set; }
    }
}
