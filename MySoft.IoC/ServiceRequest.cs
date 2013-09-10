using MySoft.IoC.Communication.Scs.Client;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务请求类
    /// </summary>
    internal class ServiceRequest
    {
        private IServiceCallback callback;
        private IScsClient client;
        private ServerNode node;
        private IList<string> messageIds;
        private bool subscribed;

        /// <summary>
        /// 实例化ServiceMessage
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="node"></param>
        /// <param name="subscribed"></param>
        public ServiceRequest(IServiceCallback callback, ServerNode node, bool subscribed)
        {
            this.callback = callback;
            this.node = node;
            this.subscribed = subscribed;
            this.messageIds = new List<string>();

            this.client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(node.IP, node.Port));
            this.client.KeepAlive = subscribed;
            this.client.WireProtocol = new CustomWireProtocol(node.Compress);

            this.client.Connected += client_Connected;
            this.client.Disconnected += client_Disconnected;
            this.client.MessageSent += client_MessageSent;
            this.client.MessageReceived += client_MessageReceived;
            this.client.MessageError += client_MessageError;
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public void Send(string messageId, RequestMessage reqMsg)
        {
            this.messageIds.Add(messageId);

            //如果连接断开，直接抛出异常
            if (client.CommunicationState != CommunicationStates.Connected)
            {
                ConnectServer();
            }

            //设置压缩与加密
            IScsMessage message = new ScsResultMessage(reqMsg, messageId);

            //发送消息
            client.SendMessage(message);
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_Connected(object sender, EventArgs e)
        {
            var error = new SocketException((int)SocketError.Success);

            callback.Connected(this, new ConnectEventArgs(this.client)
            {
                Error = error,
                Subscribed = subscribed
            });
        }

        /// <summary>
        /// 断开成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_Disconnected(object sender, EventArgs e)
        {
            var error = new SocketException((int)SocketError.ConnectionReset);

            callback.Disconnected(this, new ConnectEventArgs(this.client)
            {
                Error = error,
                Subscribed = subscribed
            });

            //实例化异常
            var message = string.Format("Connect to server ({0}:{1}) error！Server node : {2} -> ({3}){4}"
                                    , node.IP, node.Port, node.Key, error.ErrorCode, error.SocketErrorCode);
            var ex = new WarningException((int)SocketError.ConnectionReset, message, error);

            //断开时响应错误信息
            client_MessageError(sender, new ErrorEventArgs(ex));
        }

        private void client_MessageSent(object sender, MessageEventArgs e)
        {
            //TODO
        }

        private void client_MessageReceived(object sender, MessageEventArgs e)
        {
            //移除消息Id
            messageIds.Remove(e.Message.RepliedMessageId);

            if (e.Message is ScsCallbackMessage)
            {
                //消息类型转换
                var data = e.Message as ScsCallbackMessage;
                var value = new CallbackMessageEventArgs
                {
                    MessageId = e.Message.RepliedMessageId,
                    Message = data.MessageValue
                };

                //回调消息
                callback.MessageCallback(this, value);
            }
            else if (e.Message is ScsResultMessage)
            {
                //消息类型转换
                var data = e.Message as ScsResultMessage;
                var value = new ResponseMessageEventArgs
                {
                    MessageId = e.Message.RepliedMessageId,
                    Message = data.MessageValue as ResponseMessage
                };

                //把数据发送到客户端
                callback.MessageCallback(this, value);
            }
        }

        /// <summary>
        /// 错误回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_MessageError(object sender, ErrorEventArgs e)
        {
            foreach (var messageId in new List<string>(messageIds))
            {
                //移除消息Id
                messageIds.Remove(messageId);

                var value = new ResponseMessageEventArgs
                {
                    MessageId = messageId,
                    Error = e.Error
                };

                //把数据发送到客户端
                callback.MessageCallback(this, value);
            }
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private void ConnectServer()
        {
            try
            {
                //连接到服务器
                client.Connect();
            }
            catch (Exception e)
            {
                throw new WarningException((int)SocketError.NotConnected, string.Format("Can't connect to server ({0}:{1})！Server node : {2} -> {3}", node.IP, node.Port, node.Key, e.Message));
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.client.Connected -= client_Connected;
            this.client.Disconnected -= client_Disconnected;
            this.client.MessageSent -= client_MessageSent;
            this.client.MessageReceived -= client_MessageReceived;
            this.client.MessageError -= client_MessageError;
            this.client.Disconnect();
            this.client = null;
        }
    }
}
