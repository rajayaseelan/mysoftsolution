using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Http接口解析器
    /// </summary>
    public interface IHttpApiResolver
    {
        /// <summary>
        /// 解析接口服务为方法
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        IList<HttpApiMethod> MethodResolver(Type interfaceType);
    }
}
