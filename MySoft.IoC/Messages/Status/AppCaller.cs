using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 应用服务调用信息
    /// </summary>
    [Serializable]
    public class AppCaller : AppClient
    {
        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime CallTime { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public string Parameters { get; set; }
    }
}
