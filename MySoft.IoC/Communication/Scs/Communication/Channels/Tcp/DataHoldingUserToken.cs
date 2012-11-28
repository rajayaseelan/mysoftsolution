using System;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// 缓冲数据包
    /// </summary>
    internal class DataHoldingUserToken : IDisposable
    {
        private IScsMessage message;
        private byte[] buffer;
        private int bufferLength;
        private int bufferIndex;

        /// <summary>
        /// 实例化BufferUserToken
        /// </summary>
        /// <param name="message"></param>
        /// <param name="buffer"></param>
        public DataHoldingUserToken(IScsMessage message, byte[] buffer)
        {
            this.message = message;
            this.buffer = buffer;
            this.bufferLength = buffer.Length;
            this.bufferIndex = 0;
        }

        /// <summary>
        /// 请求的消息
        /// </summary>
        public IScsMessage Message
        {
            get { return message; }
        }

        /// <summary>
        /// 获取缓冲数据
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public byte[] GetRemainingBuffer(int bufferSize)
        {
            try
            {
                //计算缓冲区大小
                if (bufferLength - bufferIndex < bufferSize)
                {
                    bufferSize = bufferLength - bufferIndex;
                }

                //判断是否结束
                if (bufferSize > 0)
                {
                    var bytes = new byte[bufferSize];
                    Buffer.BlockCopy(buffer, bufferIndex, bytes, 0, bufferSize);
                    bufferIndex += bufferSize;

                    return bytes;
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            this.message = null;
            this.buffer = null;
        }

        #endregion
    }
}
