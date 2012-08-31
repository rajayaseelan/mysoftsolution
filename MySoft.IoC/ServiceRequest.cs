using System;
using System.Net.Sockets;
using MySoft.IoC.Communication;
using MySoft.IoC.Communication.Scs.Client;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务请求类
    /// </summary>
    public class ServiceRequest
    {
        /// <summary>
        /// 数据回调
        /// </summary>
        public event EventHandler<ServiceMessageEventArgs> OnCallback;

        /// <summary>
        /// 错误回调
        /// </summary>
        public event EventHandler<ErrorMessageEventArgs> OnError;

        private RequestMessage reqMsg;
        private IScsClient client;
        private IServiceContainer container;
        private ServerNode node;
        private bool isCallback;

        /// <summary>
        /// 实例化ServiceMessage
        /// </summary>
        /// <param name="node"></param>
        /// <param name="container"></param>
        public ServiceRequest(ServerNode node, IServiceContainer container, bool isCallback)
        {
            this.container = container;
            this.node = node;
            this.isCallback = isCallback;

            this.client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(node.IP, node.Port));
            this.client.IsTimeoutDisconnect = !isCallback;
            this.client.Connected += new EventHandler(client_Connected);
            this.client.Disconnected += client_Disconnected;
            this.client.MessageReceived += client_MessageReceived;
            this.client.MessageError += client_MessageError;
            this.client.WireProtocol = new CustomWireProtocol(node.Compress);
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_Connected(object sender, EventArgs e)
        {
            var error = new SocketException((int)SocketError.Success);

            container.SendConnected(sender, new ConnectEventArgs(this.client)
            {
                Error = error,
                IsCallback = isCallback
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

            container.SendDisconnected(sender, new ConnectEventArgs(this.client)
            {
                Error = error,
                IsCallback = isCallback
            });

            //断开时响应错误信息
            client_MessageError(sender, new ErrorEventArgs(error));
        }

        void client_MessageError(object sender, ErrorEventArgs e)
        {
            //输出错误信息
            if (OnError != null)
            {
                OnError(sender, new ErrorMessageEventArgs { Request = reqMsg, Error = e.Error });
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
                throw new WarningException(string.Format("Can't connect to server ({0}:{1})！Server node : {2} -> {3}", node.IP, node.Port, node.Key, e.Message));
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public void SendMessage(RequestMessage reqMsg)
        {
            this.reqMsg = reqMsg;

            //如果连接断开，直接抛出异常
            if (client.CommunicationState == CommunicationStates.Disconnected)
            {
                ConnectServer();

                //发送客户端信息到服务端
                var clientInfo = new AppClient
                {
                    AppPath = AppDomain.CurrentDomain.BaseDirectory,
                    AppName = reqMsg.AppName,
                    IPAddress = reqMsg.IPAddress,
                    HostName = reqMsg.HostName
                };

                //发送消息
                client.SendMessage(new ScsClientMessage(clientInfo));
            }

            //设置压缩与加密
            IScsMessage message = new ScsResultMessage(reqMsg, reqMsg.TransactionId.ToString());

            //发送消息
            client.SendMessage(message);
        }

        #region Socket消息委托

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = new ServiceMessageEventArgs
            {
                Client = client,
                Request = reqMsg
            };

            //不是指定消息不处理
            if (e.Message is ScsCallbackMessage)
            {
                //消息类型转换
                var data = e.Message as ScsCallbackMessage;
                message.Result = data.MessageValue;
            }
            else if (e.Message is ScsResultMessage)
            {
                //消息类型转换
                var data = e.Message as ScsResultMessage;
                message.Result = data.MessageValue;
            }

            //把数据发送到客户端
            if (OnCallback != null) OnCallback(this, message);
        }

        #endregion
    }
}
