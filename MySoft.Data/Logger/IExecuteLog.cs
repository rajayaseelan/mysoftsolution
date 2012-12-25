using System;
using System.Data;
using MySoft.Logger;
using System.Data.Common;

namespace MySoft.Data.Logger
{
    /// <summary>
    /// 执行并输出日志的接口
    /// </summary>
    public interface IExecuteLog
    {
        /// <summary>
        /// 开始执行命令，返回是否需要执行
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        void Begin(DbCommand command);

        /// <summary>
        /// 结束执行命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="retValue"></param>
        /// <param name="elapsedTime"></param>
        void End(DbCommand command, ReturnValue retValue, long elapsedTime);
    }
}
