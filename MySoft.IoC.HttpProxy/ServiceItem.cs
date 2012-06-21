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
        /// 方法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否认证
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// 是否String类型
        /// </summary>
        public bool TypeString { get; set; }
    }
}