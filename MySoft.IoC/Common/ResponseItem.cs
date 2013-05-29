using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存项
    /// </summary>
    [Serializable]
    internal class ResponseItem
    {
        public ResponseItem() { }

        public ResponseItem(ResponseMessage resMsg)
        {
            if (resMsg != null)
            {
                this.Message = resMsg;
                this.Count = resMsg.Count;
                this.ElapsedTime = resMsg.ElapsedTime;
            }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public long ElapsedTime { get; set; }

        /// <summary>
        /// 缓存数据
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public ResponseMessage Message { get; set; }
    }
}
