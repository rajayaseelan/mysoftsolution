using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// Message usertoken
    /// </summary>
    internal class MessageUserToken
    {
        /// <summary>
        /// Send message value.
        /// </summary>
        public IScsMessage Message { get; private set; }

        /// <summary>
        /// Send message buffer.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageBytes"></param>
        public MessageUserToken(IScsMessage message, byte[] messageBytes)
        {
            this.Message = message;
            this.Buffer = messageBytes;
        }
    }
}
