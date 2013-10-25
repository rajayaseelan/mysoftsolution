using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 服务响应事件参数
    /// </summary>
    public class ResponseMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 请求异常信息
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 响应的消息
        /// </summary>
        public ResponseMessage Response { get; set; }
    }
}
