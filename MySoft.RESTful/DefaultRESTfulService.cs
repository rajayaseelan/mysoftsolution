using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using MySoft.Logger;
using MySoft.RESTful.Business;
using MySoft.RESTful.Utils;
using MySoft.Security;

namespace MySoft.RESTful
{
    /// <summary>
    /// 默认的RESTful服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DefaultRESTfulService : IRESTfulService
    {
        private string host;
        /// <summary>
        /// 上下文处理
        /// </summary>
        public IRESTfulContext Context { get; set; }

        /// <summary>
        /// 实例化DefaultRESTfulService
        /// </summary>
        public DefaultRESTfulService()
        {
            //创建上下文
            this.Context = new BusinessRESTfulContext();
            this.host = ConfigurationManager.AppSettings["HttpProxyServer"];
        }

        #region IRESTfulService 成员

        /// <summary>
        /// 实现Post方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public Stream PostJsonEntry(string kind, string method, Stream parameter)
        {
            return GetResponseStream(ParameterFormat.Json, kind, method, parameter);
        }

        /// <summary>
        /// 实现Get方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetJsonEntry(string kind, string method)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var query = request.UriTemplateMatch.QueryParameters;
            var jsoncallback = query["jsoncallback"];

            if (string.IsNullOrEmpty(jsoncallback))
            {
                return GetResponseStream(ParameterFormat.Json, kind, method, null);
            }
            else
            {
                string result = GetResponseString(ParameterFormat.Json, kind, method, null, null);
                result = string.Format("{0}({1});", jsoncallback, result ?? "{}");
                return new MemoryStream(Encoding.UTF8.GetBytes(result));
            }
        }

        /// <summary>
        /// 实现Post方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public Stream PostXmlEntry(string kind, string method, Stream parameter)
        {
            return GetResponseStream(ParameterFormat.Xml, kind, method, parameter);
        }

        /// <summary>
        /// 实现Get方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetXmlEntry(string kind, string method)
        {
            return GetResponseStream(ParameterFormat.Xml, kind, method, null);
        }

        /// <summary>
        /// 实现Get方式Jsonp响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetEntryCallBack(string kind, string method)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            var query = request.UriTemplateMatch.QueryParameters;
            var callback = query["callback"];
            string result = string.Empty;

