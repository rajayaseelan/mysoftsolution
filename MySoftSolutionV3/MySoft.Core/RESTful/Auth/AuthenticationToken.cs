using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Net;

namespace MySoft.RESTful
{
    /// <summary>
    /// 认证Token信息
    /// </summary>
    [Serializable]
    public class AuthenticationToken
    {
        /// <summary>
        /// 请求的uri
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// 传入的参数
        /// </summary>
        public NameValueCollection Parameters { get; set; }

        /// <summary>
        /// Cookie信息
        /// </summary>
        public HttpCookieCollection Cookies { get; set; }

        /// <summary>
        /// 提交方法 （Post or Get）
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 实例化AuthenticationToken
        /// </summary>
        /// <param name="requestUri"></param>
        public AuthenticationToken(Uri requestUri)
        {
            this.RequestUri = requestUri;
            this.Cookies = new HttpCookieCollection();
            this.Parameters = new NameValueCollection();
            this.Method = "POST";
        }

        /// <summary>
        /// 实例化AuthenticationToken
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="parameters"></param>
        public AuthenticationToken(Uri requestUri, NameValueCollection parameters)
            : this(requestUri)
        {
            this.Parameters = parameters;
        }

        /// <summary>
        /// 实例化AuthenticationToken
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="parameters"></param>
        /// <param name="method"></param>
        public AuthenticationToken(Uri requestUri, NameValueCollection parameters, string method)
            : this(requestUri, parameters)
        {
            this.Method = method;
        }
    }
}
