using System;
using System.Linq;
using System.Text;
using System.Web;

namespace MySoft.Auth
{
    /// <summary>
    /// 认证结果
    /// </summary>
    public class AuthorizeResult
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
        /// 实例化AuthorizeResult
        /// </summary>
        public AuthorizeResult()
        {
            this.Succeed = false;
        }
    }
}
