using System;
using System.Collections.Specialized;
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
using Newtonsoft.Json.Linq;

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
                result = GetResponseString(ParameterFormat.Jsonp, kind, method, query);
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
            var response = WebOperationContext.Current.OutgoingResponse;

            var html = Context.MakeDocument(request.UriTemplateMatch.RequestUri, kind, method);
            response.ContentType = "text/html;charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }

        #endregion

        private Stream GetResponseStream(ParameterFormat format, string kind, string method, Stream stream)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            NameValueCollection nvs = null;
            if (stream == null)
            {
                nvs = request.UriTemplateMatch.QueryParameters;
            }
            else
            {
                //接收流内部数据
                var sr = new StreamReader(stream);
                string streamValue = sr.ReadToEnd();

                //转换成NameValueCollection
                nvs = ConvertCollection(streamValue);
            }

            string result = GetResponseString(format, kind, method, nvs);
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

        /// <summary>
        /// 转换成NameValueCollection
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private NameValueCollection ConvertCollection(string data)
        {
            //处理成Form方式
            var values = HttpUtility.ParseQueryString(data, Encoding.UTF8);

            //为0表示为json方式
            if (values.Count == 0 || (values.Count == 1 && values.AllKeys[0] == null))
            {
                try
                {
                    //清除所的值
                    values.Clear();

                    //保持与Json兼容处理
                    var jobj = JObject.Parse(data);
                    foreach (var kvp in jobj)
                    {
                        values[kvp.Key] = kvp.Value.ToString();
                    }
                }
                catch (Exception ex)
                {
                    //TODO 不做处理
                    SimpleLog.Instance.WriteLogForDir("DataConvert", ex);
                }
            }

            return values;
        }

        private string GetResponseString(ParameterFormat format, string kind, string method, NameValueCollection parameters)
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
                        if (retType == typeof(string))
                        {
                            switch (format)
                            {
                                case ParameterFormat.Text:
                                case ParameterFormat.Html:
                                    return Convert.ToString(result);
                            }
                        }

                        //如果是值类型，则以对象方式返回
                        if (retType.IsValueType || retType == typeof(string))
                        {
                            result = new RESTfulResponse { Value = result };
                        }
                    }
                    catch (Exception ex)
                    {
                        RESTfulResult ret;
                        var errorMessage = GetErrorMessage(ex, kind, method, parameters, out ret);

                        result = ret;

                        //重新定义一个异常
                        var error = new Exception(errorMessage, ex);

                        //记录错误日志
                        SimpleLog.Instance.WriteLogForDir("RESTful\\" + kind, error);
                    }
                }
                else
                {
                    result = authResult;
                }
            }

            ISerializer serializer = SerializerFactory.Create(format);
            return serializer.Serialize(result, format == ParameterFormat.Jsonp);
        }

        /// <summary>
        /// 获取错误消息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetErrorMessage(Exception exception, string kind, string method, NameValueCollection nvs, out RESTfulResult ret)
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

            //如果参数大于0
            if (nvs.Count > 0)
            {
                errorMessage = string.Format("{0}\r\n\tParameters:{1}", errorMessage, GetParameters(nvs));
            }

            //加上认证的用户名
            if (AuthorizeContext.Current != null && AuthorizeContext.Current.Result.Succeed)
            {
                errorMessage = string.Format("{0}\r\n\tUser:[{1}]", errorMessage, AuthorizeContext.Current.Result.Name);
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
            var restResult = new RESTfulResult { Code = (int)response.StatusCode };

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