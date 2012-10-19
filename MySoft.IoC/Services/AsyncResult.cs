using System;
using System.Threading;
using MySoft.IoC.Messages;
using System.Collections;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    internal sealed class AsyncResult
    {
        private ManualResetEvent ev;
        private ResponseMessage resMsg;

        /// <summary>
        /// 实例化AsyncResult
        /// </summary>
        public AsyncResult()
        {
            this.ev = new ManualResetEvent(false);
        }

        /// <summary>
        /// 等待响应
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage GetResponse(RequestMessage reqMsg)
        {
            try
            {
                WaitHandle.WaitAll(new[] { ev });
            }
            catch (Exception ex)
            {
                ev = new ManualResetEvent(false);
            }

            //如果返回值为null
            if (resMsg == null) return null;

            //如果是当前请求，直接返回
            if (reqMsg.TransactionId == resMsg.TransactionId)
            {
                return resMsg;
            }
            else
            {
                //获取响应信息
                return new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = resMsg.ReturnType,
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ElapsedTime = resMsg.ElapsedTime,
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };
            }
        }

        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="resMsg"></param>
        /// <param name="isSet"></param>
        /// <returns></returns>
        public bool SetResponse(ResponseMessage resMsg)
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
    }
}