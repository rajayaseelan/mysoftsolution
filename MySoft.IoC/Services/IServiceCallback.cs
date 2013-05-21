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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MessageCallback(object sender, CallbackMessageEventArgs e);

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MessageCallback(object sender, ResponseMessageEventArgs e);
    }
}
