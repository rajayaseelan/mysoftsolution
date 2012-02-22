using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;

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

        private int timeout = 5 * 60;
        /// <summary>
        /// 超时时间：单位（秒）
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
        {
            return (T)GetResponse(typeof(T));
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
                //request.KeepAlive = false;
                if (timeout > 0) request.Timeout = timeout * 1000;

                #region 处理Cookies

                //处理Cookies
                if (parameter.Cookies.Count > 0)
                {
                    CookieContainer container = new CookieContainer();
                    container.Add(request.RequestUri, parameter.Cookies);
                    request.CookieContainer = container;
                }

                #endregion

                //判断是否为POST方式
                if (parameter.DataObject != null && parameter.HttpMethod != HttpMethod.GET)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    //request.ContentType = "application/json";
                    request.Method = parameter.HttpMethod.ToString();

                    string input = SerializationManager.SerializeJson(parameter.DataObject);
                    var buffer = encoding.GetBytes(input);
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                    }
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), encoding);
                    string value = sr.ReadToEnd();

                    if (string.IsNullOrEmpty(value) || returnType == typeof(string))
                    {
                        return Convert.ChangeType(value, returnType);
                    }
                    else
                    {
                        var retType = returnType;
                        object result = null;
                        if (returnType.IsValueType)
                        {
                            retType = typeof(RESTfulResponse);
                        }

                        if (parameter.DataFormat == DataFormat.JSON)
                            result = SerializationManager.DeserializeJson(retType, value);
                        else if (parameter.DataFormat == DataFormat.XML)
                            result = SerializationManager.DeserializeXml(retType, value);
                        else
                            result = value;

                        if (retType == typeof(RESTfulResponse))
                            return Convert.ChangeType((result as RESTfulResponse).Value, returnType);
                        else
                            return result;
                    }
                }
                else
                {
                    throw new RESTfulException(response.StatusDescription) { Code = (int)response.StatusCode };
                }
            }
            catch (WebException ex)
            {
                RESTfulResult result = null;
                try
                {
                    var stream = (ex.Response as HttpWebResponse).GetResponseStream();
                    StreamReader sr = new StreamReader(stream);
                    string content = sr.ReadToEnd();

                    if (parameter.DataFormat == DataFormat.JSON)
                        result = SerializationManager.DeserializeJson<RESTfulResult>(content);
                    else if (parameter.DataFormat == DataFormat.XML)
                        result = SerializationManager.DeserializeXml<RESTfulResult>(content);
                }
                catch { }

                if (result != null)
                {
                    throw new RESTfulException(result.Message) { Code = result.Code };
                }
                else if (ex.Response != null)
                {
                    var res = ex.Response as HttpWebResponse;
                    throw new RESTfulException(res.StatusDescription) { Code = (int)res.StatusCode };
                }
                else
                {
                    throw ex;
                }
            }
            catch (RESTfulException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new RESTfulException(ex.Message, ex) { Code = 404 };
            }
        }
    }
}
