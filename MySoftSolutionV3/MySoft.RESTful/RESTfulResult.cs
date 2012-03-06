using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful
{
    /// <summary>
    /// RESTful结果
    /// </summary>
    [Serializable]
    public class RESTfulResult
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

    /// <summary>
    /// RESTful响应
    /// </summary>
    [Serializable]
    public class RESTfulResponse
    {
        public object Value { get; set; }
    }
}
