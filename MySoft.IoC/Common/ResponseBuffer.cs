using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存项
    /// </summary>
    [Serializable]
    public sealed class ResponseBuffer : ResponseMessage
    {
        /// <summary>
        /// 缓存数据
        /// </summary>
        public byte[] Buffer { get; set; }
    }
}
