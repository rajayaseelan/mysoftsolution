using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Status;
using MySoft.Logger;

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
        private string serverUrl;
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
            //实例化Socket服务
            ScsTcpEndPoint endPoint = null;
            if (string.Compare(config.Host, "any", true) == 0)
            {
                config.Host = IPAddress.Loopback.ToString();
                endPoint = new ScsTcpEndPoint(config.Port);
            }
            else
                endPoint = new ScsTcpEndPoint(config.Host, config.Port);

            this.serverUrl = endPoint.ToString();
            this.server = ScsServerFactory.CreateServer(endPoint);
            this.server.ClientConnected += new EventHandler<ServerClientEventArgs>(server_ClientConnected);
            this.server.ClientDisconnected += new EventHandler<ServerClientEventArgs>(server_ClientDisconnected);
            this.server.WireProtocolFactory = new CustomWireProtocolFactory(config.Compress, config.Encrypt);
            this.OnCalling += new EventHandler<CallEventArgs>(CastleService_OnCalling);

            //实例化调用者
            var callbackTypes = GetCallbackTypes();
            this.caller = new ServiceCaller(container, callbackTypes);

            //绑定事件
            MessageCenter.Instance.OnError += new ErrorLogEventHandler(Instance_OnError);
        }

        void CastleService_OnCalling(object sender, CallEventArgs e)
        {
            //响应错误信息
            MessageCenter.Instance.Notify(e);
        }

        /// <summary>
        /// 处理异常信息
        /// </summary>
        /// <param name="exception"></param>
        void Instance_OnError(Exception exception)
        {
            container_OnError(exception);
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
            Start(false);
        }

        /// <summary>
        /// 启用服务
        /// </summary>
        /// <param name="isWriteLog"></param>
        public void Start(bool isWriteLog)
        {
            //写发布服务信息
            if (isWriteLog) Publish();

            //启动服务
            server.Start();
        }

        /// <summary>
        /// 发布服务
        /// </summary>
        private void Publish()
        {
            var list = this.GetServiceList();

            string log = string.Format("此次发布的服务有{0}个，共有{1}个方法，详细信息如下：\r\n\r\n", list.Count, list.Sum(p => CoreHelper.GetMethodsFromType(p).Count()));
            StringBuilder sb = new StringBuilder(log);

            int index = 0;
            foreach (var type in list)
            {
                sb.AppendFormat("{0}, {1}\r\n", type.Name, type.Assembly.FullName);
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
                foreach (var method in CoreHelper.GetMethodsFromType(type))
                {
                    sb.AppendLine(method.ToString());
                }

                if (index < list.Count - 1)
                {
                    sb.AppendLine();
                    sb.AppendLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                    sb.AppendLine();
                }

                index++;
            }

            SimpleLog.Instance.WriteLogForDir("Publish", sb.ToString());
        }

        /// <summary>
        /// 获取服务的ServerUrl地址
        /// </summary>
        public string ServerUrl
        {
            get { return serverUrl.ToLower(); }
        }

        /// <summary>
        /// 停止服务
        /// </summary>1
        public void Stop()
        {
            server.Stop();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public override void Dispose()
        {
            server.Stop();
            server.Clients.ClearAll();

            server = null;
            statuslist = null;
            base.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region 侦听事件

        void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container_OnLog(string.Format("User connection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Information);
            e.Client.MessageReceived += new EventHandler<MessageEventArgs>(Client_MessageReceived);
            e.Client.MessageSent += new EventHandler<MessageEventArgs>(Client_MessageSent);

            //处理登入事件
            var point = new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort);
            MessageCenter.Instance.Notify(point, true);
        }

        void Client_MessageSent(object sender, MessageEventArgs e)
        {
            //暂不作处理
        }

        void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container_OnLog(string.Format("User Disconnection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Error);

            //处理登出事件
            var point = new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort);
            MessageCenter.Instance.Notify(point, false);
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
                    client.State = (e.Message as ScsClientMessage).Client;

                    //响应客户端详细信息
                    var endPoint = (info.RemoteEndPoint as ScsTcpEndPoint);
                    var point = new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort);
                    MessageCenter.Instance.Notify(point, (e.Message as ScsClientMessage).Client);
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
                }
                catch (Exception ex)
                {
                    container_OnError(ex);

                    var resMsg = new ResponseMessage
                    {
                        TransactionId = new Guid(data.RepliedMessageId),
                        Expiration = DateTime.Now.AddMinutes(1),
                        ReturnType = ex.GetType(),
                        Error = ex
                    };

                    //发送数据到服务端
                    client.SendMessage(new ScsResultMessage(resMsg, e.Message.RepliedMessageId));

                    //调用参数信息
                    args = new CallEventArgs();
                    args.Caller.AppName = reqMsg.AppName;
                    args.Caller.IPAddress = reqMsg.IPAddress;
                    args.Caller.HostName = reqMsg.HostName;
                    args.Caller.AssemblyName = typeof(CastleService).Assembly.FullName;
                    args.Caller.ServiceName = reqMsg.ServiceName;
                    args.Caller.SubServiceName = reqMsg.SubServiceName;
                    args.Caller.Parameters = reqMsg.Parameters.ToString();
                    args.InvokeTime = DateTime.Now;
                    args.Error = ex;
                }

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

                //调用请求方法
                var resMsg = caller.CallMethod(client, reqMsg);

                //发送数据到服务端
                client.SendMessage(new ScsResultMessage(resMsg, reqMsg.TransactionId.ToString()));
            }
            else
            {
                args = new CallEventArgs();
                args.Caller.AppName = reqMsg.AppName;
                args.Caller.IPAddress = reqMsg.IPAddress;
                args.Caller.HostName = reqMsg.HostName;
                args.InvokeTime = DateTime.Now;

                //开始计时
                Stopwatch watch = Stopwatch.StartNew();

                //获取或创建一个对象
                TimeStatus status = statuslist.GetOrCreate(DateTime.Now);

                //调用请求方法
                var resMsg = caller.CallMethod(client, reqMsg);

                watch.Stop();

                //处理时间
                status.ElapsedTime += watch.ElapsedMilliseconds;

                //错误及成功计数
                if (resMsg.Error == null)
                {
                    status.SuccessCount++;

                    args.Count = resMsg.Count;
                    args.Value = resMsg.Data;
                    args.ElapsedTime = watch.ElapsedMilliseconds;
                }
                else
                {
                    status.ErrorCount++;

                    args.Error = resMsg.Error;
                }

                //实例化Message对象来进行发送
                var sendMessage = new ScsResultMessage(resMsg, reqMsg.TransactionId.ToString());

                //发送消息到客户端
                client.SendMessage(sendMessage);

                //计算流量
                status.DataFlow += sendMessage.DataLength;

                //服务参数信息
                args.Caller.AssemblyName = resMsg.AssemblyName;
                args.Caller.ServiceName = resMsg.ServiceName;
                args.Caller.SubServiceName = resMsg.SubServiceName;

                //计算流量
                args.Length = sendMessage.DataLength;
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
        /// 获取连接客户信息
        /// </summary>
        /// <returns></returns>
        public override IList<ClientInfo> GetClientInfoList()
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
                     })
                     .Select(p => new
                     {
                         AppName = p.Key.AppName,
                         IPAddress = p.Key.IPAddress,
                         HostName = p.Key.HostName,
                         Count = p.Count()
                     })
                     .GroupBy(p => p.AppName)
                     .Select(p => new ClientInfo
                     {
                         AppName = p.Key,
                         Connections = p.Select(g => new ConnectionInfo
                         {
                             IPAddress = g.IPAddress,
                             HostName = g.HostName,
                             Count = g.Count
                         }).ToList()
                     })
                     .ToList();

                if (items.Any(p => p.State == null))
                {
                    var ls = items.Where(p => p.State == null)
                        .Select(p => p.RemoteEndPoint).Cast<ScsTcpEndPoint>()
                        .GroupBy(p => p.IpAddress)
                        .Select(g => new ConnectionInfo
                        {
                            IPAddress = g.Key,
                            HostName = "Unknown",
                            Count = g.Count()
                        }).ToList();

                    list.Add(new ClientInfo { AppName = "Unknown", Connections = ls });
                }

                return list;
            }
            catch (Exception ex)
            {
                container_OnError(ex);

                return new List<ClientInfo>();
            }
        }

        #endregion
    }
}
