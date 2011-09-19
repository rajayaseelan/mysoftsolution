using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// Token参数
    /// </summary>
    public class TokenParameter
    {
        /// <summary>
        /// 认证Url
        /// </summary>
        public string AuthorizeUrl { get; set; }

        /// <summary>
        /// 签名Url
        /// </summary>
        public string SignatureUrl { get; set; }

        /// <summary>
        /// 访问TokenUrl
        /// </summary>
        public string AccessTokenUrl { get; set; }

        /// <summary>
        /// 消费方Key
        /// </summary>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// 消息方密钥
        /// </summary>
        public string ConsumerSecret { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 签名方式
        /// </summary>
        public string SignatureMethod { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; set; }

        public TokenParameter()
        {
            this.SignatureMethod = "HMAC-SHA1";
            this.Encoding = Encoding.UTF8;
        }
    }
}
