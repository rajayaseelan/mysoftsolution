using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// Invoke 值
    /// </summary>
    [Serializable]
    public class InvokeData
    {
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string OutParameters { get; set; }
    }

    /// <summary>
    /// 调用消息
    /// </summary>
    [Serializable]
    public class InvokeMessage
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 参数值，必须为json格式
        /// </summary>
        public string Parameters { get; set; }
    }
}
