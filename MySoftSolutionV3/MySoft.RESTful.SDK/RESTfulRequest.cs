using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful请求
    /// </summary>
    public class RESTfulRequest
    {
        private Encoding encoding = Encoding.UTF8;
        /// <summary>
        /// 编码方式
        /// </summary>
        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        private int timeout = 60;
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        private string url;
        /// <summary>
        /// 请求的url，例如：http://openapi.mysoft.com
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private RESTfulParameter parameter;

        /// <summary>
        /// 实例化RESTfulRequest
        /// </summary>
        /// <param name="parameter"></param>
        public RESTfulRequest(RESTfulParameter parameter)
        {
            this.parameter = parameter;
        }

        /// <summary>
        /// 实例化RESTfulRequest
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="timeout"></param>
        public RESTfulRequest(RESTfulParameter parameter, int timeout)
        {
            this.parameter = parameter;
            this.timeout = timeout;
        }

        /// <summary>
        /// 实例化RESTfulRequest
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        public RESTfulRequest(string url, RESTfulParameter parameter)
        {
            this.url = url;
            this.parameter = parameter;
        }

        /// <summary>
        /// 实例化RESTfulRequest
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <param name="timeout"></param>
        public RESTfulRequest(string url, RESTfulParameter parameter, int timeout)
        {
            this.url = url;
            this.parameter = parameter;
            this.timeout = timeout;
        }

        private string GetRequestUrl()
        {
            string value = string.Format("{0}/{1}.{2}/{3}", url.TrimEnd('/'), parameter.HttpMethod, parameter.DataFormat, parameter.MethodName);
            List<string> list = new List<string>();
            foreach (var p in parameter.Parameters)
            {
                list.Add(string.Format("{0}={1}", p.Name, p.Value));
            }

            //添加Token参数
            if (parameter.Token != null)
            {
                list.Add(string.Format("tokenID={0}", parameter.Token.TokenId));
                if (parameter.Token.Parameters.Count > 0)
                {
                    foreach (var p in parameter.Token.Parameters)
                    {
                        list.Add(string.Format("{0}={1}", p.Name, p.Value));
                    }
                }
                else
                {
                    list.Add(string.Format("username={0}", parameter.Token.TokenId));
                }
            }

            if (list.Count > 0)
                return string.Format("{0}?{1}", value, string.Join("&", list.ToArray())).ToLower();
            else
                return value.ToLower();
        }

        /// <summary>
        /// 获取响应的字符串
        /// </summary>
        /// <returns></returns>
        public string GetResponseString()
        {
            return GetResponse<string>();
        }

        /// <summary>
        /// 获取响应结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetResponse<T>()
            where T : class
        {
            return GetResponse(typeof(T)) as T;
        }

        /// <summary>
        /// 获取相应
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public object GetResponse(Type returnType)
        {
            try
            {
                string url = GetRequestUrl();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                if (timeout > 0) request.Timeout = timeout * 1000;

                //判断是否为POST方式
                if (parameter.DataObject != null && parameter.HttpMethod != HttpMethod.GET)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "POST";

                    var stream = request.GetRequestStream();
                    string input = string.Empty;

                    var postObject = parameter.DataObject;
                    if (parameter.DataObject is ApiParameterCollection)
                    {
                        postObject = (parameter.DataObject as ApiParameterCollection).ToDictionary();
                    }

                    //判断数据类型
                    if (parameter.DataFormat == DataFormat.JSON)
                    {
                        input = SerializationManager.SerializeJson(parameter.DataObject);
                    }
                    else if (parameter.DataFormat == DataFormat.XML)
                    {
                        input = SerializationManager.SerializeXml(parameter.DataObject);
                    }

                    var buffer = encoding.GetBytes(input);
                    stream.Write(buffer, 0, buffer.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), encoding);
                    string value = sr.ReadToEnd();

                    if (returnType == typeof(string))
                    {
                        return value;
                    }
                    else
                    {
                        if (parameter.DataFormat == DataFormat.JSON)
                            return SerializationManager.DeserializeJson(returnType, value);
                        else if (parameter.DataFormat == DataFormat.XML)
                            return SerializationManager.DeserializeXml(returnType, value);
                        else
                            return value;
                    }
                }
                else
                {
                    throw new RESTfulException(response.StatusDescription) { Code = (int)response.StatusCode };
                }
            }
            catch (WebException ex)
            {
                throw new RESTfulException(ex.Message, ex) { Code = (int)(ex.Response as HttpWebResponse).StatusCode };
            }
            catch (Exception ex)
            {
                throw new RESTfulException(ex.Message, ex) { Code = 404 };
            }
        }
    }
}
