using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Logger
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Writes the log.
        /// </summary>
        /// <param name="log">The log info.</param>
        void Write(string log, LogType type);

        /// <summary>
        /// Writes the exception.
        /// </summary>
        /// <param name="exception">The exception info.</param>
        void Write(Exception error);
    }
}
