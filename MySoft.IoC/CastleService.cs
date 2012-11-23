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
using MySoft.IoC.Nodes;
using MySoft.IoC.Services;
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
        private ServerStatusService status;

        /// <summary>
        /// 处理服务
        /// </summary>
        public IScsServer Server { get { return server; } }

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
            this.status = new ServerStatusService(server, config, container);

            //注册状态服务
            container.Register(typeof(IStatusService), status);

            this.caller = new ServiceCaller(config, container);

            //判断是否启用httpServer
            if (config.HttpEnabled)
            {
                //设置默认的解析器
                IHttpApiResolver apiResolver = null;
                IServerNodeResolver nodeResolver = null;

                //判断是否配置了ApiResolverType
                apiResolver = Create<IHttpApiResolver>(config.ApiResolverType) ?? new DefaultApiResolver();

                //判断是否配置了NodeResolverType
                nodeResolver = Create<IServerNodeResolver>(config.NodeResolverType) ?? new DefaultNodeResolver();

                var httpCaller = new HttpServiceCaller(config, container);

                //刷新服务委托
                status.OnRefresh += (sender, args) => httpCaller.InitCaller(apiResolver);

                //获取服务节点
                status.OnServerNode += (sender, args) => nodeResolver.GetServerNodes(args.NodeKey, args.ServiceName);

                //初始化调用器
                httpCaller.InitCaller(apiResolver);

                var handler = new HttpServiceHandler(httpCaller);
                var factory = new HttpRequestHandlerFactory(handler);
                this.httpServer = new HTTPServer(factory, config.HttpPort);
            }

            //绑定事件
            MessageCenter.Instance.OnLog += container.WriteLog;
            MessageCenter.Instance.OnError += container.WriteError;

            //发布日志
            PublishService(status.GetServiceList());
        }

        private void PublishService(IList<ServiceInfo> list)
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

        /// <summary>
        /// 创建指定类型的实例
        /// </summary>
        /// <typeparam name="InterfaceType"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        private InterfaceType Create<InterfaceType>(Type type)
            where InterfaceType : class
        {
            try
            {
                if (type != null && typeof(InterfaceType).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(type) as InterfaceType;
                }
            }
            catch
            {
            }

            return default(InterfaceType);
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

        /// <summary>
        /// 开始链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                e.Channel.MessageReceived += Client_MessageReceived;
                e.Channel.MessageError += Client_MessageError;

                var endPoint = (e.Channel.RemoteEndPoint as ScsTcpEndPoint);
                PushConnectInfo(endPoint, true, e.ConnectCount);
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 断开链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                e.Channel.MessageReceived -= Client_MessageReceived;
                e.Channel.MessageError -= Client_MessageError;

                var endPoint = (e.Channel.RemoteEndPoint as ScsTcpEndPoint);
                PushConnectInfo(endPoint, false, e.ConnectCount);
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
            finally
            {
                //结束线程
                ThreadManager.Cancel(e.Channel);
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var channel = sender as IScsServerClient;

            try
            {
                //只处理指定消息
                if (e.Message is ScsClientMessage)
                {
                    var client = (e.Message as ScsClientMessage).Client;
                    channel.UserToken = client;

                    //响应客户端详细信息
                    var endPoint = (channel.RemoteEndPoint as ScsTcpEndPoint);
                    PushAppClient(endPoint, client);
                }
                else if (e.Message is ScsResultMessage)
                {
                    var message = e.Message as ScsResultMessage;
                    var reqMsg = message.MessageValue as RequestMessage;

                    //实例化服务通道
                    using (var client = new ServiceChannel(channel, caller, status))
                    {
                        try
                        {
                            //发送消息
                            client.SendMessage(message.RepliedMessageId, reqMsg, NotifyResult);
                        }
                        catch (Exception ex)
                        {
                            //写异常日志
                            container.WriteError(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 响应结果
        /// </summary>
        /// <param name="callArgs"></param>
        private void NotifyResult(CallEventArgs callArgs)
        {
            try
            {
                //响应消息
                MessageCenter.Instance.Notify(callArgs);

                if (OnCalling != null)
                {
                    OnCalling(container, callArgs);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_MessageError(object sender, ErrorEventArgs e)
        {
            container.WriteError(e.Error);
        }

        /// <summary>
        /// 推送链接信息
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="connected"></param>
        /// <param name="count"></param>
        private void PushConnectInfo(ScsTcpEndPoint endPoint, bool connected, int count)
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
            var appConnect = new ConnectInfo
            {
                ConnectTime = DateTime.Now,
                IPAddress = endPoint.IpAddress,
                Port = endPoint.TcpPort,
                ServerIPAddress = epServer.IpAddress ?? DnsHelper.GetIPAddress(),
                ServerPort = epServer.TcpPort,
                Connected = connected
            };

            MessageCenter.Instance.Notify(appConnect);
        }

        /// <summary>
        /// 推送客户端信息
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="appClient"></param>
        private void PushAppClient(ScsTcpEndPoint endPoint, AppClient appClient)
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