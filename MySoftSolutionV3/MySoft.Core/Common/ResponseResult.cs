using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace MySoft
{
    /// <summary>
    /// 响应返回
    /// </summary>
    [Serializable]
    public class ResponseResult
    {
        /// <summary>
        /// 返回的代码（用于自定义代码）
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 返回的消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 实例化ResponseResult
        /// </summary>
        public ResponseResult()
        {
            this.Message = "返回成功！";
        }

        /// <summary>
        ///  实例化ResponseResult
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ResponseResult(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }
    }

    /// <summary>
    /// 响应返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ResponseResult<T> : ResponseResult
    {
        /// <summary>
        /// 返回的结果
        /// </summary>
        public new T Result
        {
            get { return (T)base.Result; }
            set { base.Result = value; }
        }

        /// <summary>
        /// 实体化DataResult
        /// </summary>
        public ResponseResult()
            : base()
        { }

        /// <summary>
        ///  实例化ResponseResult
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ResponseResult(int code, string message)
            : base(code, message)
        { }
    }
}