            if (string.IsNullOrEmpty(callback))
            {
                var ret = new RESTfulResult { Code = (int)HttpStatusCode.OK, Message = "Not found [callback] parameter!" };
                //throw new WebFaultException<RESTfulResult>(ret, HttpStatusCode.Forbidden);
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ContentType = "application/json;charset=utf-8";
                result = SerializationManager.SerializeJson(ret);
            }
            else
            {
                result = GetResponseString(ParameterFormat.Jsonp, kind, method, null, null);
                response.ContentType = "text/javascript;charset=utf-8";
                result = string.Format("{0}({1});", callback, result ?? "{}");
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        /// <summary>
        /// 实现Get方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetTextEntry(string kind, string method)
        {
            return GetResponseStream(ParameterFormat.Text, kind, method, null);
        }

        /// <summary>
        /// 实现Get方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetHtmlEntry(string kind, string method)
        {
            return GetResponseStream(ParameterFormat.Html, kind, method, null);
        }

        /// <summary>
        /// 获取方法的html文档
        /// </summary>
        /// <returns></returns>
        public Stream GetMethodHtml()
        {
            return GetMethodHtmlFromKind(string.Empty);
        }

        /// <summary>
        /// 获取方法的html文档
        /// </summary>
        /// <returns></returns>
        public Stream GetMethodHtmlFromKind(string kind)
        {
            return GetMethodHtmlFromKindAndMethod(kind, null);
        }

        /// <summary>
        /// 获取方法的html文档
        /// </summary>
        /// <returns></returns>
        public Stream GetMethodHtmlFromKindAndMethod(string kind, string method)
        {
            var response = WebOperationContext.Current.OutgoingResponse;

            var html = Context.MakeDocument(GetRequestUri(), kind, method);
            response.ContentType = "text/html;charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }

        #endregion

        private Stream GetResponseStream(ParameterFormat format, string kind, string method, Stream stream)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            NameValueCollection nvs = null;
            if (stream != null)
            {
                //接收流内部数据
                var sr = new StreamReader(stream, Encoding.UTF8);
                string streamValue = sr.ReadToEnd();

                //转换成NameValueCollection
                nvs = ParameterHelper.ConvertCollection(streamValue);
            }

            string result = GetResponseString(format, kind, method, null, nvs);
            if (string.IsNullOrEmpty(result)) return new MemoryStream();

            //转换成buffer
            var buffer = Encoding.UTF8.GetBytes(result);

            if (request.Method.ToUpper() == "GET" && response.StatusCode == HttpStatusCode.OK)
            {
                //处理ETag功能
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

            return new MemoryStream(buffer);
        }

        private string GetResponseString(ParameterFormat format, string kind, string method, NameValueCollection nvget, NameValueCollection nvpost)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            if (nvget == null) nvget = request.UriTemplateMatch.QueryParameters;
            if (nvpost == null) nvpost = new NameValueCollection();

            if (format == ParameterFormat.Json)
                response.ContentType = "application/json;charset=utf-8";
            else if (format == ParameterFormat.Xml)
                response.ContentType = "text/xml;charset=utf-8";
            else if (format == ParameterFormat.Text)
                response.ContentType = "text/plain;charset=utf-8";
            else if (format == ParameterFormat.Html)
                response.ContentType = "text/html;charset=utf-8";

            //从缓存读取
            object result = null;
            if (Context != null && !Context.Contains(kind, method))
            {
                response.StatusCode = HttpStatusCode.NotFound;
                result = new RESTfulResult { Code = (int)response.StatusCode, Message = "service [" + kind + "." + method + "] not found." };
            }
            else
            {

                //进行认证处理
                RESTfulResult authResult = new RESTfulResult { Code = (int)HttpStatusCode.OK };

                //进行认证处理
                if (Context != null)
                {
                    var type = Context.IsAuthorized(kind, method);
                    authResult = AuthorizeRequest(type);
                }

                //认证成功
                if (authResult.Code == (int)HttpStatusCode.OK)
                {
                    try
                    {
                        Type retType;
                        result = Context.Invoke(kind, method, nvget, nvpost, out retType);

                        //设置返回成功
                        response.StatusCode = HttpStatusCode.OK;

                        //xml方式需要进行数据包装
                        if (format == ParameterFormat.Xml)
                        {
                            //如果是值类型，则以对象方式返回
                            if (retType.IsValueType || retType == typeof(string))
                            {
                                result = new RESTfulResponse { Value = result };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        RESTfulResult ret;
                        var errorMessage = GetErrorMessage(ex, kind, method, nvget, nvpost, out ret);

                        result = ret;

                        //重新定义一个异常
                        var error = new Exception(errorMessage, ex);

                        //记录错误日志
                        SimpleLog.Instance.WriteLogForDir("RESTful\\" + kind, error);
                    }
                    finally
                    {
                        //使用完后清理上下文
                        AuthorizeContext.Current = null;
                    }
                }
                else
                {
                    result = authResult;
                }
            }

            ISerializer serializer = SerializerFactory.Create(format);
            return serializer.Serialize(result);
        }

        /// <summary>
        /// 获取错误消息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetErrorMessage(Exception exception, string kind, string method, NameValueCollection nvget, NameValueCollection nvpost, out RESTfulResult ret)
        {
            var response = WebOperationContext.Current.OutgoingResponse;
            var request = WebOperationContext.Current.IncomingRequest;

            int code = (int)HttpStatusCode.BadRequest;
            if (exception is RESTfulException)
            {
                code = (exception as RESTfulException).Code;
            }

            //转换状态码
            response.StatusCode = (HttpStatusCode)code;

            //设置返回值
            ret = new RESTfulResult { Code = code, Message = ErrorHelper.GetInnerException(exception).Message };

            var errorMessage = string.Format("\r\n\tCode:[{0}]\r\n\tError:[{1}]\r\n\tMethod:[{2}.{3}]", code,
                    ErrorHelper.GetInnerException(exception).Message, kind, method);

            //请求地址
            errorMessage = string.Format("{0}\r\n\tRequest Uri:{1}", errorMessage, GetRequestUri());

            if (request.Method.ToUpper() == "POST")
            {
                errorMessage = string.Format("{0}\r\n\tGET Parameters:{1}\r\n\tPOST Parameters:{2}",
                                                errorMessage, GetParameters(nvget), GetParameters(nvpost));
            }
            else
            {
                errorMessage = string.Format("{0}\r\n\tGET Parameters:{1}", errorMessage, GetParameters(nvget));
            }

            //加上认证的用户名
            if (AuthorizeContext.Current != null && !string.IsNullOrEmpty(AuthorizeContext.Current.UserName))
            {
                errorMessage = string.Format("{0}\r\n\tUser:[{1}]", errorMessage, AuthorizeContext.Current.UserName);
            }

            //返回结果
            return errorMessage;
        }

        private string GetParameters(NameValueCollection nvs)
        {
            var sb = new StringBuilder();
            foreach (var key in nvs.AllKeys)
            {
                sb.AppendFormat("\r\n\t\t{0}={1}", key, nvs[key]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 进行认证
        /// </summary>
        /// <returns></returns>
        private RESTfulResult AuthorizeRequest(AuthorizeType type)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            response.StatusCode = HttpStatusCode.Unauthorized;

            var token = new AuthorizeToken
            {
                RequestUri = GetRequestUri(),
                Method = request.Method,
                Headers = request.Headers,
                Parameters = request.UriTemplateMatch.QueryParameters,
                Cookies = GetCookies(),
                AuthorizeType = type
            };

            //认证成功，设置上下文
            AuthorizeContext.Current = new AuthorizeContext { Token = token };

            //实例化一个结果
            var restResult = new RESTfulResult { Code = (int)response.StatusCode };

            try
            {
                var user = Authorize(token);
                response.StatusCode = HttpStatusCode.OK;

                //认证成功
                restResult.Code = (int)response.StatusCode;
                restResult.Message = "Authentication request success.";

                //认证信息
                AuthorizeContext.Current.UserName = user.UserName;
                AuthorizeContext.Current.UserState = user.UserState;
            }
            catch (AuthorizeException ex)
            {
                restResult.Code = ex.Code;
                restResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                restResult.Message = ex.Message;
            }

            return restResult;
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

                if (!string.IsNullOrEmpty(host))
                {
                    uri = new Uri(host + HttpContext.Current.Request.RawUrl);
                }
            }
            else if (WebOperationContext.Current != null)
            {
                var request = WebOperationContext.Current.IncomingRequest;
                uri = request.UriTemplateMatch.RequestUri;

                if (!string.IsNullOrEmpty(host))
                {
                    uri = new Uri(host + uri.ToString().Replace(uri.GetLeftPart(UriPartial.Authority), ""));
                }
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
    }
}