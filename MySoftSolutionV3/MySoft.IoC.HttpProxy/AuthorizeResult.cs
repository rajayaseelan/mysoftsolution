using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace MySoft.IoC.HttpProxy
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

    /// <summary>
    /// 认证的Token信息
    /// </summary>
    public class AuthorizeToken
    {
        /// <summary>
        /// 参数信息
        /// </summary>
        public NameValueCollection Parameters { get; set; }

        /// <summary>
        /// Cookie信息
        /// </summary>
        public HttpCookieCollection Cookies { get; set; }

        /// <summary>
        /// 实例化AuthorizeToken
        /// </summary>
        public AuthorizeToken()
        {
            this.Parameters = new NameValueCollection();
            this.Cookies = new HttpCookieCollection();
        }
    }
}
