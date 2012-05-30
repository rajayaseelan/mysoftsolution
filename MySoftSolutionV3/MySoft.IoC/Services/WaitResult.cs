using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    [Serializable]
    internal sealed class WaitResult : IDisposable
    {
        private AutoResetEvent reset;
        private ResponseMessage message;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get { return message; }
        }

        /// <summary>
        /// 实例化QueueResult
        /// </summary>
        /// <param name="reqMsg"></param>
        public WaitResult(RequestMessage reqMsg)
        {
            this.reset = new AutoResetEvent(false);
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool Wait(TimeSpan timeSpan)
        {
            return reset.WaitOne(timeSpan);
        }

        /// <summary>
        /// 响应信号
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            this.message = resMsg;
            return reset.Set();
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.message = null;
        }

        #endregion
    }
}
