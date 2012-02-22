using System;
using System.Collections.Specialized;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using MySoft.Logger;
using MySoft.RESTful.Business;
using MySoft.RESTful.Utils;
using System.Net;
using MySoft.Security;
using MySoft.RESTful.Auth;

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
        /// 实现Delete方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream DeleteJsonEntry(string kind, string method)
        {
            return GetJsonEntry(kind, method);
        }

        /// <summary>
        /// 实现Put方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream PutJsonEntry(string kind, string method)
        {
            return GetJsonEntry(kind, method);
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
        /// 实现Delete方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream DeleteXmlEntry(string kind, string method)
        {
            return GetXmlEntry(kind, method);
        }

        /// <summary>
        /// 实现Put方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream PutXmlEntry(string kind, string method)
        {
            return GetXmlEntry(kind, method);
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

            NameValueCollection values = request.UriTemplateMatch.QueryParameters;
            var callback = values["callback"];
            string result = string.Empty;

            if (string.IsNullOrEmpty(callback))
            {
                var ret = new RESTfulResult { Code = (int)RESTfulCode.OK, Message = "Not found [callback] parameter!" };
                //throw new WebFaultException<RESTfulResult>(ret, HttpStatusCode.Forbidden);
                response.StatusCode = HttpStatusCode.Forbidden;
                response.ContentType = "application/json;charset=utf-8";
                result = SerializationManager.SerializeJson(ret);
            }
            else
            {
                result = GetResponseString(ParameterFormat.Jsonp, kind, method, null);
                response.ContentType = "application/javascript;charset=utf-8";
                result = string.Format("{0}({1});", callback, result ?? "{}");
            }

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(result));
            return stream;
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
            var html = Context.MakeApiDocument(request.UriTemplateMatch.RequestUri, kind, method);
            var response = WebOperationContext.Current.OutgoingResponse;
            response.ContentType = "text/html;charset=utf-8";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(html));
            return stream;
        }

        #endregion

        private Stream GetResponseStream(ParameterFormat format, string kind, string method, Stream stream)
        {
            string data = null;
            if (stream != null)
            {
                StreamReader sr = new StreamReader(stream);
                data = sr.ReadToEnd();
                sr.Close();
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
            var responseString = CacheHelper.Get<string>(cacheKey);
            if (responseString == null)
            {
                object result = null;

                //进行认证处理
                RESTfulResult authResult = new RESTfulResult { Code = (int)RESTfulCode.OK };

                //进行认证处理
                if (Context != null && Context.IsAuthorized(kind, method))
                {
                    authResult = AuthManager.Authorize();
                }

                //认证成功
                if (authResult.Code == (int)RESTfulCode.OK)
                {
                    try
                    {
                        Type retType;
                        result = Context.Invoke(kind, method, parameters, out retType);

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

                        //设置返回成功
                        response.StatusCode = HttpStatusCode.OK;
                    }
                    catch (Exception e)
                    {
                        //记录错误日志
                        result = GetResultWriteErrorLog(parameters, e);
                    }
                    finally
                    {
                        //清理上下文资源
                        AuthenticationContext.Current = null;
                    }
                }
                else
                {
                    result = authResult;
                }

                ISerializer serializer = SerializerFactory.Create(format);
                if (result is RESTfulResult)
                {
                    var ret = result as RESTfulResult;
                    ret.Code = Convert.ToInt32(string.Format("{0}{1}", (int)response.StatusCode, ret.Code.ToString("00")));
                }

                responseString = serializer.Serialize(result, format == ParameterFormat.Jsonp);

                //缓存5秒钟
                CacheHelper.Insert(cacheKey, responseString, 5);
            }

            return responseString;
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="exception"></param>
        private RESTfulResult GetResultWriteErrorLog(string parameter, Exception exception)
        {
            var response = WebOperationContext.Current.OutgoingResponse;
            int code = (int)RESTfulCode.BUSINESS_ERROR;
            if (exception is BusinessException)
            {
                code = (exception as BusinessException).Code;
                response.StatusCode = HttpStatusCode.ExpectationFailed;
            }
            else if (exception is RESTfulException)
            {
                code = (int)(exception as RESTfulException).Code;
                response.StatusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                response.StatusCode = HttpStatusCode.ExpectationFailed;
            }

            //重新定义一个异常
            var error = new Exception(string.Format("{0} - {1}", code, exception.Message), exception);

            //记录错误日志
            SimpleLog.Instance.WriteLog(error);

            //返回结果
            return new RESTfulResult { Code = code, Message = exception.Message };
        }
    }
}