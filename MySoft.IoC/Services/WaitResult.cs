using MySoft.IoC.Messages;
using System;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    internal class WaitResult : IDisposable
    {
        private EventWaitHandle ev;
        private RequestMessage reqMsg;
        private ResponseMessage resMsg;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Response
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
            this.ev = new ManualResetEvent(false);
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool WaitOne(TimeSpan timeout)
        {
            try
            {
                if (timeout == TimeSpan.Zero)
                    return ev.WaitOne();
                else
                    return ev.WaitOne(timeout, false);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 响应信号
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            try
            {
                this.resMsg = resMsg;

                return ev.Set();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 异常响应
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool Set(Exception ex)
        {
            var resMsg = IoCHelper.GetResponse(reqMsg, ex);
            return Set(resMsg);
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.reqMsg = null;
            this.ev.Close();
        }

        #endregion
    }
}
