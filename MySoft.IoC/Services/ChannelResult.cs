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
        private RequestMessage reqMsg;
        private int count;
        private byte[] buffer;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseItem Message
        {
            get
            {
                if (waitResult.Message == null)
                {
                    //实例化ResponseItem
                    return new ResponseItem()
                                {
                                    Count = count,
                                    Buffer = buffer
                                };
                }
                else
                {
                    //实例化ResponseItem
                    return new ResponseItem(waitResult.Message)
                                {
                                    Count = count,
                                    Buffer = buffer
                                };
                }
            }
        }

        /// <summary>
        /// 实例化ChannelResult
        /// </summary>
        /// <param name="reqMsg"></param>
        public ChannelResult(RequestMessage reqMsg)
        {
            this.reqMsg = reqMsg;
            this.waitResult = new WaitResult();
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

            if (resItem.Message == null)
            {
                return waitResult.Set(null);
            }

            //判断传输Key是否一致
            if (resItem.Message.TransactionId == reqMsg.TransactionId)
            {
                return waitResult.Set(resItem.Message);
            }
            else
            {
                var resMsg = new ResponseMessage
                                {
                                    TransactionId = reqMsg.TransactionId,
                                    ServiceName = resItem.Message.ServiceName,
                                    MethodName = resItem.Message.MethodName,
                                    Parameters = resItem.Message.Parameters,
                                    ElapsedTime = resItem.Message.ElapsedTime,
                                    Error = resItem.Message.Error,
                                    Value = resItem.Message.Value
                                };

                return waitResult.Set(resMsg);
            }
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
            this.reqMsg = null;
        }

        #endregion
    }
}
