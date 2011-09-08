using System;
using MySoft.Communication.Scs.Client;
using MySoft.Communication.Scs.Communication;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.IoC.Configuration;
using MySoft.Logger;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务请求类
    /// </summary>
    public class ServiceRequest : IDisposable
    {
        /// <summary>
        /// 数据回调
        /// </summary>
        public event EventHandler<ServiceMessageEventArgs> OnCallback;

        private IScsClient client;
        private ILog logger;
        private string node;
        private string ip;
        private int port;

        /// <summary>
        /// 实例化ServiceMessage
        /// </summary>
        /// <param name="node"></param>
        /// <param name="logger"></param>
        public ServiceRequest(RemoteNode node, ILog logger)
        {
            this.logger = logger;
            this.node = node.Key;
            this.ip = node.IP;
            this.port = node.Port;

            this.client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(ip, port));
            this.client.Connected += new EventHandler(client_Connected);
            this.client.Disconnected += new EventHandler(client_Disconnected);
            this.client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);
            this.client.MessageSent += new EventHandler<MessageEventArgs>(client_MessageSent);
            this.client.WireProtocol = new CustomWireProtocol(node.Compress, node.Encrypt);
            this.client.ConnectTimeout = 5000;
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return client.CommunicationState == CommunicationStates.Connected; }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public void SendMessage(RequestMessage reqMsg)
        {
            //如果连接断开，直接抛出异常
            if (!IsConnected)
            {
                try
                {
                    //连接到服务器
                    client.Connect();
                }
                catch (Exception e)
                {
                    throw new WarningException(string.Format("Can't connect to server ({0}:{1})！Remote node : {2} -> {3}", ip, port, node, e.Message));
                }
            }

            client.SendMessage(new ScsResultMessage(reqMsg, reqMsg.TransactionId.ToString()));
        }

        #region Socket消息委托

        void client_MessageSent(object sender, MessageEventArgs e)
        {
            //暂不作处理
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            //不是指定消息不处理
            if (e.Message is ScsResultMessage)
            {
                try
                {
                    //消息类型转换
                    var data = e.Message as ScsResultMessage;

                    //把数据发送到客户端
                    if (OnCallback != null) OnCallback(this, new ServiceMessageEventArgs { Client = client, Result = data.MessageValue as ResponseMessage });
                }
                catch (Exception ex)
                {
                    logger.WriteError(ex);

                    var resMsg = new ResponseMessage
                    {
                        TransactionId = new Guid(e.Message.RepliedMessageId),
                        Expiration = DateTime.Now.AddMinutes(1),
                        ReturnType = ex.GetType(),
                        Exception = ex
                    };

                    //把数据发送到客户端
                    if (OnCallback != null) OnCallback(this, new ServiceMessageEventArgs { Client = client, Result = resMsg });
                }
            }
        }

        void client_Disconnected(object sender, EventArgs e)
        {
            //暂不处理 
        }

        void client_Connected(object sender, EventArgs e)
        {
            //暂不处理 
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            client.Dispose();

            logger = null;
            client = null;
        }
    }
}
