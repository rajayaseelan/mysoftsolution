using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful参数
    /// </summary>
    public class RESTfulParameter
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataFormat DataFormat { get; set; }

        /// <summary>
        /// 认证Token
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// 参数集合
        /// </summary>
        public ApiParameterCollection Parameters { get; set; }

        /// <summary>
        /// 数据对象
        /// </summary>
        public IDictionary<string, object> DataObject { get; set; }

        /// <summary>
        /// Cookie集合
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// RESTfulParameter
        /// </summary>
        public RESTfulParameter()
        {
            this.Parameters = new ApiParameterCollection();
            this.Cookies = new CookieCollection();
            this.HttpMethod = HttpMethod.GET;
            this.DataFormat = DataFormat.JSON;
        }

        /// <summary>
        /// RESTfulParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="method"></param>
        public RESTfulParameter(string name, HttpMethod method)
            : this()
        {
            this.MethodName = name;
            this.HttpMethod = method;
        }

        /// <summary>
        /// RESTfulParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <param name="format"></param>
        public RESTfulParameter(string name, HttpMethod method, DataFormat format)
            : this()
        {
            this.MethodName = name;
            this.HttpMethod = method;
            this.DataFormat = format;
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookie"></param>
        public void AddCookie(Cookie cookie)
        {
            this.Cookies.Add(cookie);
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookie"></param>
        public void AddCookie(Cookie[] cookies)
        {
            foreach (var cookie in cookies)
            {
                this.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="urlParameter"></param>
        public void AddParameter(string urlParameter)
        {
            var items = urlParameter.Split('&');
            foreach (var item in items)
            {
                var vitem = item.Split('=');
                AddParameter(vitem[0], vitem[1]);
            }
        }

        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddParameter(string name, object value)
        {
            this.Parameters.Add(name, value);
        }

        /// <summary>
        /// 添加一组参数
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        public void AddParameter(string[] names, object[] values)
        {
            for (int index = 0; index < names.Length; index++)
            {
                AddParameter(names[index], values[index]);
            }
        }

        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void AddParameter<T>(T item)
            where T : class
        {
            //添加对象参数
            foreach (var p in CoreHelper.GetPropertiesFromType(item.GetType()))
            {
                AddParameter(p.Name, p.GetValue(item, null));
            }
        }
    }
}
