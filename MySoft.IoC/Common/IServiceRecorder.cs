using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Logger
{
    /// <summary>
    /// 服务日志记录
    /// </summary>
    public interface IServiceRecorder
    {
        /// <summary>
        /// 记录服务请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Call(object sender, CallEventArgs e);
    }
}
