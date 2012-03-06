using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace MySoft.Auth
{
    /// <summary>
    /// 认证的当前上下文对象
    /// </summary>
    public class AuthorizeContext
    {
        private AuthorizeResult result;
        /// <summary>
        /// 认证结果信息
        /// </summary>
        public AuthorizeResult Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
            }
        }

        private AuthorizeToken token;
        /// <summary>
        /// 认证的token信息
        /// </summary>
        public AuthorizeToken Token
        {
            get
            {
                return token;
            }
            set
            {
                token = value;
            }
        }

        /// <summary>
        /// 认证的当前上下文
        /// </summary>
        public static AuthorizeContext Current
        {
            get
            {
                return CallContext.HostContext as AuthorizeContext;
            }
            set
            {
                CallContext.HostContext = value;
            }
        }
    }
}
