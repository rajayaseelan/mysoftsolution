using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.HttpServer;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : ILogable, IErrorLogable, IDisposable
    {
        private CastleServiceConfiguration config;
        private IServiceContainer container;
        private HTTPServer httpServer;
        private IScsServer server;
        private ScsTcpEndPoint epServer;
        private ServerStatusService status;
        private ServiceCaller caller;
        private TaskPool pool1, pool2;

        /// <summary>
        /// Gets the service container.
        /// </summary>
        /// <value>The service container.</value>
        public IContainer Container { get { return container; } }

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
            this.container.OnError += Instance_OnError;
            this.container.OnLog += Instance_OnLog;

            //注册状态服务
            this.status = new ServerStatusService(server, config, container);
            container.Register(typeof(IStatusService), status);

            var processorCount = Environment.ProcessorCount;
            this.pool1 = new TaskPool(processorCount * 2 + 2, processorCount);
            this.pool2 = new TaskPool(config.MaxCaller, processorCount);

            //实例化调用者
            this.caller = new ServiceCaller(pool2, config, container);

            //判断是否启用httpServer
            if (config.HttpEnabled)
            {
                //设置默认的解析器
                IHttpApiResolver apiResolver = null;

                //判断是否配置了ApiResolverType
                apiResolver = Create<IHttpApiResolver>(config.ApiResolverType) ?? new DefaultApiResolver();

                var httpCaller = new HttpServiceCaller(config, container);

                //刷新服务委托
                status.OnRefresh += (sender, args) => httpCaller.InitCaller(apiResolver);

                //初始化调用器
                httpCaller.InitCaller(apiResolver);

                var handler = new HttpServiceHandler(httpCaller);
                var factory = new HttpRequestHandlerFactory(handler);
                this.httpServer = new HTTPServer(factory, config.HttpPort);
            }

            //绑定事件
            MessageCenter.Instance.OnLog += Instance_OnLog;
            MessageCenter.Instance.OnError += Instance_OnError;

            //发布日志
            PublishService(status.GetServiceList());
        }

        /// <summary>
        /// 正常日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="type"></param>
        private void Instance_OnLog(string log, LogType type)
        {
            if (OnLog != null) OnLog(log, type);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="exception"></param>
        private void Instance_OnError(Exception exception)
        {
            if (OnError != null) OnError(exception);
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

                httpServer.OnServerException += Instance_OnError;
                httpServer.Start();
            }

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
                var service = container.Resolve<IStatusService>();
                return service.GetServiceList().Count;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            try
            {
                if (config.HttpEnabled)
                {
                    httpServer.Stop();
                }

                server.Stop();
            }
            catch (Exception ex) { }
            finally
            {
                caller.Dispose();
                container.Dispose();
                pool1.Dispose();
                pool2.Dispose();
            }
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
            IScsServer server = sender as IScsServer;

            try
            {
                e.Channel.MessageSent += Channel_MessageSent;
                e.Channel.MessageReceived += Channel_MessageReceived;
                e.Channel.MessageError += Channel_MessageError;

                //输出信息
                var endPoint = (e.Channel.RemoteEndPoint as ScsTcpEndPoint);

                PushConnectInfo(server, endPoint, true);
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 消息发送成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Channel_MessageSent(object sender, MessageEventArgs e)
        {
            //TODO
        }

        /// <summary>
        /// 断开链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            IScsServer server = sender as IScsServer;

            try
            {
                e.Channel.MessageSent -= Channel_MessageSent;
                e.Channel.MessageReceived -= Channel_MessageReceived;
                e.Channel.MessageError -= Channel_MessageError;

                //输出信息
                var endPoint = (e.Channel.RemoteEndPoint as ScsTcpEndPoint);

                PushConnectInfo(server, endPoint, false);
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Channel_MessageReceived(object sender, MessageEventArgs e)
        {
            var channel = sender as IScsServerClient;

            try
            {
                //只处理指定消息
                if (e.Message is ScsResultMessage)
                {
                    var message = e.Message as ScsResultMessage;
                    var reqMsg = message.MessageValue as RequestMessage;

                    if (reqMsg == null) throw new NullReferenceException("The request object is empty or null.");

                    if (channel.UserToken == null)
                    {
                        var client = new AppClient
                        {
                            AppVersion = reqMsg.AppVersion,
                            AppName = reqMsg.AppName,
                            AppPath = reqMsg.AppPath,
                            IPAddress = reqMsg.IPAddress,
                            HostName = reqMsg.HostName
                        };

                        channel.UserToken = client;

                        //响应客户端详细信息
                        var endPoint = (channel.RemoteEndPoint as ScsTcpEndPoint);

                        PushAppClient(endPoint, client);
                    }

                    //调用服务
                    SendResponse(channel, message.RepliedMessageId, reqMsg);
                }
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        private void SendResponse(IScsServerClient channel, string messageId, RequestMessage reqMsg)
        {
            try
            {
                var appCaller = CreateCaller(reqMsg);

                //响应消息
                var resMsg = caller.HandleResponse(channel, appCaller, reqMsg);

                //数据计数
                DataCounter(messageId, appCaller, resMsg);

                var msgItem = new MessageItem
                {
                    MessageId = messageId,
                    Channel = channel,
                    Request = reqMsg,
                    Response = resMsg
                };

                //添加到发送队列
                pool1.AddTaskItem(WaitCallback, msgItem);
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 等等响应
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            if (state == null) return;

            try
            {
                var msgItem = state as MessageItem;

                //实例化上下文
                using (var client = new ServiceChannel(msgItem.Channel, msgItem.Request))
                {
                    //发送消息
                    client.SendResponse(msgItem.MessageId, msgItem.Response);
                }
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 数据计数
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appCaller"></param>
        /// <param name="resMsg"></param>
        private void DataCounter(string messageId, AppCaller appCaller, ResponseMessage resMsg)
        {
            if (resMsg == null) return;

            //调用参数
            var callArgs = new CallEventArgs(appCaller)
            {
                MessageId = messageId,
                ElapsedTime = resMsg.ElapsedTime,
                Count = resMsg.Count,
                Value = resMsg.Value,
                Error = resMsg.Error
            };

            //如果是Buffer数据
            if (resMsg is ResponseBuffer)
            {
                callArgs.Value = (resMsg as ResponseBuffer).Buffer;
            }

            //异步调用
            var func = new Action<CallEventArgs>(AsyncCounter);
            func.BeginInvoke(callArgs, null, null);
        }

        /// <summary>
        /// 获取AppCaller
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private AppCaller CreateCaller(RequestMessage reqMsg)
        {
            //服务参数信息
            var caller = new AppCaller
            {
                AppVersion = reqMsg.AppVersion,
                AppPath = reqMsg.AppPath,
                AppName = reqMsg.AppName,
                IPAddress = reqMsg.IPAddress,
                HostName = reqMsg.HostName,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName,
                Parameters = reqMsg.Parameters.ToString(),
                CallTime = DateTime.Now
            };

            return caller;
        }

        /// <summary>
        /// 同步调用方法
        /// </summary>
        /// <param name="callArgs"></param>
        private void AsyncCounter(CallEventArgs callArgs)
        {
            try
            {
                //响应消息
                MessageCenter.Instance.Notify(callArgs);

                //调用计数服务
                status.Counter(callArgs);

                if (Completed != null)
                {
                    Completed(this, callArgs);
                }
            }
            catch (Exception ex)
            {
                //写异常日志
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Channel_MessageError(object sender, ErrorEventArgs e)
        {
            container.WriteError(e.Error);
        }

        /// <summary>
        /// 推送链接信息
        /// </summary>
        /// <param name="server"></param>
        /// <param name="endPoint"></param>
        /// <param name="connected"></param>
        private void PushConnectInfo(IScsServer server, ScsTcpEndPoint endPoint, bool connected)
        {
            if (connected)
            {
                container.WriteLog(string.Format("[{2}] User connection ({0}:{1}).",
                                    endPoint.IpAddress, endPoint.TcpPort, server.Clients.Count), LogType.Information);
            }
            else
            {
                container.WriteLog(string.Format("[{2}] User Disconnection ({0}:{1}).",
                                    endPoint.IpAddress, endPoint.TcpPort, server.Clients.Count), LogType.Error);
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
        /// Completed event.
        /// </summary>
        public event EventHandler<CallEventArgs> Completed;

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
}