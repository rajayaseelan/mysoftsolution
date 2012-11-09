using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 调用上下文
    /// </summary>
    internal class CallerContext
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Caller信息
        /// </summary>
        public AppCaller Caller { get; set; }

        /// <summary>
        /// 通道信息
        /// </summary>
        public IScsServerClient Channel { get; set; }

        /// <summary>
        /// 请求信息
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// 响应信息
        /// </summary>
        public ResponseMessage Message { get; set; }
    }
}
