using System;
using System.Linq;
using System.Net;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Callback;
using MySoft.IoC.Configuration;
using MySoft.IoC.HttpServer;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Net.Http;
using MySoft.Threading;
using System.Threading;
using MySoft.IoC.Communication;
using System.Collections;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : ILogable, IErrorLogable, IDisposable
    {
        private SmartThreadPool smart;
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

            //服务端注入内存处理
            this.container = new SimpleServiceContainer(CastleFactoryType.Local);
            this.container.OnError += error => { if (OnError != null) OnError(error); };
            this.container.OnLog += (log, type) => { if (OnLog != null) OnLog(log, type); };

            //实例化SmartThreadPool
            var stp = new STPStartInfo
            {
                IdleTimeout = config.Timeout * 1000,
                MaxWorkerThreads = Math.Max(config.MaxCalls, 10),
                MinWorkerThreads = 5,
                ThreadPriority = ThreadPriority.Normal,
                WorkItemPriority = WorkItemPriority.Normal
            };

            //创建线程池
            smart = new SmartThreadPool(stp);
            smart.Start();

            //创建并发任务组
            var group = smart.CreateWorkItemsGroup(Environment.ProcessorCount);
            group.Start();

            //实例化调用者
            var status = new ServerStatusService(server, config, container);
            this.caller = new ServiceCaller(group, status);

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

                var httpCaller = new HttpServiceCaller(group, config, container);

                //刷新服务委托
                status.OnRefresh += () => httpCaller.InitCaller(resolver);

                //初始化调用器
                httpCaller.InitCaller(resolver);

                var handler = new HttpServiceHandler(httpCaller);
                var factory = new HttpRequestHandlerFactory(handler);
                this.httpServer = new HTTPServer(factory, config.HttpPort);
            }

            //绑定事件
            MessageCenter.Instance.OnError += container.WriteError;
            MessageCenter.Instance.OnLog += container.WriteLog;
        }

        #region 启动停止服务

        /// <summary>
        /// 启用服务
        /// </summary>
        public void Start()
        {
            if (config.HttpEnabled)
            {
                httpServer.OnServerStart += () => { Console.WriteLine("[{0}] => Http server started. http://{1}:{2}/", DateTime.Now, DnsHelper.GetIPAddress(), config.HttpPort); };
                httpServer.OnServerStop += () => { Console.WriteLine("[{0}] => Http server stoped.", DateTime.Now); };

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
                //获取拥有ServiceContract约束的服务
                var types = container.GetServiceTypes<ServiceContractAttribute>();
                return types.Where(type => type != typeof(IStatusService)).Count();
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
            server.Clients.ClearAll();
            container.Dispose();

            //停止所有线程
            smart.Shutdown();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }

        #endregion

        #region 侦听事件

        void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;
            e.Client.MessageSent += Client_MessageSent;
            e.Client.MessageError += Client_MessageError;

            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);

            //推送ConnectInfo
            ThreadPool.QueueUserWorkItem(PushConnectInfo, new ArrayList { endPoint, true, server.Clients.Count });
        }

        void Client_MessageError(object sender, ErrorEventArgs e)
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

            //推送ConnectInfo
            ThreadPool.QueueUserWorkItem(PushConnectInfo, new ArrayList { endPoint, false, server.Clients.Count });
        }

        void PushConnectInfo(object state)
        {
            if (state == null) return;

            try
            {
                var arr = state as ArrayList;
                var endPoint = arr[0] as ScsTcpEndPoint;
                bool connected = Convert.ToBoolean(arr[1]);
                var count = Convert.ToInt32(arr[2]);

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
            catch
            {
                //TODO
            }
        }

        void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
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

                        //推送ConnectInfo
                        ThreadPool.QueueUserWorkItem(PushAppClient, new ArrayList { endPoint, appClient });
                    }
                }
                else if (e.Message is ScsResultMessage)
                {
                    //获取client发送端
                    var client = sender as IScsServerClient;

                    //解析消息
                    var message = e.Message as ScsResultMessage;
                    var reqMsg = message.MessageValue as RequestMessage;

                    //调用方法
                    var resMsg = caller.CallMethod(client, reqMsg);

                    //发送数据到服务端
                    SendMessage(client, resMsg, message.RepliedMessageId);
                }
            }
            catch (Exception ex)
            {
                container.WriteError(ex);
            }
        }

        void PushAppClient(object state)
        {
            if (state == null) return;

            try
            {
                var arr = state as ArrayList;
                var endPoint = arr[0] as ScsTcpEndPoint;
                var appClient = arr[1] as AppClient;

                container.WriteLog(string.Format("Change app 【{4}】 client {0}:{1} to {2}[{3}].",
                        endPoint.IpAddress, endPoint.TcpPort, appClient.IPAddress, appClient.HostName, appClient.AppName), LogType.Information);

                MessageCenter.Instance.Notify(endPoint.IpAddress, endPoint.TcpPort, appClient);
            }
            catch
            {
                //TODO
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msgBase"></param>
        /// <param name="messageId"></param>
        private void SendMessage(IScsServerClient client, MessageBase msgBase, string messageId)
        {
            try
            {
                var sendMsg = new ScsResultMessage(msgBase, messageId);

                //发送消息
                client.SendMessage(sendMsg);
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);

                try
                {
                    msgBase = new ResponseMessage
                    {
                        TransactionId = msgBase.TransactionId,
                        ReturnType = msgBase.ReturnType,
                        ServiceName = msgBase.ServiceName,
                        MethodName = msgBase.MethodName,
                        Parameters = msgBase.Parameters,
                        Error = ex
                    };

                    var sendMsg = new ScsResultMessage(msgBase, messageId);

                    //发送消息
                    client.SendMessage(sendMsg);
                }
                catch
                {
                    //写异常日志
                    container.WriteError(ex);
                }
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
