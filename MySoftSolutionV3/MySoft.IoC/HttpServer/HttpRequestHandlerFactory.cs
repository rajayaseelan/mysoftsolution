using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Net.Http;
using MySoft.IoC.Configuration;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// http解析工厂类
    /// </summary>
    public class HttpRequestHandlerFactory : IHTTPRequestHandlerFactory
    {
        private IHTTPRequestHandler handler;

        #region IHTTPRequestHandlerFactory 成员

        /// <summary>
        /// 初始化CastleServiceHandler
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        public HttpRequestHandlerFactory(CastleServiceConfiguration config, IServiceContainer container)
        {
            this.handler = new HttpServiceHandler(config, container);
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
                return handler;
            else
                return null;
        }

        #endregion
    }
}
