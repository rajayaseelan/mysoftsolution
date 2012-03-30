using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    [Serializable]
    public sealed class QueueResult
    {
        private AutoResetEvent reset;
        private Guid transactionId;
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
        public QueueResult(RequestMessage reqMsg)
        {
            this.reset = new AutoResetEvent(false);
            this.transactionId = reqMsg.TransactionId;
        }

        /// <summary>
        /// 等待响应
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
            if (resMsg != null)
            {
                var tmpMsg = new ResponseMessage
                {
                    TransactionId = transactionId,
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ReturnType = resMsg.ReturnType,
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };

                this.message = tmpMsg;
            }

            return reset.Set();
        }
    }
}
