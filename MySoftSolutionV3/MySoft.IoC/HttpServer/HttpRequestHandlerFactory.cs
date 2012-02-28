using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Net.Http;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// http解析工厂类
    /// </summary>
    public class HttpRequestHandlerFactory : IHTTPRequestHandlerFactory
    {
        private HttpServiceCaller caller;

        #region IHTTPRequestHandlerFactory 成员

        /// <summary>
        /// 初始化CastleServiceHandler
        /// </summary>
        /// <param name="caller"></param>
        public HttpRequestHandlerFactory(HttpServiceCaller caller)
        {
            this.caller = caller;
        }

        /// <summary>
        /// 返回请求句柄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHTTPRequestHandler CreateRequestHandler(HTTPServerRequest request)
        {
            //不是HttpGET或HttpPOST方式，直接返回
            if (request.Method == HTTPServerRequest.HTTP_GET || request.Method == HTTPServerRequest.HTTP_POST)
                return new HttpServiceHandler(caller);
            else
                return null;
        }

        #endregion
    }
}
