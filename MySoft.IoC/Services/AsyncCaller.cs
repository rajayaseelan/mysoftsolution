using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用服务
    /// </summary>
    /// <param name="service"></param>
    /// <param name="context"></param>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncCaller(IService service, OperationContext context, RequestMessage reqMsg);

    /// <summary>
    /// 任务信息
    /// </summary>
    internal class TaskInfo
    {
        /// <summary>
        /// 线程
        /// </summary>
        public Thread Thread { get; set; }

        /// <summary>
        /// 信号量
        /// </summary>
        public WaitHandle WaitHandle { get; set; }

        //当前句柄
        public RegisteredWaitHandle RegisteredHandle { get; set; }
    }
}
