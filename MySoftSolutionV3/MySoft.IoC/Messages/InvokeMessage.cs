using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Messages
{
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
        public string Parameter { get; set; }
    }
}
