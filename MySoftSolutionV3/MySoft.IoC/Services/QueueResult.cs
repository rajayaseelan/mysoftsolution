using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
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
            set
            {
                if (value != null)
                {
                    var tmpMsg = new ResponseMessage
                    {
                        TransactionId = transactionId,
                        ServiceName = value.ServiceName,
                        MethodName = value.MethodName,
                        Parameters = value.Parameters,
                        ReturnType = value.ReturnType,
                        Error = value.Error,
                        Value = value.Value
                    };

                    message = tmpMsg;
                }
            }
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
        /// <returns></returns>
        public bool Set()
        {
            return reset.Set();
        }
    }
}
