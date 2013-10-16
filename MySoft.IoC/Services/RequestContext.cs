using MySoft.IoC.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 请求上下文
    /// </summary>
    internal class RequestContext
    {
        /// <summary>
        /// 操作上下文
        /// </summary>
        public OperationContext Context { get; set; }

        /// <summary>
        /// 请求对象
        /// </summary>
        public RequestMessage Request { get; set; }
    }

    /// <summary>
    /// 异步上下文
    /// </summary>
    internal class AsyncContext : RequestContext
    {
        /// <summary>
        /// Queue manager.
        /// </summary>
        public QueueManager Manager { get; set; }
    }
}
