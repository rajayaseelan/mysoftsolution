using System;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Client;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务请求类
    /// </summary>
    public class ServiceRequest
    {
        private RequestMessage reqMsg;
        private IServiceCallback callback;
        private IScsClient client;
        private ServerNode node;
        private string messageId;
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
        /// 发送消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        public void SendMessage(string messageId, RequestMessage reqMsg)
        {
            this.messageId = messageId;
            this.reqMsg = reqMsg;

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
        void client_Connected(object sender, EventArgs e)
        {
            var error = new SocketException((int)SocketError.Success);

            callback.Connected(sender, new ConnectEventArgs(this.client)
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
        void client_Disconnected(object sender, EventArgs e)
        {
            var error = new SocketException((int)SocketError.ConnectionReset);

            callback.Disconnected(sender, new ConnectEventArgs(this.client)
            {
                Error = error,
                Subscribed = subscribed
            });

            //断开时响应错误信息
            client_MessageError(sender, new ErrorEventArgs(error));
        }

        void client_MessageSent(object sender, MessageEventArgs e)
        {
            //TODO
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message is ScsCallbackMessage)
                {
                    //消息类型转换
                    var data = e.Message as ScsCallbackMessage;
                    var value = new CallbackMessageEventArgs
                    {
                        MessageId = messageId,
                        Request = reqMsg,
                        Message = data.MessageValue
                    };

                    //回调消息
                    callback.MessageCallback(this, value);
                }
                else
                {
                    //获取响应消息
                    var value = new ResponseMessageEventArgs
                    {
                        MessageId = messageId,
                        Request = reqMsg,
                        Message = GetResponseMessage(e)
                    };

                    //把数据发送到客户端
                    callback.MessageCallback(this, value);
                }
            }
            catch (Exception ex)
            {
                //写异常日志
                client_MessageError(sender, new ErrorEventArgs(ex));
            }
        }

        /// <summary>
        /// 获取响应消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseMessage(MessageEventArgs e)
        {
            //定义消息
            ResponseMessage resMsg = null;

            if (e.Message is ScsResultMessage)
            {
                //消息类型转换
                var data = e.Message as ScsResultMessage;

                resMsg = data.MessageValue as ResponseMessage;
            }
            else if (e.Message is ScsRawDataMessage)
            {
                try
                {
                    //获取响应信息
                    var data = e.Message as ScsRawDataMessage;
                    var buffer = CompressionManager.DecompressGZip(data.MessageData);
                    resMsg = SerializationManager.DeserializeBin<ResponseMessage>(buffer);
                }
                catch (Exception ex)
                {
                    //出错时响应错误
                    resMsg = IoCHelper.GetResponse(reqMsg, ex);
                }
            }

            return resMsg;
        }

        /// <summary>
        /// 错误回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_MessageError(object sender, ErrorEventArgs e)
        {
            //输出错误信息
            var resMsg = IoCHelper.GetResponse(reqMsg, e.Error);

            var value = new ResponseMessageEventArgs
            {
                MessageId = messageId,
                Request = reqMsg,
                Message = resMsg
            };

            //把数据发送到客户端
            callback.MessageCallback(this, value);
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
    }
}
