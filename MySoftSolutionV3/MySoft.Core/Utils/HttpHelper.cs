using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace MySoft
{
    /// <summary>
    /// Http资源帮助
    /// </summary>
    public class HttpHelper
    {
        /// <summary>
        /// 读取指定的url的数据，默认缓存1分钟
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Reader(string url)
        {
            return Reader(url, -1);
        }

        /// <summary>
        /// 读取指定url的数据 ，cacheTime小于0，不进行缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public static string Reader(string url, int cacheTime)
        {
            string cacheKey = string.Format("Resource_{0}", url);
            string responseString = CacheHelper.Get(cacheKey) as string;
            if (responseString == null)
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 60 * 1000;

                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                    if (cacheTime > 0)
                    {
                        CacheHelper.Insert(cacheKey, responseString, cacheTime);
                    }
                }
            }

            return responseString;
        }

        /// <summary>
        /// Post指定url的数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Post(string url, string value)
        {
            string cacheKey = string.Format("Resource_{0}", url);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 60 * 1000;

            //将数据写入请求流
            if (!string.IsNullOrEmpty(value))
            {
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                using (var stream = request.GetRequestStream())
                {
                    var buffer = Encoding.UTF8.GetBytes(value);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }

            var response = request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        private string uri;
        /// <summary>
        /// 实例化HttpHelper，不带参数的uri，如http://127.0.0.1:8012/getuser/
        /// </summary>
        /// <param name="uri"></param>
        public HttpHelper(string uri)
        {
            this.uri = uri;
        }

        #region GET方式

        /// <summary>
        /// 读取指定url的数据，通过parameter传递参数
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public string Reader(string method, string parameter)
        {
            return Reader(method, parameter, -1);
        }

        /// <summary>
        /// 读取指定url的数据，通过parameter传递参数
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public string Reader(string method, string parameter, int cacheTime)
        {
            string query = string.Empty;
            if (string.IsNullOrEmpty(parameter))
                query = string.Format("/{0}", method);
            else
                query = string.Format("/{0}?{1}", method, parameter);

            var url = uri.TrimEnd('/') + query;
            return Reader(url, cacheTime);
        }

        /// <summary>
        /// 读取指定url的数据，通过item传递参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public string Reader<T>(string method, T item)
            where T : class
        {
            return Reader<T>(method, item, -1);
        }

        /// <summary>
        /// 读取指定url的数据，通过item传递参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="item"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public string Reader<T>(string method, T item, int cacheTime)
            where T : class
        {
            string query = string.Format("/{0}", method);
            var list = new List<string>();
            foreach (var p in typeof(T).GetProperties())
            {
                list.Add(string.Format("{0}={1}", p.Name, p.GetValue(item, null)));
            }

            if (list.Count > 0)
                query += "?" + string.Join("&", list.ToArray());

            var url = uri.TrimEnd('/') + query;
            return Reader(url, cacheTime);
        }

        #endregion

        #region POST方式

        /// <summary>
        /// Post指定url的数据，通过parameter传递参数
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Post(string method, string parameter, string value)
        {
            string query = string.Empty;
            if (string.IsNullOrEmpty(parameter))
                query = string.Format("/{0}", method);
            else
                query = string.Format("/{0}?{1}", method, parameter);

            var url = uri.TrimEnd('/') + query;
            return Post(url, value);
        }

        /// <summary>
        /// Post指定url的数据，通过item传递参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="item"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Post<T>(string method, T item, string value)
            where T : class
        {
            string query = string.Format("/{0}", method);
            var list = new List<string>();
            foreach (var p in typeof(T).GetProperties())
            {
                list.Add(string.Format("{0}={1}", p.Name, p.GetValue(item, null)));
            }

            if (list.Count > 0)
                query += "?" + string.Join("&", list.ToArray());

            var url = uri.TrimEnd('/') + query;
            return Post(url, value);
        }

        #endregion
    }
}
