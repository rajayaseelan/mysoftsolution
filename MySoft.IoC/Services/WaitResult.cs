using System;
using System.Threading;
using MySoft.IoC.Messages;

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
        /// 请求消息
        /// </summary>
        internal RequestMessage Request
        {
            get { return reqMsg; }
        }

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get
            {
                if (resMsg == null) return null;

                //如果传输Id一样直接返回
                if (reqMsg.TransactionId == resMsg.TransactionId)
                {
                    return resMsg;
                }

                //实例化新消息
                return new ResponseMessage
                {
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ElapsedTime = resMsg.ElapsedTime,
                    Value = resMsg.Value,
                    Error = resMsg.Error
                };
            }
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

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.reqMsg = null;
            this.resMsg = null;

            this.ev.Close();
            this.ev = null;
        }

        #endregion
    }
}
