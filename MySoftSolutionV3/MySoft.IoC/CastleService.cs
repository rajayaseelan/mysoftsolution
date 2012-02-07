using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Net.Sockets;

namespace MySoft.IoC
{
    /// <summary>
    /// Castle服务
    /// </summary>
    public class CastleService : ServerMoniter
    {
        /// <summary>
        /// 调用方法委托
        /// </summary>
        public event EventHandler<CallEventArgs> OnCalling;

        private IScsServer server;
        //实例化Socket服务
        private ScsTcpEndPoint epServer;
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
            : base(config)
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
            this.OnCalling += CastleService_OnCalling;

            //实例化调用者
            var callbackTypes = GetCallbackTypes();
            this.caller = new ServiceCaller(container, callbackTypes);

            //绑定事件
            MessageCenter.Instance.OnError += Instance_OnError;
        }

        void CastleService_OnCalling(object sender, CallEventArgs e)
        {
            //响应错误信息
            MessageCenter.Instance.Notify(e);
        }

        /// <summary>
        /// 处理异常信息
        /// </summary>
        /// <param name="error"></param>
        void Instance_OnError(Exception error)
        {
            container.WriteError(error);
        }

        private IDictionary<string, Type> GetCallbackTypes()
        {
            var callbackTypes = new Dictionary<string, Type>();
            callbackTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);

            var types = container.GetInterfaces<ServiceContractAttribute>();
            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }
            }

            return callbackTypes;
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
            get { return container.GetInterfaces<ServiceContractAttribute>().Count(); }
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
        public override void Dispose()
        {
            caller.Dispose();

            server.Stop();
            server.Clients.ClearAll();
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
                var data = e.Message as ScsResultMessage;
                var reqMsg = data.MessageValue as RequestMessage;

                //调用事件
                CallEventArgs args = null;

                try
                {
                    //发送响应信息
                    GetSendResponse(client, reqMsg, out args);

                    //如果调用句柄不为空，则调用
                    if (args != null && OnCalling != null)
                    {
                        try
                        {
                            OnCalling(client, args);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    container.WriteError(ex);

                    var resMsg = new ResponseMessage
                    {
                        TransactionId = new Guid(data.RepliedMessageId),
                        ReturnType = ex.GetType(),
                        Error = ex
                    };

                    //发送数据到服务端
                    SendMessage(client, new ScsResultMessage(resMsg, data.RepliedMessageId));
                }
            }
        }

        /// <summary>
        /// 获取响应信息并发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        private void GetSendResponse(IScsServerClient client, RequestMessage reqMsg, out CallEventArgs args)
        {
            //如果是状态请求，则直接返回数据
            if (!IsServiceCounter(reqMsg))
            {
                args = null;
                long elapsedMilliseconds;

                //调用请求方法
                var resMsg = caller.CallMethod(client, reqMsg, null, out elapsedMilliseconds);

                //发送数据到服务端
                SendMessage(client, new ScsResultMessage(resMsg, resMsg.TransactionId.ToString()));
            }
            else
            {
                //服务参数信息
                args = new CallEventArgs();
                args.CallTime = DateTime.Now;
                args.Caller.AppName = reqMsg.AppName;
                args.Caller.IPAddress = reqMsg.IPAddress;
                args.Caller.HostName = reqMsg.HostName;
                args.Caller.ServiceName = reqMsg.ServiceName;
                args.Caller.MethodName = reqMsg.MethodName;
                args.Caller.Parameters = reqMsg.Parameters.ToString();

                //获取或创建一个对象
                TimeStatus status = statuslist.GetOrCreate(DateTime.Now);

                //调用请求方法
                long elapsedMilliseconds;
                var resMsg = caller.CallMethod(client, reqMsg, args.Caller, out elapsedMilliseconds);

                //处理时间
                status.ElapsedTime += elapsedMilliseconds;

                //错误及成功计数
                if (resMsg.IsError)
                {
                    status.ErrorCount++;

                    args.Error = resMsg.Error;

                    //捕获全局错误
                    if (reqMsg.InvokeMethod)
                    {
                        resMsg.Parameters.Clear();
                        resMsg.Error = new WarningException(args.Error.Message);
                    }
                }
                else
                {
                    status.SuccessCount++;

                    args.Count = resMsg.Count;
                    args.Value = resMsg.Value;
                    args.ElapsedTime = elapsedMilliseconds;
                }

                //实例化Message对象来进行发送
                var sendMessage = new ScsResultMessage(resMsg, resMsg.TransactionId.ToString());

                //发送消息到客户端
                SendMessage(client, sendMessage);

                //计算流量
                status.DataFlow += sendMessage.DataLength;
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
                string a = ex.ToString();
                string b = a;
            }
            catch (Exception ex)
            {
                //发送失败
                container.WriteError(ex);
            }
        }

        /// <summary>
        /// 判断是否需要计数
        /// </summary>
        /// <param name="request"></param>
        private bool IsServiceCounter(RequestMessage request)
        {
            if (request == null) return false;
            if (request.ServiceName == typeof(IStatusService).FullName) return false;

            return true;
        }

        /// <summary>
        /// 获取所有的客户端信息
        /// </summary>
        /// <returns></returns>
        public override IList<AppClient> GetAppClients()
        {
            try
            {
                var items = server.Clients.GetAllItems();

                //统计客户端数量
                return items.Where(p => p.State != null)
                      .Select(p => p.State as AppClient)
                      .Distinct(new AppClientComparer())
                      .ToList();
            }
            catch
            {
                return new List<AppClient>();
            }
        }

        /// <summary>
        /// 获取连接客户信息
        /// </summary>
        /// <returns></returns>
        public override IList<ClientInfo> GetClientList()
        {
            try
            {
                var items = server.Clients.GetAllItems();

                //统计客户端数量
                var list = items.Where(p => p.State != null)
                     .Select(p => p.State as AppClient)
                     .GroupBy(p => new
                     {
                         AppName = p.AppName,
                         IPAddress = p.IPAddress,
                         HostName = p.HostName
                     }).Select(p => new ClientInfo
                     {
                         AppName = p.Key.AppName,
                         IPAddress = p.Key.IPAddress,
                         HostName = p.Key.HostName,
                         ServerIPAddress = epServer.IpAddress,
                         ServerPort = epServer.TcpPort,
                         Count = p.Count()
                     }).ToList();

                if (items.Any(p => p.State == null))
                {
                    var ls = items.Where(p => p.State == null)
                        .Select(p => p.RemoteEndPoint).Cast<ScsTcpEndPoint>()
                        .GroupBy(p => p.IpAddress)
                        .Select(g => new ClientInfo
                        {
                            AppName = "Unknown",
                            IPAddress = g.Key,
                            ServerIPAddress = epServer.IpAddress,
                            ServerPort = epServer.TcpPort,
                            HostName = "Unknown",
                            Count = g.Count()
                        }).ToList();

                    list.AddRange(ls);
                }

                return list;
            }
            catch (Exception ex)
            {
                container.WriteError(ex);
                return new List<ClientInfo>();
            }
        }

        #endregion
    }
}
