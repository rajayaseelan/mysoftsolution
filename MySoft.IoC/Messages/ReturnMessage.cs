using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 返回消息
    /// </summary>
    [Serializable]
    public sealed class ReturnMessage : ServiceMessage
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 是否有异常
        /// </summary>
        public bool IsError
        {
            get
            {
                return this.Error != null;
            }
        }
    }
}
