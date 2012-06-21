using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 回调信息
    /// </summary>
    internal class CallbackInfo
    {
        /// <summary>
        /// 客户端
        /// </summary>
        public IScsServerClient Client { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public CallbackMessage Message { get; set; }
    }
}
