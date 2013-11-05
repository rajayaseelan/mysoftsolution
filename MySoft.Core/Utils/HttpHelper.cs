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
        private Encoding encoding = Encoding.UTF8;
        private int timeout = 30 * 1000;

        /// <summary>
        /// 实例化HttpHelper
        /// </summary>
        public HttpHelper() { }

        /// <summary>
        /// 实例化HttpHelper
        /// </summary>
        /// <param name="encoding">编码</param>
        public HttpHelper(Encoding encoding)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// 实例化HttpHelper
        /// </summary>
        /// <param name="timeout">超时时间（单位：秒）</param>
        public HttpHelper(int timeout)
        {
            this.timeout = timeout * 1000;
        }

        /// <summary>
        /// 实例化HttpHelper
        /// </summary>
        /// <param name="encoding">编码</param>
        /// <param name="timeout">超时时间（单位：秒）</param>
        public HttpHelper(Encoding encoding, int timeout)
        {
            this.encoding = encoding;
            this.timeout = timeout * 1000;
        }

        #region GET方式

        /// <summary>
        /// 读取指定的url的数据，默认不缓存
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Reader(string url)
        {
            return Reader(url, null);
        }

        /// <summary>
        /// 读取指定的url的数据，默认不缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public string Reader(string url, WebHeaderCollection header)
        {
            return Reader(url, -1, header);
        }

        /// <summary>
        /// 读取指定url的数据 ，cacheTime小于0，不进行缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public string Reader(string url, int cacheTime)
        {
            return Reader(url, cacheTime, null);
        }

        /// <summary>
        /// 读取指定url的数据 ，cacheTime小于0，不进行缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cacheTime"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public string Reader(string url, int cacheTime, WebHeaderCollection header)
        {
            string cacheKey = string.Format("Resource_{0}", url);
            string responseString = CacheHelper.Get(cacheKey) as string;
            if (responseString == null)
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = timeout;

                if (header != null) request.Headers = header;

                //定义返回流
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //读取流信息
                        using (var sr = new StreamReader(response.GetResponseStream(), encoding))
                        {
                            responseString = sr.ReadToEnd();

                            if (cacheTime > 0)
                            {
                                CacheHelper.Insert(cacheKey, responseString, cacheTime);
                            }

                            return responseString;
                        }
                    }
                    else
                    {
                        throw new Exception(response.StatusDescription);
                    }
                }
                finally
                {
                    //最后关闭流
                    if (response != null) response.Close();
                }
            }

            return responseString;
        }

        #endregion

        #region Post方式

        /// <summary>
        /// Post指定url的数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Poster(string url, string value)
        {
            return Poster(url, value, null);
        }

        /// <summary>
        /// Post指定url的数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="value"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public string Poster(string url, string value, WebHeaderCollection header)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = timeout;

            if (header != null) request.Headers = header;

            //将数据写入请求流
            if (!string.IsNullOrEmpty(value))
            {
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";

                var buffer = encoding.GetBytes(value);

                //设置流长度
                request.ContentLength = buffer.Length;

                //写入流信息
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
            }

            //定义返回流
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //读取流信息
                    using (var sr = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        var responseString = sr.ReadToEnd();
                        return responseString;
                    }
                }
                else
                {
                    throw new Exception(response.StatusDescription);
                }
            }
            finally
            {
                //最后关闭流
                if (response != null) response.Close();
            }
        }

        #endregion
    }
}
