using System;
using System.Reflection;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// 调用信息
    /// </summary>
    [Serializable]
    public class HttpCallerInfo
    {
        /// <summary>
        /// 调用名称
        /// </summary>
        public string CallerName { get; set; }

        /// <summary>
        /// 缓存时间，单位（秒）
        /// </summary>
        public int CacheTime { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public Type Service { get; set; }

        /// <summary>
        /// 调用方法
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public ParameterInfo[] Parameters { get; set; }

        /// <summary>
        /// 方法描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否认证
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// 认证参数
        /// </summary>
        public string AuthParameter { get; set; }

        /// <summary>
        /// Http方式
        /// </summary>
        public HttpMethod HttpMethod { get; set; }
    }
}
