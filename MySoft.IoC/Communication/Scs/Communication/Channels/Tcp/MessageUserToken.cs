using System;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// Message usertoken
    /// </summary>
    internal class MessageUserToken : IDisposable
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
        /// 实例化MessageUserToken
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageBytes"></param>
        public MessageUserToken(IScsMessage message, byte[] messageBytes)
        {
            this.Message = message;
            this.Buffer = messageBytes;
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose resource.
        /// </summary>
        public void Dispose()
        {
            this.Buffer = null;
            this.Message = null;
        }

        #endregion
    }
}
