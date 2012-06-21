using System;

namespace MySoft.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// Stores message to be used by an event.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Message object that is associated with this event.
        /// </summary>
        public IScsMessage Message { get; private set; }

        /// <summary>
        /// Creates a new MessageEventArgs object.
        /// </summary>
        /// <param name="message">Message object that is associated with this event</param>
        public MessageEventArgs(IScsMessage message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Error message args.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Message error.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Creates a new MessageEventArgs object.
        /// </summary>
        /// <param name="message">Message object that is associated with this event</param>
        public ErrorEventArgs(Exception error)
        {
            Error = error;
        }
    }
}
