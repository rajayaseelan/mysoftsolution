using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.IO;
using System.ServiceModel.Web;
using System.Net;
using System.Text.RegularExpressions;
using MySoft.Security;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;

namespace MySoft.IoC.HttpProxy
{
    /// <summary>
    /// 默认的http代理服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public abstract class DefaultHttpProxyService : IHttpProxyService
    {
        private HttpHelper helper;
        private IList<ServiceItem> services;

        // TODO: Implement the collection resource that will contain the SampleItem instances
        public DefaultHttpProxyService()
        {
            var url = ConfigurationManager.AppSettings["HttpProxyServer"];
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("HttpProxyServer can't for empty.");

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
            var jsonString = helper.Reader("api", null);

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

            //认证用户信息
            var stream = AuthorizeData(name, query);
            if (stream != null) return stream;

            var buffer = new byte[0];

            try
            {
                var jsonString = helper.Reader(name, query.ToString());

                //如果无值，则置为null
                if (string.IsNullOrEmpty(jsonString)) jsonString = null;

                //判断是否需要回调
                var callback = query["callback"];

                if (string.IsNullOrEmpty(callback))
                {
                    jsonString = jsonString ?? "{}";

                    buffer = Encoding.UTF8.GetBytes(jsonString);
                    string etagToken = MD5.HexHash(buffer);
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
                    buffer = Encoding.UTF8.GetBytes(jsonString);
                }
            }
            catch (WebException ex)
            {
                var rep = (ex.Response as HttpWebResponse);
                stream = rep.GetResponseStream();
                using (var sr = new StreamReader(stream))
                {
                    var jsonString = sr.ReadToEnd();
                    buffer = Encoding.UTF8.GetBytes(jsonString);
                }

                response.StatusCode = rep.StatusCode;
                response.StatusDescription = rep.StatusDescription;
            }

            //转换成utf8返回
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

            //认证用户信息
            var stream = AuthorizeData(name, query);
            if (stream != null) return stream;

            var buffer = new byte[0];

            try
            {
                var postValue = string.Empty;
                using (var sr = new StreamReader(parameters))
                {
                    postValue = sr.ReadToEnd();
                }

                var jsonString = helper.Post(name, query.ToString(), postValue);
                buffer = Encoding.UTF8.GetBytes(jsonString);
            }
            catch (WebException ex)
            {
                var rep = (ex.Response as HttpWebResponse);
                stream = rep.GetResponseStream();
                using (var sr = new StreamReader(stream))
                {
                    var jsonString = sr.ReadToEnd();
                    buffer = Encoding.UTF8.GetBytes(jsonString);
                }

                response.StatusCode = rep.StatusCode;
                response.StatusDescription = rep.StatusDescription;
            }

            //转换成utf8返回
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
            string html = helper.Reader(method, null);

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

        private Stream AuthorizeData(string name, NameValueCollection query)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            response.ContentType = "application/json;charset=utf-8";

            //检测服务名称
            if (name == "favicon.ico" || !services.Any(p => string.Compare(p.Name, name, true) == 0))
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var item = new { Code = (int)response.StatusCode, Message = "【" + name + "】 NOT FOUND." };
                return new MemoryStream(SerializeJson(item));
            }
            else
            {
                #region 进行认证处理

                var service = services.Single(p => string.Compare(p.Name, name, true) == 0);
                if (service.Authorized)
                {
                    var token = new AuthorizeToken { Parameters = request.UriTemplateMatch.QueryParameters };
                    if (HttpContext.Current != null) token.Cookies = HttpContext.Current.Request.Cookies;

                    try
                    {
                        var result = Authorize(token);
                        if (result.Succeed && !string.IsNullOrEmpty(result.Name))
                        {
                            query[service.AuthParameter] = result.Name;
                        }
                        else
                        {
                            response.StatusCode = HttpStatusCode.Unauthorized;
                            var item = new { Code = (int)response.StatusCode, Message = "UNAUTHORIZED OR AUTHORIZE NAME IS EMPTY." };
                            return new MemoryStream(SerializeJson(item));
                        }
                    }
                    catch (Exception ex)
                    {
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        var item = new { Code = (int)response.StatusCode, Message = "UNAUTHORIZED - " + ex.Message };
                        return new MemoryStream(SerializeJson(item));
                    }
                }

                #endregion
            }

            return null;
        }

        /// <summary>
        /// 系列化数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private byte[] SerializeJson(object item)
        {
            var jsonString = SerializationManager.SerializeJson(item);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        /// <summary>
        /// 进行认证处理
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract AuthorizeResult Authorize(AuthorizeToken token);
    }
}
