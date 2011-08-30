using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using MySoft.Logger;
using MySoft.RESTful.Business;

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
        /// 是否记录错误日志
        /// </summary>
        public bool IsRecordErrorLog { get; set; }

        /// <summary>
        /// 实例化DefaultRESTfulService
        /// </summary>
        public DefaultRESTfulService()
        {
            //创建上下文
            this.Context = new BusinessRESTfulContext();

            this.IsRecordErrorLog = true;
        }

        #region IRESTfulService 成员

        /// <summary>
        /// 实现Post方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Stream PostJsonEntry(string kind, string method, Stream parameters)
        {
            return GetResponseStream(ParameterFormat.Json, kind, method, parameters);
        }

        /// <summary>
        /// 实现Delete方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Stream DeleteJsonEntry(string kind, string method, Stream parameters)
        {
            return PostJsonEntry(kind, method, parameters);
        }

        /// <summary>
        /// 实现Put方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Stream PutJsonEntry(string kind, string method, Stream parameters)
        {
            return PostJsonEntry(kind, method, parameters);
        }

        /// <summary>
        /// 实现Get方式Json响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetJsonEntry(string kind, string method)
        {
            return PostJsonEntry(kind, method, null);
        }

        /// <summary>
        /// 实现Post方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Stream PostXmlEntry(string kind, string method, Stream parameters)
        {
            return GetResponseStream(ParameterFormat.Xml, kind, method, parameters);
        }

        /// <summary>
        /// 实现Delete方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Stream DeleteXmlEntry(string kind, string method, Stream parameters)
        {
            return PostXmlEntry(kind, method, parameters);
        }

        /// <summary>
        /// 实现Put方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Stream PutXmlEntry(string kind, string method, Stream parameters)
        {
            return PostXmlEntry(kind, method, parameters);
        }

        /// <summary>
        /// 实现Get方式Xml响应
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Stream GetXmlEntry(string kind, string method)
        {
            return PostXmlEntry(kind, method, null);
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
                var ret = new RESTfulResult { Code = RESTfulCode.OK.ToString(), Message = "Not found [callback] parameter!" };
                //throw new WebFaultException<RESTfulResult>(ret, HttpStatusCode.Forbidden);
                response.StatusCode = HttpStatusCode.Forbidden;
                response.ContentType = "application/json;charset=utf-8";
                result = SerializationManager.SerializeJson(ret);
            }
            else
            {
                result = GetResponseString(ParameterFormat.Jsonp, kind, method, null) as string;
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
            var request = WebOperationContext.Current.IncomingRequest;
            var html = Context.MakeApiDocument(request.UriTemplateMatch.RequestUri);
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

            string result = GetResponseString(format, kind, method, data) as string;
            if (string.IsNullOrEmpty(result)) return new MemoryStream();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result)); ;
            return ms;
        }

        private string GetResponseString(ParameterFormat format, string kind, string method, string parameters)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;

            if (format == ParameterFormat.Json || format == ParameterFormat.Jsonp)
                response.ContentType = "application/json;charset=utf-8";
            else if (format == ParameterFormat.Xml)
                response.ContentType = "text/xml;charset=utf-8";
            else if (format == ParameterFormat.Text)
                response.ContentType = "text/plain";
            else if (format == ParameterFormat.Html)
                response.ContentType = "text/html";

            object result = null;

            //进行认证处理
            RESTfulResult authResult = new RESTfulResult { Code = RESTfulCode.OK.ToString() };

            //进行认证处理
            if (Context != null && Context.IsAuthorized(format, kind, method))
            {
                authResult = AuthenticationManager.Authorize();
            }

            //认证成功
            if (authResult.Code == RESTfulCode.OK.ToString())
            {
                try
                {
                    result = Context.Invoke(format, kind, method, parameters);
                    if (result == null)
                    {
                        if (format == ParameterFormat.Json || format == ParameterFormat.Jsonp)
                            result = "{}";
                        else if (format == ParameterFormat.Html)
                            response.ContentType = "text/html";
                        else
                            response.ContentType = "text/plain";

                        return result as string;
                    }
                }
                catch (RESTfulException e)
                {
                    result = new RESTfulResult { Code = e.Code.ToString(), Message = e.Message };
                    //result = new WebFaultException<RESTfulResult>(ret, HttpStatusCode.BadRequest);
                    response.StatusCode = HttpStatusCode.BadRequest;

                    //记录错误日志
                    if (IsRecordErrorLog)
                    {
                        SimpleLog.Instance.WriteLogForDir("RESTfulError", e);
                    }
                }
                catch (Exception e)
                {
                    result = new RESTfulResult { Code = RESTfulCode.BUSINESS_ERROR.ToString(), Message = e.Message };
                    //result = new WebFaultException<RESTfulResult>(ret, HttpStatusCode.ExpectationFailed);
                    response.StatusCode = HttpStatusCode.ExpectationFailed;

                    //记录错误日志
                    if (IsRecordErrorLog && !(e is BusinessException))
                    {
                        SimpleLog.Instance.WriteLogForDir("RESTfulError", e);
                    }
                }
            }
            else
            {
                result = authResult;
            }

            ISerializer serializer = SerializerFactory.Create(format);
            try
            {
                result = serializer.Serialize(result);
                return result.ToString();
            }
            catch (Exception ex)
            {
                //如果系列化失败
                result = new RESTfulResult { Code = RESTfulCode.BUSINESS_ERROR.ToString(), Message = ex.Message };
                result = serializer.Serialize(result);
                return result.ToString();
            }
        }
    }
}