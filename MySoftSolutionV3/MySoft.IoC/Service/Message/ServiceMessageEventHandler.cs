using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Message
{
    /// <summary>
    /// 服务消息委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ServiceMessageEventHandler(object sender, ServiceMessageEventArgs args);
}
