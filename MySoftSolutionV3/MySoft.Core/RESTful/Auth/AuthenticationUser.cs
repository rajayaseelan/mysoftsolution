using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful.Auth
{
    /// <summary>
    /// 认证用户
    /// </summary>
    [Serializable]
    public class AuthenticationUser
    {
        /// <summary>
        /// 认证用户ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 认证用户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 认证用户的手机
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 认证用户邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
