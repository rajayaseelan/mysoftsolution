using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using MySoft.Auth;
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
            return GetResponseStream(ParameterFormat.Json, kind, method, null);
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
                result = GetResponseString(ParameterFormat.Jsonp, kind, method, null);
                response.ContentType = "application/javascript;charset=utf-8";
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
            var request = WebOperationContext.Current.IncomingRequest;
            var html = Context.MakeDocument(request.UriTemplateMatch.RequestUri, kind, method);
            var response = WebOperationContext.Current.OutgoingResponse;
            response.ContentType = "text/html;charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }

        #endregion

        private Stream GetResponseStream(ParameterFormat format, string kind, string method, Stream stream)
        {
            string data = null;
            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    data = sr.ReadToEnd();
                }
            }

            string result = GetResponseString(format, kind, method, data);
            if (string.IsNullOrEmpty(result)) return new MemoryStream();

            //处理ETag功能
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var buffer = Encoding.UTF8.GetBytes(result);

            if (request.Method.ToUpper() == "GET" && response.StatusCode == HttpStatusCode.OK)
            {
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

        private string GetResponseString(ParameterFormat format, string kind, string method, string parameters)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            if (format == ParameterFormat.Json)
                response.ContentType = "application/json;charset=utf-8";
            else if (format == ParameterFormat.Xml)
                response.ContentType = "text/xml;charset=utf-8";
            else if (format == ParameterFormat.Text)
                response.ContentType = "text/plain;charset=utf-8";
            else if (format == ParameterFormat.Html)
                response.ContentType = "text/html;charset=utf-8";

            //从缓存读取
            var cacheKey = string.Format("{0}_{1}_{2}_{3}", format, kind, method, parameters);
            object result = null;

            //进行认证处理
            RESTfulResult authResult = new RESTfulResult { Code = (int)HttpStatusCode.OK };

            //进行认证处理
            if (Context != null && Context.IsAuthorized(kind, method))
            {
                authResult = AuthorizeRequest();
            }

            //认证成功
            if (authResult.Code == (int)HttpStatusCode.OK)
            {
                try
                {
                    Type retType;
                    result = Context.Invoke(kind, method, parameters, out retType);

                    //设置返回成功
                    response.StatusCode = HttpStatusCode.OK;

                    //如果值为null，以对象方式返回
                    if (result == null || retType == typeof(string))
                    {
                        return Convert.ToString(result);
                    }

                    //如果是值类型，则以对象方式返回
                    if (retType.IsValueType)
                    {
                        result = new RESTfulResponse { Value = result };
                    }
                }
                catch (Exception ex)
                {
                    //记录错误日志
                    result = GetResult(parameters, ex);

                    //转换结果
                    var ret = result as RESTfulResult;

                    //重新定义一个异常
                    var error = new Exception(string.Format("{0} - {1}", ret.Code, ret.Message), ex);

                    //记录错误日志
                    SimpleLog.Instance.WriteLog(ex);
                }
            }
            else
            {
                result = authResult;
            }

            ISerializer serializer = SerializerFactory.Create(format);
            return serializer.Serialize(result, format == ParameterFormat.Jsonp);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="exception"></param>
        private RESTfulResult GetResult(string parameter, Exception exception)
        {
            var response = WebOperationContext.Current.OutgoingResponse;
            var result = new RESTfulResult { Code = (int)HttpStatusCode.BadRequest };

            if (exception is RESTfulException)
            {
                var error = exception as RESTfulException;
                result.Code = error.Code;
                response.StatusCode = (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), error.Code);
            }

            //返回结果
            return result;
        }

        /// <summary>
        /// 进行认证
        /// </summary>
        /// <returns></returns>
        private RESTfulResult AuthorizeRequest()
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            response.StatusCode = HttpStatusCode.Unauthorized;

            var token = new AuthorizeToken
            {
                RequestUri = request.UriTemplateMatch.RequestUri,
                Method = request.Method,
                Headers = request.Headers,
                Parameters = request.UriTemplateMatch.QueryParameters,
                Cookies = GetCookies()
            };

            //实例化一个结果
            RESTfulResult restResult = new RESTfulResult { Code = (int)response.StatusCode };

            try
            {
                var result = Authorize(token);
                if (result.Succeed)
                {
                    response.StatusCode = HttpStatusCode.OK;

                    //认证成功
                    restResult.Code = (int)response.StatusCode;
                    restResult.Message = "Authentication request success.";

                    //认证成功，设置上下文
                    AuthorizeContext.Current = new AuthorizeContext
                    {
                        Result = result,
                        Token = token
                    };
                }
                else
                {
                    restResult.Message = "Authentication request fail.";
                }
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