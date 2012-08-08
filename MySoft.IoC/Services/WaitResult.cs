using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    internal sealed class WaitResult : IDisposable
    {
        private AutoResetEvent ev;
        private RequestMessage reqMsg;
        private ResponseMessage resMsg;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get { return resMsg; }
        }

        /// <summary>
        /// 实例化WaitResult
        /// </summary>
        /// <param name="reqMsg"></param>
        public WaitResult(RequestMessage reqMsg)
        {
            this.reqMsg = reqMsg;
            this.ev = new AutoResetEvent(false);
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool Wait(TimeSpan timeSpan)
        {
            return ev.WaitOne(timeSpan);
        }

        /// <summary>
        /// 响应信号
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            if (ev == null) return false;

            this.resMsg = resMsg;

            if (resMsg.TransactionId != reqMsg.TransactionId)
            {
                resMsg.TransactionId = reqMsg.TransactionId;
            }

            return ev.Set();
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.ev.Reset();

            this.ev = null;
            this.reqMsg = null;
            this.resMsg = null;
        }

        #endregion
    }
}
