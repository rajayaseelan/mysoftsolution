using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel
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
            //设置异常消息
            SetMessageError(resMsg);

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
            //如果是Json方式调用，则需要处理异常
            if (reqMsg.InvokeMethod && resMsg.IsError)
            {
                //获取最底层异常信息
                var error = ErrorHelper.GetInnerException(resMsg.Error);

                resMsg.Error = new Exception(error.Message);
            }
        }
    }
}
