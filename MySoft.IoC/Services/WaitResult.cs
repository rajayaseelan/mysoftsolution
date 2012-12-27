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
            this.ev = new AutoResetEvent(false);
            this.reqMsg = reqMsg;
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        /// <returns></returns>
        public bool WaitOne()
        {
            try
            {
                return ev.WaitOne(-1, false);
            }
            catch
            {
                return false;
            }
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

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.ev.Close();
            }
            catch (Exception ex) { }
            finally
            {
                this.ev = null;
                this.reqMsg = null;
                this.resMsg = null;
            }
        }

        #endregion
    }
}
