using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;

namespace MySoft.IoC.Http
{
    /// <summary>
    /// Http处理接口
    /// </summary>
    public interface IHttpAuthentication
    {
        /// <summary>
        /// 认证parameters，并返回认证的值，此值与AuthParameter对应
        /// </summary>
        /// <param name="container"></param>
        /// <param name="cookies"></param>
        /// <returns>返回认证的值</returns>
        string Authorize(IContainer container, CookieCollection cookies);
    }
}
