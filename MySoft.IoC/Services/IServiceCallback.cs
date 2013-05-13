using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务回调
    /// </summary>
    public interface IServiceCallback
    {
        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Connected(object sender, ConnectEventArgs e);

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Disconnected(object sender, ConnectEventArgs e);

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        void MessageCallback(string messageId, CallbackMessage message);

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        void MessageCallback(string messageId, ResponseMessage message);
    }
}
