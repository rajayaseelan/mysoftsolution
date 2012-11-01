using System;
using System.Linq;
using System.Text;
using System.Web;

namespace MySoft.RESTful
{
    /// <summary>
    /// 认证User结果
    /// </summary>
    [Serializable]
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
