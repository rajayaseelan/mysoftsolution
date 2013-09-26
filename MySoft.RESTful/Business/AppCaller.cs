using System;
using System.Reflection;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 调用者信息
    /// </summary>
    public class AppCaller
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 调用分类名称
        /// </summary>
        public string ApiKind { get; set; }

        /// <summary>
        /// 调用方法名称
        /// </summary>
        public string ApiMethod { get; set; }

        /// <summary>
        /// 调用参数信息
        /// </summary>
        public string ApiParameters { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIP { get; set; }

        /// <summary>
        /// 请求Url
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// 强类型服务
        /// </summary>
        public Type Service { get; set; }

        /// <summary>
        /// 强类型方法
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 强类型参数
        /// </summary>
        public object[] Parameters { get; set; }
    }
}
