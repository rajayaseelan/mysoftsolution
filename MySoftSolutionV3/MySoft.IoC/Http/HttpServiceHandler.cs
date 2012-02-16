using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Net.HTTP;
using System.IO;
using System.Collections.Specialized;

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
            if (request.URI.ToLower() == "/help")
            {
                response.ContentType = "text/html";
                using (Stream ostr = response.Send())
                using (TextWriter tw = new StreamWriter(ostr))
                {
                    tw.WriteLine(caller.GetDocument());
                }
            }
            else if (request.URI.Substring(request.URI.IndexOf('/') + 1).Length > 5)
            {
                /**
                             * In this example we'll write the body into the
                             * stream obtained by response.Send(). This will cause the
                             * KeepAlive to be false since the size of the body is not
                             * known when the response header is sent.
                             **/

                try
                {
                    var paramString = request.URI.Substring(request.URI.IndexOf("/") + 1);
                    var arr = paramString.Split('?');
                    var collection = ParseCollection(arr[1]);

                    //调用方法
                    string responseString = caller.CallMethod(arr[0], collection);

                    string callback = collection["callback"];
                    if (string.IsNullOrEmpty(callback))
                    {
                        response.ContentType = "application/json;charset=utf-8";
                    }
                    else
                    {
                        response.ContentType = "application/javascript;charset=utf-8";
                        responseString = string.Format("{0}({1});", callback, responseString ?? "{}");
                    }

                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        tw.WriteLine(responseString);
                    }
                }
                catch (HTTPMessageException ex)
                {
                    response.ContentType = "application/json;charset=utf-8";
                    var error = new HttpServiceException
                    {
                        Code = (int)HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST,
                        Message = ex.Message
                    };

                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        tw.WriteLine(SerializationManager.SerializeJson(error));
                    }
                }
                catch
                {
                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_INTERNAL_SERVER_ERROR;
                    response.Send();
                }
            }
            else
            {
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                response.Send();
            }
        }

        private NameValueCollection ParseCollection(string paramString)
        {
            if (!string.IsNullOrEmpty(paramString))
            {
                var arr = paramString.Split('&');

                var nameValue = new NameValueCollection(arr.Length);
                foreach (var str in arr)
                {
                    var arr2 = str.Split('=');
                    nameValue[arr2[0]] = arr2[1];
                }

                return nameValue;
            }

            return null;
        }

        #endregion
    }
}
