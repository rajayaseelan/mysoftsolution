using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MySoft.RESTful.Auth
{
    /// <summary>
    /// 认证异常信息
    /// </summary>
    [Serializable]
    public class AuthenticationException : MySoftException
    {
        private string code;
        /// <summary>
        /// 错误Code
        /// </summary>
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        private HttpStatusCode statusCode;
        /// <summary>
        /// Http状态码
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        /// <summary>
        /// 实例化AuthenticationException
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public AuthenticationException(string code, string message)
            : base(message)
        {
            this.code = code;
        }

        /// <summary>
        /// 实例化AuthenticationException
        /// </summary>
        /// <param name="message"></param>
        public AuthenticationException(string message)
            : base(message)
        { }
    }
}
