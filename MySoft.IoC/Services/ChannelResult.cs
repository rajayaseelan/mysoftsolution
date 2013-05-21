using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 通道等待响应
    /// </summary>
    internal class ChannelResult : IDisposable
    {
        private WaitResult waitResult;
        private int count;
        private byte[] buffer;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseItem Message
        {
            get
            {
                //实例化ResponseItem
                return new ResponseItem(waitResult.Message)
                {
                    Count = count,
                    Buffer = buffer
                };
            }
        }

        /// <summary>
        /// 实例化ChannelResult
        /// </summary>
        /// <param name="reqMsg"></param>
        public ChannelResult(RequestMessage reqMsg)
        {
            this.waitResult = new WaitResult(reqMsg);
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        /// <returns></returns>
        public bool WaitOne()
        {
            return waitResult.WaitOne(TimeSpan.Zero);
        }

        /// <summary>
        /// 响应信号
        /// </summary>
        /// <param name="resItem"></param>
        /// <returns></returns>
        public bool Set(ResponseItem resItem)
        {
            this.count = resItem.Count;
            this.buffer = resItem.Buffer;

            return waitResult.Set(resItem.Message);
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.waitResult.Dispose();

            this.waitResult = null;
            this.buffer = null;
        }

        #endregion
    }
}
