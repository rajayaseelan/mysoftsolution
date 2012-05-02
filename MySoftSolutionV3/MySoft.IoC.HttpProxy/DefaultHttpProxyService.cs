using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MySoft.Auth;
using MySoft.Security;

namespace MySoft.IoC.HttpProxy
{
    /// <summary>
    /// 默认的http代理服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DefaultHttpProxyService : IHttpProxyService
    {
        private HttpHelper helper;
        private IList<ServiceItem> services;

        // TODO: Implement the collection resource that will contain the SampleItem instances
        public DefaultHttpProxyService()
        {
            var url = ConfigurationManager.AppSettings["HttpProxyServer"];
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("Http proxy server can't for empty.");

            this.helper = new HttpHelper(url);
            this.services = new List<ServiceItem>();

            //读取服务信息
            this.ReaderService();
        }

        /// <summary>
        /// 读取服务
        /// </summary>
        private void ReaderService()
        {
            //数据缓存1分钟
            var jsonString = helper.Get("api", string.Empty, 60);

            //将数据反系列化成对象
            this.services = SerializationManager.DeserializeJson<IList<ServiceItem>>(jsonString);
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>字节数据流</returns>
        public Stream GetTextEntry(string name)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var query = request.UriTemplateMatch.QueryParameters;

            //响应格式
            response.ContentType = "application/json;charset=utf-8";

            //认证用户信息
            ServiceItem service;
            var header = new WebHeaderCollection();
            var jsonString = AuthorizeMethod(name, header, out service);

            //如果jsonString为null，则继续处理
            if (string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    //数据缓存5秒
                    var parameters = HttpUtility.UrlDecode(query.ToString());
                    jsonString = helper.Get(name, parameters, 5, header);

                    if (service != null && service.TypeString)
                    {
                        //如果返回是字符串类型，则设置为文本返回
                        response.ContentType = "text/plain;charset=utf-8";
                    }

                    //判断是否需要回调
                    var callback = query["callback"];

                    if (string.IsNullOrEmpty(callback))
                    {
                        //如果值为空或null
                        if (string.IsNullOrEmpty(jsonString))
                        {
                            return new MemoryStream();
                        }

                        var bytes = Encoding.UTF8.GetBytes(jsonString);
                        string etagToken = MD5.HexHash(bytes);
                        response.ETag = etagToken;
                        var IfNoneMatch = request.Headers["If-None-Match"];
                        if (IfNoneMatch != null && IfNoneMatch == etagToken)
                        {
                            response.StatusCode = HttpStatusCode.NotModified;
                            //request.IfModifiedSince.HasValue ? request.IfModifiedSince.Value : 
                            var IfModifiedSince = request.Headers["If-Modified-Since"];
                            response.LastModified = IfModifiedSince == null ? DateTime.Now : Convert.ToDateTime(IfModifiedSince);
                            return new MemoryStream();
                        }
                        else
                        {
                            response.LastModified = DateTime.Now;
                        }
                    }
                    else
                    {
                        //输出为javascript格式数据
                        response.ContentType = "application/javascript;charset=utf-8";
                        jsonString = string.Format("{0}({1});", callback, jsonString ?? "{}");
                    }
                }
                catch (WebException ex)
                {
                    var rep = (ex.Response as HttpWebResponse);
                    var stream = rep.GetResponseStream();
                    using (var sr = new StreamReader(stream))
                    {
                        jsonString = sr.ReadToEnd();
                    }

                    response.StatusCode = rep.StatusCode;
                    response.StatusDescription = rep.StatusDescription;
                }
            }

            //转换成utf8返回
            var buffer = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(buffer);
        }

        /// <summary>
        /// POST入口
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>字节数据流</returns>
        public Stream PostTextEntry(string name, Stream parameters)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var query = request.UriTemplateMatch.QueryParameters;

            //响应格式
            response.ContentType = "application/json;charset=utf-8";

            //认证用户信息
            ServiceItem item;
            var header = new WebHeaderCollection();
            var jsonString = AuthorizeMethod(name, header, out item);

