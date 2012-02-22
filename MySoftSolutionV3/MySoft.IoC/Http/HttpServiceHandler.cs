using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Net.HTTP;
using System.IO;
using System.Collections.Specialized;
using System.Net;

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
                response.ContentType = "text/html;charset=utf-8";
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
                    response.ContentType = "application/json;charset=utf-8";
                    var paramString = request.URI.Substring(request.URI.IndexOf("/") + 1);
                    var arr = paramString.Split('?');
                    var collection = ParseCollection(arr[1], '&');
                    var cookies = ConvertCookies(ParseCollection(request.Get("Cookie"), ';'));

                    var call = caller.GetCaller(arr[0]);
                    if (call == null)
                    {
                    }
                    else
                    {
                        if (call.HttpMethod != request.Method.ToUpper())
                        {
                            response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_METHOD_NOT_ALLOWED;
                            response.Send();
                            return;
                        }

                        //接收流内部数据
                        using (var stream = request.GetRequestStream())
                        using (var sr = new StreamReader(stream))
                        {
                            string streamValue = sr.ReadToEnd();
                        }
                    }

                    //调用方法
                    string responseString = caller.CallMethod(arr[0], collection, cookies);
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        tw.WriteLine(responseString);
                    }
                }
                catch (HTTPMessageException ex)
                {
                    response.ContentType = "application/json;charset=utf-8";
                    if (ex is HTTPAuthMessageException)
                        response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_UNAUTHORIZED;
                    else
                        response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;

                    var error = new HttpServiceException
                    {
                        Code = (int)response.Status,
                        Message = ex.Message
                    };

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

        private NameValueCollection ParseCollection(string paramString, char split)
        {
            if (!string.IsNullOrEmpty(paramString))
            {
                var arr = paramString.Split('&');

                var values = new NameValueCollection(arr.Length);
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

        private CookieCollection ConvertCookies(NameValueCollection values)
        {
            var cookies = new CookieCollection();
            foreach (var key in values.AllKeys)
            {
                cookies.Add(new Cookie(key, values[key]));
            }
            return cookies;
        }

        #endregion
    }
}
