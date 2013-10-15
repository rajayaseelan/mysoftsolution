using MySoft.IoC.Communication.Scs.Communication.Messages;
using System;
using System.IO;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// 消息缓存区
    /// </summary>
    [Serializable]
    internal class MessageBuffer : IDisposable
    {
        private byte[] buffer;
        private int bufferSize;
        private IScsMessage message;
        private MemoryStream stream;

        /// <summary>
        /// 初始化MessageBuffer
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageBytes"></param>
        /// <param name="bufferSize"></param>
        public MessageBuffer(IScsMessage message, byte[] messageBytes, int bufferSize)
        {
            this.message = message;
            this.buffer = new byte[bufferSize];
            this.bufferSize = bufferSize;
            this.stream = new MemoryStream(messageBytes);
            this.stream.Position = 0;
        }

        /// <summary>
        /// 获取消息对象
        /// </summary>
        public IScsMessage Message
        {
            get { return message; }
        }

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool SetBuffer(SocketAsyncEventArgs e)
        {
            e.UserToken = this;

            int count = (int)(stream.Length - stream.Position);
            if (count > 0)
            {
                count = Math.Min(count, bufferSize);

                //读取到缓冲区
                stream.Read(buffer, 0, count);

                //set buffer offset.
                Buffer.BlockCopy(buffer, 0, e.Buffer, e.Offset, count);

                if (count < e.Count)
                {
                    e.SetBuffer(e.Offset, count);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Dispose resource.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.message = null;
                this.buffer = null;
                this.stream.Dispose();
            }
            catch (Exception ex)
            {
                //TODO
            }
        }
    }
}
