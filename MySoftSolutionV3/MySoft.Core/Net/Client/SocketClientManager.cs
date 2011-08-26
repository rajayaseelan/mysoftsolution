using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MySoft.Net.Sockets;

namespace MySoft.Net.Client
{
    /// <summary>
    /// 默认Socket客户端
    /// </summary>
    public class SocketClientManager
    {
        /// <summary>
        /// 数据包接收
        /// </summary>
        public event ReceiveEventHandler OnReceived;

        /// <summary>
        /// 连上服务器
        /// </summary>
        public event ConnectionEventHandler OnConnected;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event DisconnectionEventHandler OnDisconnected;

        /// <summary>
        /// 数据包缓冲类
        /// </summary>
        public BufferList BuffListManger { get; set; }

        /// <summary>
        /// SOCKETCLIENT对象
        /// </summary>
        public SocketClient Client { get; set; }

        /// <summary>
        /// 实例化Socket客户端管理器
        /// </summary>
        public SocketClientManager()
        {
            //初始化数据包缓冲区,并设置了最大数据包尽可能的大 
            BuffListManger = new BufferList(1024 * 1024 * 256); //最大为1G数据

            Client = new SocketClient();
            Client.ReceiveTimeout = 5 * 60000;
            Client.SendTimeout = 5 * 60000;
            Client.OnReceived += new ReceiveEventHandler(Client_OnReceived);
            Client.OnConnected += new ConnectionEventHandler(Client_OnConnected);
            Client.OnDisconnected += new DisconnectionEventHandler(Client_OnDisconnected);
        }

        void Client_OnConnected(string message, bool connected, Socket socket)
        {
            if (OnConnected != null)
                OnConnected(message, connected, socket);
        }

        void Client_OnDisconnected(string message, Socket socket)
        {
            if (OnDisconnected != null)
                OnDisconnected(message, socket);
        }

        void Client_OnReceived(byte[] buffer, Socket socket)
        {
            List<byte[]> datax;

            //整理从服务器上收到的数据包
            try
            {
                if (BuffListManger.InsertByteArray(buffer, 4, out datax))
                {
                    if (OnReceived != null)
                    {
                        foreach (byte[] mdata in datax)
                        {
                            OnReceived(mdata, socket);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
