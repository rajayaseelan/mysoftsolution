using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Net;

namespace MySoft.Auth
{
    /// <summary>
    /// 认证的Token信息
    /// </summary>
    public class AuthorizeToken
    {
        /// <summary>
        /// 请求的uri
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// 提交方法 （Post or Get）
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 头信息
        /// </summary>
        public WebHeaderCollection Headers { get; set; }

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
            this.Headers = new WebHeaderCollection();
            this.Parameters = new NameValueCollection();
            this.Cookies = new HttpCookieCollection();
        }
    }
}
