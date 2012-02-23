using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Net.HTTP;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC.Http
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


            if (request.URI.ToLower() == "/help")
            {
                //发送文档信息
                response.ContentType = "text/html;charset=utf-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                SendResponse(response, caller.GetDocument());
            }
            else if (request.URI.ToLower() == "/favicon.ico")
            {
                response.ContentType = "text/html;charset=utf-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                response.Send();
            }
            else if (request.URI.Substring(request.URI.IndexOf('/') + 1).Length > 5)
            {
                response.ContentType = "application/json;charset=utf-8";

                var paramString = request.URI.Substring(request.URI.IndexOf("/") + 1);
                var arr = paramString.Split('?');
                var call = caller.GetCaller(arr[0]);

                if (call == null)
                {
                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                    var error = new HttpServiceException { Message = string.Format("HTTP_NOT_FOUND {0}", arr[0]) };
                    SendResponse(response, error);
                    return;
                }
                else
                {
                    if (call.HttpMethod == HttpMethod.POST && request.Method.ToUpper() == "GET")
                    {
                        response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_METHOD_NOT_ALLOWED;
                        var error = new HttpServiceException { Message = "HTTP_METHOD_NOT_ALLOWED" };
                        SendResponse(response, error);
                        return;
                    }
                }

                try
                {
                    //调用方法
                    var collection = new Dictionary<string, string>();
                    if (arr.Length > 1) collection = ParseDictionary(arr[1]);
                    var headers = request.HeaderValues;
                    if (call.Authorized)
                    {
                        if (headers.ContainsKey("AuthParameter"))
                        {
                            collection[call.AuthParameter] = headers["AuthParameter"];
                        }
                        else
                        {
                            response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_UNAUTHORIZED;
                            var error = new HttpServiceException { Message = "HTTP_UNAUTHORIZED" };
                            SendResponse(response, error);
                            return;
                        }
                    }

                    if (call.HttpMethod == HttpMethod.POST)
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
                                collection[kvp.Key] = kvp.Value.ToString(Newtonsoft.Json.Formatting.None);
                            }
                        }
                    }

                    string responseString = caller.CallMethod(arr[0], collection);
                    SendResponse(response, responseString);
                }
                catch (HTTPMessageException ex)
                {
                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                    var error = new HttpServiceException { Message = "HTTPMessageException - " + ex.Message };
                    SendResponse(response, error);
                }
                catch (Exception ex)
                {
                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                    var e = ErrorHelper.GetInnerException(ex);
                    var error = new HttpServiceException { Message = e.GetType().Name + " - " + e.Message };
                    SendResponse(response, error);
                }
            }
            else
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_ACCEPTABLE;
                var error = new HttpServiceException { Message = "HTTP_NOT_ACCEPTABLE" };
                SendResponse(response, error);
            }
        }

        private void SendResponse(HTTPServerResponse response, string responseString)
        {
            using (Stream ostr = response.Send())
            using (TextWriter tw = new StreamWriter(ostr))
            {
                tw.WriteLine(responseString);
            }
        }

        private void SendResponse(HTTPServerResponse response, HttpServiceException error)
        {
            error.Code = (int)response.Status;
            using (Stream ostr = response.Send())
            using (TextWriter tw = new StreamWriter(ostr))
            {
                tw.WriteLine(SerializationManager.SerializeJson(error));
            }
        }

        private Dictionary<string, string> ParseDictionary(string paramString)
        {
            if (!string.IsNullOrEmpty(paramString))
            {
                var arr = paramString.Split('&');

                var values = new Dictionary<string, string>(arr.Length);
                foreach (var str in arr)
                {
                    var arr2 = str.Split('=');
                    if (arr2.Length == 2)
                        values[arr2[0]] = arr2[1];
                }

                return values;
            }

            return null;
        }

        #endregion
    }
}
