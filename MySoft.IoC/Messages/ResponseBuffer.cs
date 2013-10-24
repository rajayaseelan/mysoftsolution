using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 缓存项
    /// </summary>
    [Serializable]
    public sealed class ResponseBuffer : ResponseMessage
    {
        /// <summary>
        /// 实例化ResponseBuffer
        /// </summary>
        /// <param name="resMsg"></param>
        public ResponseBuffer(ResponseMessage resMsg)
        {
            ServiceName = resMsg.ServiceName;
            MethodName = resMsg.MethodName;
            Parameters = resMsg.Parameters;
            ElapsedTime = resMsg.ElapsedTime;
            Count = resMsg.Count;
            Error = resMsg.Error;
            Buffer = IoCHelper.SerializeObject(resMsg.Value);
        }

        /// <summary>
        /// 缓存数据
        /// </summary>
        public byte[] Buffer { get; set; }
    }
}
