using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 回调消息
    /// </summary>
    [Serializable]
    public class CallbackMessage
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public object[] Parameters { get; set; }
    }
}
