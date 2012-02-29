using System;
using System.IO;
using MySoft.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;

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
                var url = request.URI.ToLower();
                var name = url.Substring(url.IndexOf("/help/") + 6);
                SendResponse(response, caller.GetDocument(name));
            }
            else if (request.URI.Substring(request.URI.IndexOf('/') + 1).Length > 5)
            {
                HandleResponse(request, response);
            }
            else
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_ACCEPTABLE;
                var error = new HttpServiceException { Message = response.Reason };
                SendResponse(response, error);
            }
        }

        private void HandleResponse(HTTPServerRequest request, HTTPServerResponse response)
        {
            response.ContentType = "application/json;charset=utf-8";

            var pathAndQuery = request.URI.TrimStart('/');
            var array = pathAndQuery.Split('?');
            var methodName = array[0];
            var paramString = array.Length > 1 ? array[1] : null;
            var callMethod = caller.GetCaller(methodName);

            if (callMethod == null)
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                var error = new HttpServiceException { Message = string.Format("{0}【{1}】", response.Reason, methodName) };
                SendResponse(response, error);
                return;
            }
            else
            {
                if (callMethod.HttpMethod == HttpMethod.POST && request.Method.ToUpper() == "GET")
                {
                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_METHOD_NOT_ALLOWED;
                    var error = new HttpServiceException { Message = response.Reason };
                    SendResponse(response, error);
                    return;
                }
            }

            try
            {
                //调用方法
                var collection = ParseCollection(paramString);

                if (callMethod.HttpMethod == HttpMethod.POST)
                {
                    //接收流内部数据
                    using (var stream = request.GetRequestStream())
                    using (var sr = new StreamReader(stream))
                    {
                        string streamValue = sr.ReadToEnd();
                        var jobject = JObject.Parse(streamValue);

                        //处理POST的数据
                        foreach (var kvp in jobject)
                        {
                            collection[kvp.Key] = kvp.Value;
                        }
                    }
                }

                string responseString = caller.CallMethod(methodName, collection.ToString());
                SendResponse(response, responseString);
            }
            catch (HTTPMessageException ex)
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                var error = new HttpServiceException { Message = string.Format("{0} - {1}", response.Reason, ex.Message) };
                SendResponse(response, error);
            }
            catch (Exception ex)
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                var e = ErrorHelper.GetInnerException(ex);
                var error = new HttpServiceException { Message = string.Format("{0} - {1}", e.GetType().Name, e.Message) };
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

        private void SendResponse(HTTPServerResponse response, HttpServiceException error)
        {
            error.Code = (int)response.Status;

            var jsonString = SerializationManager.SerializeJson(error);
            SendResponse(response, jsonString);
        }

        private JObject ParseCollection(string paramString)
        {
            if (!string.IsNullOrEmpty(paramString))
            {
                var arr = paramString.Split('&');

                var values = new JObject();
                foreach (var str in arr)
                {
                    var arr2 = str.Split('=');
                    if (arr2.Length == 2)
                        values[arr2[0]] = arr2[1];
                }

                return values;
            }

            return new JObject();
        }

        #endregion
    }
}
