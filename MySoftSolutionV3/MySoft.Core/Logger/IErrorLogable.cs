using System;

namespace MySoft.Logger
{
    /// <summary>
    /// A delegate used for exception.
    /// </summary>
    /// <param name="exception"></param>
    public delegate void ErrorLogEventHandler(Exception exception);

    /// <summary>
    /// Mark a implementing class as loggable.
    /// </summary>
    public interface IErrorLogable
    {
        /// <summary>
        /// OnError event.
        /// </summary>
        event ErrorLogEventHandler OnError;
    }
}
