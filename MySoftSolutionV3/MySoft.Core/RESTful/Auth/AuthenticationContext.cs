using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace MySoft.RESTful
{
    /// <summary>
    /// 认证的当前上下文对象
    /// </summary>
    public class AuthenticationContext
    {
        private AuthenticationUser user;
        /// <summary>
        /// 认证的用户信息
        /// </summary>
        public AuthenticationUser User
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
            }
        }

        private AuthenticationToken token;
        /// <summary>
        /// 获取认证的Token信息
        /// </summary>
        public AuthenticationToken Token
        {
            get
            {
                return token;
            }
        }

        /// <summary>
        /// 实例化认证上下文
        /// </summary>
        /// <param name="token"></param>
        public AuthenticationContext(AuthenticationToken token)
        {
            this.token = token;
        }

        /// <summary>
        /// 认证的当前上下文
        /// </summary>
        public static AuthenticationContext Current
        {
            get
            {
                return CallContext.HostContext as AuthenticationContext;
            }
            set
            {
                CallContext.HostContext = value;
            }
        }
    }
}
