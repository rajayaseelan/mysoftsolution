using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.HttpServer;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Net.Http;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : ILogable, IErrorLogable
    {
        private CastleServiceConfiguration config;
        private IServiceContainer container;
        private HTTPServer httpServer;
        private IScsServer server;
        private ScsTcpEndPoint epServer;
        private ServiceCaller caller;

        /// <summary>
        /// 实例化CastleService
        /// </summary>
        /// <param name="config"></param>
        public CastleService(CastleServiceConfiguration config)
        {
            this.config = config;

            if (string.Compare(config.Host, "any", true) == 0)
                epServer = new ScsTcpEndPoint(config.Port);
            else if (string.Compare(config.Host, "localhost", true) == 0)
                epServer = new ScsTcpEndPoint(IPAddress.Loopback.ToString(), config.Port);
            else
                epServer = new ScsTcpEndPoint(config.Host, config.Port);

            this.server = ScsServerFactory.CreateServer(epServer);
            this.server.ClientConnected += server_ClientConnected;
            this.server.ClientDisconnected += server_ClientDisconnected;
            this.server.WireProtocolFactory = new CustomWireProtocolFactory(config.Compress);

            //服务端注入内存处理
            this.container = new SimpleServiceContainer(CastleFactoryType.Local);
            this.container.OnError += error =>
            {
                if (OnError != null) OnError(error);
                else SimpleLog.Instance.WriteLog(error);
            };
            this.container.OnLog += (log, type) =>
            {
                if (OnLog != null) OnLog(log, type);
            };

            //实例化调用者
            var status = new ServerStatusService(server, config, container);
            this.caller = new ServiceCaller(status, config, container);
            this.caller.Handler += (sender, args) =>
            {
                if (OnCalling != null) OnCalling(sender, args);
            };

            //判断是否启用httpServer
            if (config.HttpEnabled)
            {
                //设置默认的解析器
                IHttpApiResolver resolver = new DefaultApiResolver();

                //判断是否配置了HttpType
                if (config.HttpType != null && typeof(IHttpApiResolver).IsAssignableFrom(config.HttpType))
                {
                    resolver = Activator.CreateInstance(config.HttpType) as IHttpApiResolver;
                }

                var httpCaller = new HttpServiceCaller(config, container);

                //刷新服务委托
                status.OnRefresh += () => httpCaller.InitCaller(resolver);

                //初始化调用器
                httpCaller.InitCaller(resolver);

                var handler = new HttpServiceHandler(httpCaller);
                var factory = new HttpRequestHandlerFactory(handler);
                this.httpServer = new HTTPServer(factory, config.HttpPort);
            }

            //绑定事件
            MessageCenter.Instance.OnError += error =>
            {
                container.WriteError(error);
            };

            MessageCenter.Instance.OnLog += (log, type) =>
            {
                container.WriteLog(log, type);
            };

            //发布日志
            PublishService(status.GetServiceList());
        }

        void PublishService(IList<ServiceInfo> list)
        {
            string log = string.Format("此次发布的服务有{0}个，共有{1}个方法，详细信息如下：\r\n\r\n", list.Count, list.Sum(p => p.Methods.Count()));
            var sb = new StringBuilder(log);

            int index = 0;
            foreach (var info in list)
            {
                sb.AppendFormat("{0}, {1}\r\n", info.FullName, info.Assembly);
                sb.AppendLine("".PadRight(180, '-'));

                foreach (var method in info.Methods)
                {
                    sb.AppendLine(method.FullName);
                }

                if (index < list.Count - 1)
                {
                    sb.AppendLine();
                    sb.AppendLine("".PadRight(180, '<'));
                    sb.AppendLine();
                }

                index++;
            }

            //写日志
            SimpleLog.Instance.WriteLogForDir("ServiceRun", sb.ToString());
        }

        #region 启动停止服务

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if (config.HttpEnabled)
            {
                httpServer.OnServerStart += () =>
                {
                    container.WriteLog(string.Format("Http server host -> http://{0}:{1}/", IPAddress.Loopback, config.HttpPort), LogType.Normal);
                };
                httpServer.OnServerStop += () =>
                {
                    container.WriteLog("Http server stoped.", LogType.Normal);
                };

                httpServer.OnServerException += httpServer_OnServerException;
                httpServer.Start();
            }

            //启动服务
            server.Start();
        }

        /// <summary>
        /// HttpServer异常
        /// </summary>
        /// <param name="ex"></param>
        void httpServer_OnServerException(Exception ex)
        {
            if (OnError != null) OnError(ex);
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
                var service = container.Resolve<IStatusService>();
                return service.GetServiceList().Count;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (config.HttpEnabled)
            {
                httpServer.Stop();
            }

            server.Stop();

            container.Dispose();
        }

        #endregion

        #region 侦听事件

        void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            e.Channel.MessageReceived += Client_MessageReceived;
            e.Channel.MessageError += Client_MessageError;

            var endPoint = (e.Channel.RemoteEndPoint as ScsTcpEndPoint);
            PushConnectInfo(endPoint, true, e.ConnectCount);
        }

        void Client_MessageError(object sender, ErrorEventArgs e)
        {
            container.WriteError(e.Error);
        }

        void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            e.Channel.MessageReceived -= Client_MessageReceived;
            e.Channel.MessageError -= Client_MessageError;

            var endPoint = (e.Channel.RemoteEndPoint as ScsTcpEndPoint);
            PushConnectInfo(endPoint, false, e.ConnectCount);
        }

        void PushConnectInfo(ScsTcpEndPoint endPoint, bool connected, int count)
        {
            if (connected)
            {
                container.WriteLog(string.Format("[{2}] User connection ({0}:{1}).",
                                    endPoint.IpAddress, endPoint.TcpPort, count), LogType.Information);
            }
            else
            {
                container.WriteLog(string.Format("[{2}] User Disconnection ({0}:{1}).",
                                    endPoint.IpAddress, endPoint.TcpPort, count), LogType.Error);
            }

            //推送连接信息
            var connect = new ConnectInfo
            {
                ConnectTime = DateTime.Now,
                IPAddress = endPoint.IpAddress,
                Port = endPoint.TcpPort,
                ServerIPAddress = epServer.IpAddress ?? DnsHelper.GetIPAddress(),
                ServerPort = epServer.TcpPort,
                Connected = connected
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
                    var channel = server.Clients[info.ClientId];
                    var appClient = (e.Message as ScsClientMessage).Client;
                    channel.UserToken = appClient;

                    //响应客户端详细信息
                    var endPoint = (info.RemoteEndPoint as ScsTcpEndPoint);
                    PushAppClient(endPoint, appClient);
                }
            }
            else if (e.Message is ScsResultMessage)
            {
                //获取client发送端
                var channel = sender as IScsServerClient;
                var message = e.Message as ScsResultMessage;
                var reqMsg = message.MessageValue as RequestMessage;

                //调用方法
                caller.CallMethod(channel, reqMsg, message.RepliedMessageId);
            }
        }

        void PushAppClient(ScsTcpEndPoint endPoint, AppClient appClient)
        {
            container.WriteLog(string.Format("Change app 【{4}】 client {0}:{1} to {2}[{3}].",
                    endPoint.IpAddress, endPoint.TcpPort, appClient.IPAddress, appClient.HostName, appClient.AppName), LogType.Information);

            MessageCenter.Instance.Notify(endPoint.IpAddress, endPoint.TcpPort, appClient);
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

        /// <summary>
        /// OnCalling event.
        /// </summary>
        public event EventHandler<CallEventArgs> OnCalling;

        #endregion
    }
}
