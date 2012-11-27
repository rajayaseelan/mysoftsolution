using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// 通道完成接口
    /// </summary>
    internal interface ICommunicationCompleted
    {
        /// <summary>
        /// IO完成事件
        /// </summary>
        /// <param name="e"></param>
        void IOCompleted(SocketAsyncEventArgs e);
    }
}