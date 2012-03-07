using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// http服务结果
    /// </summary>
    [Serializable]
    public class HttpServiceResult
    {
        /// <summary>
        /// 代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
