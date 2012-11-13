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
    public interface IServiceCall
    {
        /// <summary>
        /// 记录服务请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Recorder(object sender, CallEventArgs e);
    }
}
