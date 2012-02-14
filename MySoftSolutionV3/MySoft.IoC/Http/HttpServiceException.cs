using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Http
{
    /// <summary>
    /// http服务异常
    /// </summary>
    [Serializable]
    public class HttpServiceException
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
