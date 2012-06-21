using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MySoft.Auth
{
    /// <summary>
    /// 认证异常信息
    /// </summary>
    [Serializable]
    public class AuthorizeException : MySoftException
    {
        private int code;
        /// <summary>
        /// 错误Code
        /// </summary>
        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        /// <summary>
        /// 实例化AuthorizeException
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public AuthorizeException(int code, string message)
            : base(message)
        {
            this.code = code;
        }

        /// <summary>
        /// 实例化AuthenticationException
        /// </summary>
        /// <param name="message"></param>
        public AuthorizeException(string message)
            : base(message)
        { }
    }
}
