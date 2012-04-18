using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Logger;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 执行并输出日志的接口
    /// </summary>
    public interface IServiceLog : ILog
    {
        /// <summary>
        /// 开始执行命令
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="parameters"></param>
        void Begin(CallMessage reqMsg, ParameterCollection parameters);

        /// <summary>
        /// 结束执行命令
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="parameters"></param>
        /// <param name="elapsedTime"></param>
        void End(ReturnMessage resMsg, ParameterCollection parameters, long elapsedTime);
    }
}
