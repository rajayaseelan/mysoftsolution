using System;
using System.Linq;
using System.Text;
using System.Web;

namespace MySoft.Auth
{
    /// <summary>
    /// 认证Token结果
    /// </summary>
    public class AuthorizeToken
    {
        /// <summary>
        /// 认证是否成功
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 认证名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        /// 实例化AuthorizeToken
        /// </summary>
        public AuthorizeToken()
        {
            this.Succeed = false;
        }
    }
}
