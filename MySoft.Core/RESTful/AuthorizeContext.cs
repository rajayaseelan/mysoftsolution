using System;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;

namespace MySoft.RESTful
{
    /// <summary>
    /// 认证的当前上下文对象
    /// </summary>
    public class AuthorizeContext : AuthorizeUser
    {
        private AuthorizeToken token;
        /// <summary>
        /// 认证的Token信息
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
                string name = typeof(AuthorizeContext).FullName;
                return CallContext.GetData(name) as AuthorizeContext;
            }
            set
            {
                string name = typeof(AuthorizeContext).FullName;
                if (value == null)
                    CallContext.FreeNamedDataSlot(name);
                else
                    CallContext.SetData(name, value);
            }
        }
    }
}
