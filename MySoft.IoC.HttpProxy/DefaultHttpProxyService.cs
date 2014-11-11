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
using System.Threading;
using System.Web;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.RESTful;
using MySoft.Security;
using Newtonsoft.Json;

namespace MySoft.IoC.HttpProxy
{
    /// <summary>
    /// 默认的http代理服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DefaultHttpProxyService : IHttpProxyService
    {
        const string HTTP_PROXY_API = "{0}/{1}";
        private HttpHelper helper;
        private IList<string> proxyServers;
        private IDictionary<string, IList<ServiceItem>> services;

        /// <summary>
        /// TODO: Implement the collection resource that will contain the SampleItem instances
        /// </summary>
        public DefaultHttpProxyService()
        {
            var proxyServer = ConfigurationManager.AppSettings["HttpProxyServer"];
            if (string.IsNullOrEmpty(proxyServer))
                throw new ArgumentNullException("Http proxy server can't for empty.");

            var urls = proxyServer.Split(';', '|').ToList();
            this.proxyServers = new List<string>();
            foreach (var url in urls)
            {
                if (!this.proxyServers.Contains(url.TrimEnd('/')))
                {
                    this.proxyServers.Add(url.TrimEnd('/'));
                }
            }

            this.helper = new HttpHelper(Encoding.UTF8, 30);
            this.services = new Dictionary<string, IList<ServiceItem>>();

            //更新服务
            var thread = new Thread(UpdateService);
            thread.Start();
        }

        /// <summary>
        /// 更新服务
        /// </summary>
        private void UpdateService()
        {
            while (true)
            {
                foreach (var proxyServer in proxyServers)
                {
                    bool isError = false;
                    var items = ReaderService(proxyServer, out isError);

                    //判断是否有更新
                    if (!isError)
                    {
                        lock (services)
                        {
                            //如果存在，则判断是否一致
                            if (services.ContainsKey(proxyServer))
                            {
                                //判断数量是否一致
                                if (services[proxyServer].Count != items.Count)
                                {
                                    services[proxyServer] = items;
                                }
                            }
                            else
                            {
                                //不存在，则替换掉
                                services[proxyServer] = items;
                            }
                        }
                    }
                }

                //一分钟检测一次
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        /// <summary>
        /// 读取服务
        /// </summary>
        /// <param name="proxyServer"></param>
        /// <param name="isError"></param>
        /// <returns></returns>
        private List<ServiceItem> ReaderService(string proxyServer, out bool isError)
        {
            var serviceItems = new List<ServiceItem>();

            try
            {
                //数据缓存1分钟
                var url = string.Format(HTTP_PROXY_API, proxyServer, "api");
                var jsonString = helper.Reader(url);

                //将数据反系列化成对象
                var items = SerializationManager.DeserializeJson<IList<ServiceItem>>(jsonString);
                serviceItems.AddRange(items);

                isError = false;
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("WebAPI", ex);

                isError = true;
                //TODO
            }

            return serviceItems;
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
            var jsonString = AuthorizeMethod(name, HttpMethod.GET, out service);

            //如果jsonString为null，则继续处理
            if (string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    var jobj = ParameterHelper.ConvertJObject(query, null);

                    //设置认证参数
                    SetAuthParameter(service, jobj);

                    //调用服务
                    jsonString = Invoke(service, jobj.ToString(Formatting.Indented));

                    if (query.AllKeys.Contains("format"))
                    {
                        jsonString = SerializationManager.DeserializeJson<string>(jsonString);

                        //如果返回是字符串类型，则设置为文本返回
                        var format = query["format"];
                        switch (format)
                        {
                            case "html":
                                response.ContentType = "text/html;charset=utf-8";
                                break;
                            case "xml":
                                response.ContentType = "text/xml;charset=utf-8";
                                break;
                            default:
                                response.ContentType = "text/plain;charset=utf-8";
                                break;
                        }

                        //转换成utf8返回
                        var bytes = Encoding.UTF8.GetBytes(jsonString);
                        return new MemoryStream(bytes);
                    }

                    //判断是否需要回调
                    var callback = query["callback"];
                    var jsoncallback = query["jsoncallback"];

                    if (string.IsNullOrEmpty(callback) && string.IsNullOrEmpty(jsoncallback))
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
                        if (string.IsNullOrEmpty(jsoncallback))
                        {
                            //输出为javascript格式数据
                            response.ContentType = "text/javascript;charset=utf-8";
                            jsonString = string.Format("{0}({1});", callback, jsonString ?? "{}");
                        }
                        else
                        {
                            //输出为json格式数据
                            jsonString = string.Format("{0}({1});", jsoncallback, jsonString ?? "{}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    jsonString = SerializationManager.SerializeJson(new { Code = (int)response.StatusCode, Message = ex.Message });
                }
                finally
                {
                    //使用完后清理上下文
                    AuthorizeContext.Current = null;
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
        public Stream PostTextEntry(string name, Stream stream)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var query = request.UriTemplateMatch.QueryParameters;

            //响应格式
            response.ContentType = "application/json;charset=utf-8";

            //认证用户信息
            ServiceItem service;
            var header = new WebHeaderCollection();
            var jsonString = AuthorizeMethod(name, HttpMethod.POST, out service);

            //如果jsonString为null，则继续处理
            if (string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    var postValue = string.Empty;
                    using (var sr = new StreamReader(stream))
                    {
                        postValue = sr.ReadToEnd();
                    }

                    //转换面对象
                    var nvpost = ParameterHelper.ConvertCollection(postValue);

                    var jobj = ParameterHelper.ConvertJObject(query, nvpost);

                    //设置认证参数
                    SetAuthParameter(service, jobj);

                    //调用服务
                    jsonString = Invoke(service, jobj.ToString(Formatting.Indented));
                }
                catch (Exception ex)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    jsonString = SerializationManager.SerializeJson(new { Code = (int)response.StatusCode, Message = ex.Message });
                }
                finally
                {
                    AuthorizeContext.Current = null;
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
        public Stream GetTcpDocument()
        {
            return GetHelpStream("tcp");
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <returns>字节数据流</returns>
        public Stream GetHttpDocument()
        {
            return GetHelpStream("help");
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>字节数据流</returns>
        public Stream GetHttpDocumentFromKind(string kind)
        {
            var method = "help";
            if (!string.IsNullOrEmpty(kind)) method += ("/" + kind);

            return GetHelpStream(method);
        }

        /// <summary>
        /// 获取文档流
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private Stream GetHelpStream(string method)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            //获取代理服务地址
            var sb = new StringBuilder();
            var content = string.Empty;
            var contentRegex = new Regex(@"<div id=""content"">([\s\S]+?)</div>");
            var index = 0;

            foreach (var proxyServer in proxyServers)
            {
                lock (services)
                {
                    if (!services.ContainsKey(proxyServer)) continue;
                    if (services[proxyServer].Count == 0) continue;
                }

                //文档缓存1分钟
                var url = string.Format(HTTP_PROXY_API, proxyServer, method);
                string html = helper.Reader(url);

                //转换成utf8返回
                response.ContentType = "text/html;charset=utf-8";
                var regex = new Regex(@"<title>([\s\S]+?) 处的操作</title>", RegexOptions.IgnoreCase);
                if (regex.IsMatch(html))
                {
                    var host = GetRequestUri().GetLeftPart(UriPartial.Authority);
                    html = html.Replace(regex.Match(html).Result("$1"), host + "/");
                }

                if (string.IsNullOrEmpty(content)) content = html;

                regex = new Regex(@"<p>([\s\S]+?)</p>");
                if (regex.IsMatch(html))
                {
                    html = html.Replace(regex.Match(html).Result("$1"),
                        string.Format("{0}，{1}", proxyServer, regex.Match(html).Result("$1")));
                }

                regex = new Regex(@"<p title([\s\S]+?)</p>");
                if (method == "tcp" || regex.IsMatch(html))
                {
                    if (index > 0)
                    {
                        regex = new Regex(@"<p class=""heading1"">([\s\S]+?)</p>", RegexOptions.IgnoreCase);
                        if (regex.IsMatch(html))
                        {
                            html = regex.Replace(html, string.Empty);
                        }
                    }

                    index++;

                    if (contentRegex.IsMatch(html))
                    {
                        html = contentRegex.Match(html).Result("$1");
                    }

                    sb.Append(html);
                }
            }

            //替换成统一的内容
            content = contentRegex.Replace(content,
                    string.Concat(@"<div id=""content"">", sb.ToString(), "</div>"));

            response.ContentType = "text/html;charset=utf-8";

            var buffer = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(buffer);
        }

        private string AuthorizeMethod(string name, HttpMethod method, out ServiceItem service)
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
            else if (!GetServiceItems().Any(p => string.Compare(p.CallerName, name, true) == 0))
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var item = new HttpProxyResult { Code = (int)response.StatusCode, Message = "Method 【" + name + "】 not found." };
                return SerializeJson(item);
            }
            else
            {
                #region 进行认证处理

                service = GetServiceItems().First(p => string.Compare(p.CallerName, name, true) == 0);

                if (service.HttpMethod == HttpMethod.POST && method == HttpMethod.GET)
                {
                    response.StatusCode = HttpStatusCode.MethodNotAllowed;
                    var item = new HttpProxyResult { Code = (int)response.StatusCode, Message = "Method 【" + name + "】 not allowed [GET]." };
                    return SerializeJson(item);
                }

                //认证处理
                if (service.Authorized)
                {
                    var result = Authorize();
                    if (result.Code == (int)HttpStatusCode.OK)
                        return null;
                    else
                        return SerializeJson(result);
                }

                #endregion
            }

            return null;
        }

        private HttpProxyResult Authorize()
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            response.StatusCode = HttpStatusCode.Unauthorized;

            //认证成功，设置上下文
            var token = new AuthorizeToken
            {
                RequestUri = GetRequestUri(),
                Method = request.Method,
                Headers = request.Headers,
                Parameters = request.UriTemplateMatch.QueryParameters,
                Cookies = GetCookies(),
                AuthorizeType = AuthorizeType.User
            };

            try
            {
                var user = Authorize(token);
                response.StatusCode = HttpStatusCode.OK;

                if (user == null)
                {
                    //认证成功，设置上下文
                    AuthorizeContext.Current = new AuthorizeContext { Token = token };
                }
                else
                {
                    //认证成功，设置上下文
                    AuthorizeContext.Current = new AuthorizeContext
                    {
                        Token = token,
                        UserId = user.UserId,
                        UserName = user.UserName,
                        UserState = user.UserState
                    };
                }

                return new HttpProxyResult { Code = (int)response.StatusCode, Message = "Authentication request success." };
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Authorize", ex);
                return new HttpProxyResult { Code = (int)response.StatusCode, Message = "Unauthorized - " + ex.Message };
            }
        }

        /// <summary>
        /// 获取获取列表
        /// </summary>
        /// <returns></returns>
        private IList<ServiceItem> GetServiceItems()
        {
            lock (services)
            {
                var list = new List<ServiceItem>();
                foreach (var items in services.Values)
                {
                    list.AddRange(items);
                }

                return list;
            }
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
        protected virtual AuthorizeUser Authorize(AuthorizeToken token)
        {
            //返回认证失败
            throw new AuthorizeException(401, "Authentication request fail.");
        }

        #region 获取Uri及Cookie

        /// <summary>
        /// 获取请求Uri
        /// </summary>
        /// <returns></returns>
        private Uri GetRequestUri()
        {
            Uri uri = null;
            if (HttpContext.Current != null)
            {
                uri = HttpContext.Current.Request.Url;
            }
            else if (WebOperationContext.Current != null)
            {
                var request = WebOperationContext.Current.IncomingRequest;
                uri = request.UriTemplateMatch.RequestUri;
            }

            return uri;
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

        #endregion

        /// <summary>
        /// 响应服务项
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string Invoke(ServiceItem item, string parameters)
        {
            var node = ServerNode.Parse(item.ServerUri);

            //执行消息
            var message = new InvokeMessage
            {
                ServiceName = item.ServiceName,
                MethodName = item.MethodName,
                Parameters = parameters,
                CacheTime = item.CacheTime
            };

            //执行服务
            var invokeData = CastleFactory.Create().Invoke(node, message);

            return invokeData.Value;
        }

        /// <summary>
        /// 设置认证参数
        /// </summary>
        /// <param name="service"></param>
        /// <param name="obj"></param>
        private void SetAuthParameter(ServiceItem service, Newtonsoft.Json.Linq.JObject obj)
        {
            if (service.Authorized && !string.IsNullOrEmpty(service.AuthParameter))
            {
                if (string.Compare("userid", service.AuthParameter, true) == 0)
                    obj[service.AuthParameter] = AuthorizeContext.Current.UserId;
                else
                    obj[service.AuthParameter] = AuthorizeContext.Current.UserName;
            }
        }
    }
}