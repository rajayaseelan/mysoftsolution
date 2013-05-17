using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存响应项
    /// </summary>
    [Serializable]
    internal class CacheResponse : ResponseMessage
    {
        /// <summary>
        /// 实例化CacheResponse
        /// </summary>
        /// <param name="resMsg"></param>
        public CacheResponse(ResponseMessage resMsg)
        {
            this.TransactionId = resMsg.TransactionId;
            this.ServiceName = resMsg.ServiceName;
            this.MethodName = resMsg.MethodName;
            this.Parameters = resMsg.Parameters;
            this.ElapsedTime = resMsg.ElapsedTime;
            this.Error = resMsg.Error;
            this.Value = resMsg.Value;

            if (resMsg.Value != null)
            {
                //存储buffer值
                this.Buffer = SerializationManager.SerializeBin(resMsg.Value);
            }
        }

        /// <summary>
        /// 缓存数据
        /// </summary>
        public byte[] Buffer { get; private set; }
    }
}
