using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 调用方法
        /// </summary>
        public MethodInfo Method { get; set; }

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
