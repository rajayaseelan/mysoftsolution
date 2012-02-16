using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft
{
    /// <summary>
    /// Base64的url帮助类
    /// </summary>
    public class Base64UrlHelper
    {
        /// <summary>
        /// String 转换  UrlBase64，使之适合在url中使用
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToUrlBase64(string str)
        {
            return StringToUrlBase64(str, Encoding.Default);
        }

        /// <summary>
        /// UrlBase64 转换  String
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlBase64ToString(string urlBase64str)
        {
            return UrlBase64ToString(urlBase64str, Encoding.Default);
        }

        /// <summary>
        /// String 转换  UrlBase64，使之适合在url中使用
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToUrlBase64(string str, Encoding enc)
        {
            var base64str = Convert.ToBase64String(enc.GetBytes(str));
            return Base64ToUrlBase64(base64str);
        }

        /// <summary>
        /// UrlBase64 转换  String
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlBase64ToString(string urlBase64str, Encoding enc)
        {
            var base64str = UrlBase64ToBase64(urlBase64str);
            return enc.GetString(Convert.FromBase64String(base64str));
        }

        /// <summary>
        /// Base64 转换 UrlBase64，使之适合在url中使用
        /// </summary>
        public static string Base64ToUrlBase64(string base64str)
        {
            // "+" 换成 "-A"
            // "/" 换成 "-S"
            // 去掉 "="
            return base64str.Replace("+", "-A").Replace("/", "-S").Replace("=", string.Empty);
        }

        /// <summary>
        /// UrlBase64 转换 Base64
        /// </summary>
        public static string UrlBase64ToBase64(string urlBase64str)
        {
            // "-A" 换成 "+"
            // "-S" 换成 "/"
            string str = urlBase64str.Replace("-A", "+").Replace("-S", "/");

            // 添加"="
            int mod = str.Length % 4;
            if (mod != 0)
            {
                str += new string('=', 4 - mod);
            }
            return str;
        }
    }
}
