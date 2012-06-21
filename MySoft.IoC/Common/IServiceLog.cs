using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Logger
{
    /// <summary>
    /// 执行并输出日志的接口
    /// </summary>
    public interface IServiceLog
    {
        /// <summary>
        /// 开始执行命令
        /// </summary>
        /// <param name="reqMsg"></param>
        void Begin(CallMessage reqMsg);

        /// <summary>
        /// 结束执行命令
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedTime"></param>
        void End(CallMessage reqMsg, ReturnMessage resMsg, long elapsedTime);
    }
}
