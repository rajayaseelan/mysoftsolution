using System;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 回调消息
    /// </summary>
    [Serializable]
    public class ScsCallbackMessage : ScsMessage
    {
        /// <summary>
        /// 回调消息
        /// </summary>
        public CallbackMessage MessageValue { get; private set; }

        public ScsCallbackMessage(CallbackMessage value)
        {
            this.MessageValue = value;
        }
    }
}
