using System;

namespace MySoft.RESTful
{
    /// <summary>
    /// 默认服务上下文
    /// </summary>
    public interface IRESTfulContext
    {
        /// <summary>
        /// 生成API文档
        /// </summary>
        /// <returns></returns>
        string MakeApiDocument(Uri requestUri);

        /// <summary>
        /// 是否需要认证
        /// </summary>
        /// <param name="format"></param>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        bool IsAuthorized(ParameterFormat format, string kind, string method);

        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="format"></param>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Invoke(ParameterFormat format, string kind, string method, string parameters);
    }
}