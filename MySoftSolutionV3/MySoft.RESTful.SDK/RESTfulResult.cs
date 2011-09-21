using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful.SDK
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
    /// RESTful响应结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RESTfulResponse<T>
    {
        /// <summary>
        /// 响应结果
        /// </summary>
        public T Result { get; set; }
    }
}
