using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Http
{
    /// <summary>
    /// Http处理接口
    /// </summary>
    public interface IHttpAuthentication
    {
        /// <summary>
        /// 认证sessionKey，并返回认证的值
        /// </summary>
        /// <param name="container"></param>
        /// <param name="sessionKey"></param>
        /// <returns>返回认证的值</returns>
        string Authorize(IContainer container, string sessionKey);
    }
}
