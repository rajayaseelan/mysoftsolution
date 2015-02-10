using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using System;
using System.Runtime.Serialization;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        private IScsServerClient channel;
        private RequestMessage reqMsg;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reqMsg"></param>
        public ServiceChannel(IScsServerClient channel, RequestMessage reqMsg)
        {
            this.channel = channel;
            this.reqMsg = reqMsg;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="resMsg"></param>
        public void SendResponse(string messageId, ResponseMessage resMsg)
        {
            try
            {
                //设置异常消息
                SetMessageError(resMsg);

                //发送消息
                SendMessage(messageId, resMsg);
            }
            catch (SerializationException ex)
            {
                //创建一个新响应消息
                var errMsg = IoCHelper.GetResponse(reqMsg, ex);

                //发送消息
                SendMessage(messageId, errMsg);

                throw;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="resMsg"></param>
        private void SendMessage(string messageId, ResponseMessage resMsg)
        {
            IScsMessage message = new ScsResultMessage(resMsg, messageId);

            //发送消息
            channel.SendMessage(message);
        }

        /// <summary>
        /// 设置异常消息
        /// </summary>
        /// <param name="resMsg"></param>
        private void SetMessageError(ResponseMessage resMsg)
        {
            //处理json异常
            if (resMsg.IsError && reqMsg.InvokeMethod)
            {
                var error = ErrorHelper.GetInnerException(resMsg.Error);

                //返回通用异常信息
                resMsg.Error = new Exception(error.Message);
            }
        }

        public void Dispose()
        {
            this.channel = null;
            this.reqMsg = null;
        }
    }
}
