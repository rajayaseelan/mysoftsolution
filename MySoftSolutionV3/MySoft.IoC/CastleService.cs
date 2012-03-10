using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.IoC.Services;
using MySoft.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : ILogable, IErrorLogable, IDisposable
    {
        private IServiceContainer container;
        private IScsServer server;
        private ScsTcpEndPoint epServer = null;
        private ServiceCaller caller;

        /// <summary>
        /// 服务容器
        /// </summary>
        public IServiceContainer Container
        {
            get { return container; }
        }

        /// <summary>
        /// 实例化CastleService
        /// </summary>
        /// <param name="config"></param>
        public CastleService(CastleServiceConfiguration config)
        {
            if (string.Compare(config.Host, "any", true) == 0)
            {
                config.Host = IPAddress.Loopback.ToString();
                epServer = new ScsTcpEndPoint(config.Port);
            }
            else
                epServer = new ScsTcpEndPoint(config.Host, config.Port);

            this.server = ScsServerFactory.CreateServer(epServer);
            this.server.ClientConnected += server_ClientConnected;
            this.server.ClientDisconnected += server_ClientDisconnected;
            this.server.WireProtocolFactory = new CustomWireProtocolFactory(config.Compress, config.Encrypt);

            //加载cacheType
            IServiceCache cache = null;
            if (!string.IsNullOrEmpty(config.CacheType))
            {
                try
                {
                    Type type = Type.GetType(config.CacheType);
                    object instance = Activator.CreateInstance(type);
                    cache = instance as IServiceCache;
                }
                catch (Exception ex)
                {
                    Instance_OnError(ex);
                }
            }

            //服务端注入内存处理
            this.container = new SimpleServiceContainer(CastleFactoryType.Local, new ServiceCache(cache));
            this.container.OnError += error => { if (OnError != null) OnError(error); };
            this.container.OnLog += (log, type) => { if (OnLog != null) OnLog(log, type); };

            //实例化调用者
            var service = new ServerStatusService(config, server, container);
            this.caller = new ServiceCaller(service, container, config.Timeout);

            //绑定事件
            MessageCenter.Instance.OnError += Instance_OnError;
        }

        /// <summary>
        /// 处理异常信息
        /// </summary>
        /// <param name="error"></param>
        void Instance_OnError(Exception error)
        {
            container.WriteError(error);
        }

        #region 启动停止服务

        /// <summary>
        /// 启用服务
        /// </summary>
        public void Start()
        {
            //启动服务
            server.Start();
        }

        /// <summary>
        /// 获取服务的ServerUrl地址
        /// </summary>
        public string ServerUrl
        {
            get { return epServer.ToString().ToLower(); }
        }

        /// <summary>
        /// 服务数
        /// </summary>
        public int ServiceCount
        {
            get
            {
                //获取拥有ServiceContract约束的服务
                var types = container.GetServiceTypes<ServiceContractAttribute>();
                return types.Where(type => type != typeof(IStatusService)).Count();
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>1
        public void Stop()
        {
            this.Dispose();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            server.Stop();
            server.Clients.ClearAll();
            caller.Dispose();
            container.Dispose();
        }

        #endregion

        #region 侦听事件

        void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container.WriteLog(string.Format("User connection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Information);
            e.Client.MessageReceived += Client_MessageReceived;
            e.Client.MessageSent += Client_MessageSent;
            e.Client.ErrorReceived += Client_ErrorReceived;

            //处理登入事件
            var connect = new ConnectInfo
            {
                ConnectTime = DateTime.Now,
                IPAddress = endPoint.IpAddress,
                Port = endPoint.TcpPort,
                ServerIPAddress = epServer.IpAddress ?? DnsHelper.GetIPAddress(),
                ServerPort = epServer.TcpPort,
                Connected = true
            };

            MessageCenter.Instance.Notify(connect);
        }

        void Client_ErrorReceived(object sender, ErrorEventArgs e)
        {
            container.WriteError(e.Error);
        }

        void Client_MessageSent(object sender, MessageEventArgs e)
        {
            //暂不作处理
        }

        void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container.WriteLog(string.Format("User Disconnection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Error);

            //处理登出事件
            var connect = new ConnectInfo
            {
                ConnectTime = DateTime.Now,
                IPAddress = endPoint.IpAddress,
                Port = endPoint.TcpPort,
                ServerIPAddress = epServer.IpAddress ?? DnsHelper.GetIPAddress(),
                ServerPort = epServer.TcpPort,
                Connected = false
            };

            MessageCenter.Instance.Notify(connect);
        }

        void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            //不是指定消息不处理
            if (e.Message is ScsClientMessage)
            {
                var info = sender as IScsServerClient;
                if (server.Clients.ContainsKey(info.ClientId))
                {
                    var client = server.Clients[info.ClientId];
                    var appClient = (e.Message as ScsClientMessage).Client;
                    client.State = appClient;

                    //响应客户端详细信息
                    var endPoint = (info.RemoteEndPoint as ScsTcpEndPoint);
                    container.WriteLog(string.Format("Change app 【{4}】 client {0}:{1} to {2}[{3}]！",
                        endPoint.IpAddress, endPoint.TcpPort, appClient.IPAddress, appClient.HostName, appClient.AppName), LogType.Information);

                    MessageCenter.Instance.Notify(endPoint.IpAddress, endPoint.TcpPort, appClient);
                }
            }
            else if (e.Message is ScsResultMessage)
            {
                //获取client发送端
                var client = sender as IScsServerClient;
                var message = e.Message as ScsResultMessage;
                var reqMsg = message.MessageValue as RequestMessage;

                //调用方法
                var resMsg = caller.CallMethod(client, reqMsg);

                //发送数据到服务端
                SendMessage(client, new ScsResultMessage(resMsg, message.RepliedMessageId));
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        private void SendMessage(IScsServerClient client, IScsMessage message)
        {
            try
            {
                client.SendMessage(message);
            }
            catch (SocketException ex)
            {
                //Socket异常，不处理
            }
            catch (Exception ex)
            {
                //发送失败
                container.WriteError(ex);
            }
        }

        #endregion

        #region ILogable Members

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;

        /// <summary>
        /// OnError event.
        /// </summary>
        public event ErrorLogEventHandler OnError;

        #endregion
    }
}
