using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// 
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
            this.Message = null;
            this.Buffer = null;
        }

        #endregion
    }
}
