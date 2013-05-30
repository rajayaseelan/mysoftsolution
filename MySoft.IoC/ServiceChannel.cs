using System;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        private IScsServerClient channel;
        private string messageId;
        private RequestMessage reqMsg;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        public ServiceChannel(IScsServerClient channel, string messageId, RequestMessage reqMsg)
        {
            this.channel = channel;
            this.messageId = messageId;
            this.reqMsg = reqMsg;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="item"></param>
        public void SendResponse(ResponseItem item)
        {
            if (channel.CommunicationState != CommunicationStates.Connected) return;

            //设置异常消息
            SetMessageError(item);

            //如果没有返回消息，则退出
            if (item.Buffer == null && item.Message == null) return;

            IScsMessage message = null;

            if (item.Buffer != null)
                message = new ScsRawDataMessage(item.Buffer, messageId);
            else
                message = new ScsResultMessage(item.Message, messageId);

            //发送消息
            channel.SendMessage(message);
        }

        /// <summary>
        /// 设置异常消息
        /// </summary>
        /// <param name="item"></param>
        private void SetMessageError(ResponseItem item)
        {
            if (item.Message == null) return;

            //如果是Json方式调用，则需要处理异常
            if (reqMsg.InvokeMethod && item.Message.IsError)
            {
                //获取最底层异常信息
                var error = ErrorHelper.GetInnerException(item.Message.Error);

                item.Message.Error = new Exception(error.Message);
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// dispose resource.
        /// </summary>
        public void Dispose()
        {
            this.channel = null;
            this.reqMsg = null;
        }

        #endregion
    }
}
