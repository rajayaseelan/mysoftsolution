using System;
using System.Net.Sockets;
using MySoft.IoC.Configuration;
using MySoft.Logger;
using MySoft.Net.Sockets;

namespace MySoft.IoC.Message
{
    /// <summary>
    /// 服务消息
    /// </summary>
    public class ServiceMessage : IDisposable
    {
        public event ServiceMessageEventHandler SendCallback;

        private SocketClient client;
        private ILog logger;
        private bool isConnected = false;
        private string node;
        private string ip;
        private int port;

        public ServiceMessage(RemoteNode node, ILog logger, int bufferSize)
        {
            this.logger = logger;
            this.node = node.Key;
            this.ip = node.IP;
            this.port = node.Port;

            MessageEventHandler messageEvent = SocketClient_OnMessage;
            CloseEventHandler closeEvent = SocketClient_OnClose;
            ErrorEventHandler errorEvent = SocketClient_OnError;

            client = new SocketClient(bufferSize, null, messageEvent, closeEvent, errorEvent);
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return isConnected; }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public void Send(DataPacket packet)
        {
            //如果连接断开，直接抛出异常
            if (!isConnected)
            {
                try
                {
                    //连接到服务器
                    client.Connect(ip, port);
                    isConnected = client.Connected;
                }
                catch (SocketException e)
                {
                    throw new WarningException(string.Format("Can't connect to server ({0}:{1})！Remote node : {2} -> {3}:{4}", ip, port, node, e.ErrorCode, e.Message));
                }
            }

            if (packet == null || packet.PacketObject == null)
            {
                return;
            }

            client.Send(packet);
        }

        #region Socket消息委托

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="resMsg"></param>
        private void SendMessage(ResponseMessage resMsg)
        {
            if (SendCallback != null)
            {
                var args = new ServiceMessageEventArgs
                {
                    Result = resMsg,
                    Socket = client.ClientSocket
                };

                SendCallback(this, args);
            }
        }

        void SocketClient_OnMessage(SocketBase socket, int iNumberOfBytes)
        {
            using (BufferReader read = new BufferReader(socket.RawBuffer))
            {
                int length;
                int cmd;
                Guid pid;

                if (read.ReadInt32(out length) && read.ReadInt32(out cmd) && read.ReadGuid(out pid) && length == read.Length)
                {
                    if (cmd == 10000) //返回数据包
                    {
                        try
                        {
                            ResponseMessage resMsg;
                            if (read.ReadObject(out resMsg))
                            {
                                SendMessage(resMsg);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.WriteError(ex);

                            var resMsg = new ResponseMessage
                            {
                                TransactionId = pid,
                                Expiration = DateTime.Now.AddMinutes(1),
                                Compress = false,
                                Encrypt = false,
                                ReturnType = ex.GetType(),
                                Exception = ex
                            };

                            SendMessage(resMsg);
                        }
                    }
                }
            }
        }

        void SocketClient_OnClose(SocketBase socket)
        {
            client.Dispose();

            //断开服务器
            isConnected = false;
        }

        void SocketClient_OnError(SocketBase socket, Exception exception)
        {
            //错误处理
            logger.WriteError(exception);
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
            client = null;
        }
    }
}
