using System;
using System.Linq;
using System.Text;
using System.Web;

namespace MySoft.RESTful
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// System
        /// </summary>
        System,
        /// <summary>
        /// User
        /// </summary>
        User
    }

    /// <summary>
    /// 认证User结果
    /// </summary>
    public class AuthorizeUser
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public object UserState { get; set; }
    }
}
