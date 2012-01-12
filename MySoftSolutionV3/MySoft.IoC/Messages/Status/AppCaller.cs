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
        /// 服务名称
        /// </summary>
        public string ServiceName { get; internal set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; internal set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public string Parameters { get; internal set; }
    }
}
