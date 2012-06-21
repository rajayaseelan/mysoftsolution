using System;
using MySoft.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// This message is used to send/receive a raw byte array as message data.
    /// </summary>
    [Serializable]
    public class ScsResultMessage : ScsMessage
    {
        /// <summary>
        /// Message data that is being transmitted.
        /// </summary>
        public MessageBase MessageValue { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ScsResultMessage()
        {

        }

        /// <summary>
        /// Creates a new ScsResultMessage object with Data property.
        /// </summary>
        /// <param name="Data">Message data that is being transmitted</param>
        public ScsResultMessage(MessageBase value)
        {
            this.MessageValue = value;
        }

        /// <summary>
        /// Creates a new reply ScsResultMessage object with Data property.
        /// </summary>
        /// <param name="Data">Message data that is being transmitted</param>
        /// <param name="repliedMessageId">
        /// Replied message id if this is a reply for
        /// a message.
        /// </param>
        public ScsResultMessage(MessageBase value, string repliedMessageId)
            : this(value)
        {
            RepliedMessageId = repliedMessageId;
        }

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? string.Format("ScsResultMessage [{0}]: ({1},{2})", MessageId, MessageValue.ServiceName, MessageValue.MethodName)
                       : string.Format("ScsResultMessage [{0}] Replied To [{1}]: ({2},{3})", MessageId, RepliedMessageId, MessageValue.ServiceName, MessageValue.MethodName);
        }
    }
}
