using System;
using System.Diagnostics;

namespace MySoft.Logger
{
    /// <summary>
    /// A delegate used for log.
    /// </summary>
    /// <param name="log">The msg to write to log.</param>
    public delegate void LogEventHandler(string log, LogType type);

    /// <summary>
    /// Mark a implementing class as loggable.
    /// </summary>
    public interface ILogable
    {
        /// <summary>
        /// OnLog event.
        /// </summary>
        event LogEventHandler OnLog;
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 错误事件
        /// </summary>
        Error,
        /// <summary>
        /// 警告事件
        /// </summary>
        Warning,
        /// <summary>
        /// 信息事件
        /// </summary>
        Information
    }
}
