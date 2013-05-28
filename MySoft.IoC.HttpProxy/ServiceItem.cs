using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySoft.IoC.HttpProxy
{
    /// <summary>
    /// 服务项
    /// </summary>
    [Serializable]
    public class ServiceItem
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerUri { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string CallerName { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheTime { get; set; }

        /// <summary>
        /// 认证参数
        /// </summary>
        public string AuthParameter { get; set; }

        /// <summary>
        /// 是否认证
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// 响应方式
        /// </summary>
        public HttpMethod HttpMethod { get; set; }
    }
}