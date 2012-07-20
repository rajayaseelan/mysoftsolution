using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using MySoft.Auth;
using MySoft.IoC.Configuration;
using MySoft.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MySoft.Logger;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Castle服务处理器
    /// </summary>
    public class HttpServiceHandler : IHTTPRequestHandler
    {
        private HttpServiceCaller caller;

        /// <summary>
        /// 初始化CastleServiceHandler
        /// </summary>
        /// <param name="caller"></param>
        public HttpServiceHandler(HttpServiceCaller caller)
        {
            this.caller = caller;
        }

        #region IHTTPRequestHandler 成员

        /// <summary>
        /// 实现Request响应
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public void HandleRequest(HTTPServerRequest request, HTTPServerResponse response)
        {
            /**
                     * In this example we'll write the body into the
                     * stream obtained by response.Send(). This will cause the
                     * KeepAlive to be false since the size of the body is not
                     * known when the response header is sent.
                     **/

            if (request.URI.ToLower() == "/favicon.ico")
            {
                response.ContentType = "text/html;charset=utf-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                response.SendContinue();
            }
            else if (request.URI.ToLower() == "/api")
            {
                response.ContentType = "application/json;charset=utf-8";
                SendResponse(response, caller.GetAPIText());
            }
            else if (request.URI.ToLower() == "/help")
            {
                //发送文档帮助信息
                response.ContentType = "text/html;charset=utf-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                SendResponse(response, caller.GetDocument(null));
            }
            else if (request.URI.ToLower().IndexOf("/help/") == 0)
            {
                //发送文档帮助信息
                response.ContentType = "text/html;charset=utf-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                var name = request.URI.Substring(request.URI.IndexOf("/help/") + 6);
                SendResponse(response, caller.GetDocument(name));
            }
            else if (request.URI.Substring(request.URI.IndexOf('/') + 1).Length > 5)
            {
                HandleResponse(request, response);
            }
            else
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_ACCEPTABLE;
                var error = new HttpServiceResult { Message = response.Reason };
                SendResponse(response, error);
            }
        }

        private void HandleResponse(HTTPServerRequest request, HTTPServerResponse response)
        {
            //响应格式
            response.ContentType = "application/json;charset=utf-8";

            var pathAndQuery = request.URI.TrimStart('/');
            var array = pathAndQuery.Split('?');
            var methodName = array[0];
            var paramString = array.Length > 1 ? array[1] : null;
            var callMethod = caller.GetCaller(methodName);

            if (callMethod == null)
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                var error = new HttpServiceResult { Message = string.Format("{0}【{1}】", response.Reason, methodName) };
                SendResponse(response, error);
                return;
            }
            else if (callMethod.HttpMethod == HttpMethod.POST && request.Method.ToUpper() == "GET")
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_METHOD_NOT_ALLOWED;
                var error = new HttpServiceResult { Message = response.Reason };
                SendResponse(response, error);
                return;
            }

            try
            {
                //调用方法
                NameValueCollection nvs = null;
                if (callMethod.HttpMethod == HttpMethod.GET)
                {
                    nvs = HttpUtility.ParseQueryString(paramString ?? string.Empty, Encoding.UTF8);
                }
                else
                {
                    //接收流内部数据
                    var stream = request.GetRequestStream();

                    //接收流内部数据
                    var sr = new StreamReader(stream, Encoding.UTF8);
                    string streamValue = sr.ReadToEnd();

                    //转换成NameValueCollection
                    nvs = ConvertCollection(streamValue);
                }

                if (callMethod.Authorized)
                {
                    if (!request.Has("X-AuthParameter"))
                        throw new AuthorizeException("Request header did not exist [X-AuthParameter] info.");
                    else
                        //调用认证的信息
                        nvs[callMethod.AuthParameter] = request.Get("X-AuthParameter");
                }

                //转换成JsonString
                var parameters = ConvertJsonString(nvs);
                string jsonString = caller.CallMethod(methodName, parameters);

                if (callMethod.TypeString)
                {
                    //如果返回是字符串类型，则设置为文本返回
                    response.ContentType = "text/plain;charset=utf-8";

                    //转换成string类型
                    jsonString = SerializationManager.DeserializeJson<string>(jsonString);
                }

                SendResponse(response, jsonString);
            }
            catch (HTTPMessageException ex)
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_EXPECTATION_FAILED;
                var error = new HttpServiceResult { Message = string.Format("{0} - {1}", response.Reason, ex.Message) };
                SendResponse(response, error);
            }
            catch (Exception ex)
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                var e = ErrorHelper.GetInnerException(ex);
                var error = new HttpServiceResult { Message = string.Format("{0} - {1}", e.GetType().Name, e.Message) };
                SendResponse(response, error);
            }
        }

        private void SendResponse(HTTPServerResponse response, string responseString)
        {
            using (var sw = new StreamWriter(response.Send()))
            {
                sw.Write(responseString);
            }
        }

        private void SendResponse(HTTPServerResponse response, HttpServiceResult error)
        {
            error.Code = (int)response.Status;

            var jsonString = SerializationManager.SerializeJson(error);
            SendResponse(response, jsonString);
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
                    SimpleLog.Instance.WriteLogForDir("ConvertData", ex);
                }
            }

            return values;
        }

        /// <summary>
        /// 转换成JObject
        /// </summary>
        /// <param name="nvs"></param>
        /// <returns></returns>
        private string ConvertJsonString(NameValueCollection nvs)
        {
            var obj = new JObject();
            if (nvs.Count > 0)
            {
                foreach (var key in nvs.AllKeys)
                {
                    try
                    {
                        obj[key] = nvs[key];
                    }
                    catch
                    {
                        obj[key] = JContainer.Parse(nvs[key]);
                    }
                }
            }

            //转换成Json字符串
            return obj.ToString(Formatting.Indented);
        }

        #endregion
    }
}
