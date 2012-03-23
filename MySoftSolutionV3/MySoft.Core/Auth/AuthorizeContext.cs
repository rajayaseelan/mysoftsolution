using System.Runtime.Remoting.Messaging;
using System.Threading;

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
                string name = string.Format("AuthorizeContext_{0}", Thread.CurrentThread.ManagedThreadId);
                return CallContext.GetData(name) as AuthorizeContext;
            }
            set
            {
                string name = string.Format("AuthorizeContext_{0}", Thread.CurrentThread.ManagedThreadId);
                CallContext.SetData(name, value);
            }
        }
    }
}
