using System;
using System.Threading;
using MySoft.IoC.Messages;
using System.Text;
using MySoft.Security;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    [Serializable]
    internal sealed class QueueResult : IDisposable
    {
        private AutoResetEvent reset;
        private Guid transactionId;
        private string queueKey;
        private ResponseMessage message;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get { return message; }
        }

        /// <summary>
        /// 唯一关键字
        /// </summary>
        public string QueueKey
        {
            get { return queueKey; }
        }

        /// <summary>
        /// 实例化QueueResult
        /// </summary>
        /// <param name="reqMsg"></param>
        public QueueResult(RequestMessage reqMsg)
        {
            this.reset = new AutoResetEvent(false);
            this.transactionId = reqMsg.TransactionId;

            //队列Key值
            var thisKey = string.Format("{0}${1}${2}", reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString());
            var formatKey = IoCHelper.ClearJSONSpace(thisKey);
            this.queueKey = MD5.HexHash(Encoding.Default.GetBytes(formatKey));
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
            if (resMsg != null)
            {
                if (resMsg.TransactionId == transactionId)
                {
                    this.message = resMsg;
                }
                else
                {
                    var tmpMsg = new ResponseMessage
                     {
                         TransactionId = transactionId,
                         ReturnType = resMsg.ReturnType,
                         ServiceName = resMsg.ServiceName,
                         MethodName = resMsg.MethodName,
                         Parameters = resMsg.Parameters,
                         Error = resMsg.Error,
                         Value = resMsg.Value
                     };

                    this.message = tmpMsg;
                }
            }

            return reset.Set();
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.reset = null;
            this.message = null;
        }

        #endregion
    }
}