            //如果jsonString为null，则继续处理
            if (string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    var postValue = string.Empty;
                    using (var sr = new StreamReader(parameters))
                    {
                        postValue = sr.ReadToEnd();
                    }

                    jsonString = helper.Post(name, query.ToString(), postValue, header);
                    if (item != null && item.TypeString)
                    {
                        //如果返回是字符串类型，则设置为文本返回
                        response.ContentType = "text/plain;charset=utf-8";
                    }
                }
                catch (WebException ex)
                {
                    var rep = (ex.Response as HttpWebResponse);
                    var stream = rep.GetResponseStream();
                    using (var sr = new StreamReader(stream))
                    {
                        jsonString = sr.ReadToEnd();
                    }

                    response.StatusCode = rep.StatusCode;
                    response.StatusDescription = rep.StatusDescription;
                }
            }

            //转换成utf8返回
            var buffer = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(buffer);
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <returns>字节数据流</returns>
        public Stream GetDocument()
        {
            return GetDocumentFromKind(null);
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>字节数据流</returns>
        public Stream GetDocumentFromKind(string kind)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var method = "help";
            if (!string.IsNullOrEmpty(kind)) method += ("/" + kind);

            //文档缓存1分钟
            string html = helper.Get(method, string.Empty, 60);

            //转换成utf8返回
            response.ContentType = "text/html;charset=utf-8";
            var regex = new Regex(@"<title>([\s\S]+) 处的操作</title>", RegexOptions.IgnoreCase);
            if (regex.IsMatch(html))
            {
                var url = string.Format("http://{0}/", request.UriTemplateMatch.RequestUri.Authority);
                html = html.Replace(regex.Match(html).Result("$1"), url);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }

        private string AuthorizeMethod(string name, WebHeaderCollection header, out ServiceItem service)
        {
            service = null;
            var response = WebOperationContext.Current.OutgoingResponse;

            //检测服务名称
            if (name == "favicon.ico")
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var item = new HttpProxyResult { Code = (int)response.StatusCode, Message = "Service 【" + name + "】 not found." };
                return SerializeJson(item);
            }
            else if (!services.Any(p => string.Compare(p.Name, name, true) == 0))
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var item = new HttpProxyResult { Code = (int)response.StatusCode, Message = "Method 【" + name + "】 not found." };
                return SerializeJson(item);
            }
            else
            {
                #region 进行认证处理

                service = services.Single(p => string.Compare(p.Name, name, true) == 0);

                //认证处理
                if (service.Authorized)
                {
                    var result = AuthorizeHeader(header);
                    if (result.Code == (int)HttpStatusCode.OK)
                        return null;
                    else
                        return SerializeJson(result);
                }

                #endregion
            }

            return null;
        }

        private HttpProxyResult AuthorizeHeader(WebHeaderCollection header)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            var token = new AuthorizeToken
            {
                RequestUri = request.UriTemplateMatch.RequestUri,
                Method = request.Method,
                Headers = request.Headers,
                Parameters = request.UriTemplateMatch.QueryParameters,
                Cookies = GetCookies()
            };

            try
            {
                var result = Authorize(token);
                if (result.Succeed && !string.IsNullOrEmpty(result.Name))
                {
                    header["Set-Authorize"] = result.Name;
                    response.StatusCode = HttpStatusCode.OK;
                    return new HttpProxyResult { Code = (int)response.StatusCode };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return new HttpProxyResult { Code = (int)response.StatusCode, Message = "Unauthorized or authorize name is empty." };
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                return new HttpProxyResult { Code = (int)response.StatusCode, Message = "Unauthorized - " + ex.Message };
            }
        }

        /// <summary>
        /// 获取Cookie信息
        /// </summary>
        /// <returns></returns>
        private HttpCookieCollection GetCookies()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Request.Cookies;

            HttpCookieCollection collection = new HttpCookieCollection();

            var request = WebOperationContext.Current.IncomingRequest;
            var cookie = request.Headers[HttpRequestHeader.Cookie];

            //从头中获取Cookie
            if (!string.IsNullOrEmpty(cookie))
            {
                string[] cookies = cookie.Split(';');
                HttpCookie cook = null;
                foreach (string e in cookies)
                {
                    if (!string.IsNullOrEmpty(e))
                    {
                        string[] values = e.Split(new char[] { '=' }, 2);
                        if (values.Length == 2)
                        {
                            cook = new HttpCookie(values[0], values[1]);
                        }
                        collection.Add(cook);
                    }
                }
            }

            return collection;
        }

        /// <summary>
        /// 系列化数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string SerializeJson(object item)
        {
            return SerializationManager.SerializeJson(item);
        }

        /// <summary>
        /// 进行认证处理
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual AuthorizeResult Authorize(AuthorizeToken token)
        {
            //返回认证失败
            return new AuthorizeResult
            {
                Succeed = false,
                Name = "Unknown"
            };
        }
    }
}
