using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        private IDictionary<string, Type> callbackTypes;
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
                config.Host = "127.0.0.1";
                endPoint = new ScsTcpEndPoint(config.Port);
            }
            else
                endPoint = new ScsTcpEndPoint(config.Host, config.Port);

            this.serverUrl = endPoint.ToString();
            this.server = ScsServerFactory.CreateServer(endPoint);
            this.server.ClientConnected += new EventHandler<ServerClientEventArgs>(server_ClientConnected);
            this.server.ClientDisconnected += new EventHandler<ServerClientEventArgs>(server_ClientDisconnected);
            this.server.WireProtocolFactory = new CustomWireProtocolFactory(config.Compress, config.Encrypt);

            InitCallbackTypes();
        }

        private void InitCallbackTypes()
        {
            callbackTypes = new Dictionary<string, Type>();
            var types = container.GetInterfaces<ServiceContractAttribute>();
            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }
            }
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

            SimpleLog.Instance.WriteLog(sb.ToString());
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
        }

        void Client_MessageSent(object sender, MessageEventArgs e)
        {
            //暂不作处理
        }

        void server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var endPoint = (e.Client.RemoteEndPoint as ScsTcpEndPoint);
            container_OnLog(string.Format("User Disconnection {0}:{1}！", endPoint.IpAddress, endPoint.TcpPort), LogType.Error);
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
                    args.Caller.ServiceName = reqMsg.ServiceName;
                    args.Caller.SubServiceName = reqMsg.SubServiceName;
                    args.CallTime = DateTime.Now;
                    args.ElapsedTime = -1;
                    args.CallError = ex;
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
            long elapsedTimeout;

            //如果是状态请求，则直接返回数据
            if (!IsServiceCounter(reqMsg))
            {
                args = null;

                //调用请求方法
                var resMsg = CallMethod(client, reqMsg, out elapsedTimeout);

                //发送数据到服务端
                client.SendMessage(new ScsResultMessage(resMsg, reqMsg.TransactionId.ToString()));
            }
            else
            {
                args = new CallEventArgs();
                args.Caller.AppName = reqMsg.AppName;
                args.Caller.IPAddress = reqMsg.IPAddress;
                args.Caller.HostName = reqMsg.HostName;
                args.Caller.ServiceName = reqMsg.ServiceName;
                args.Caller.SubServiceName = reqMsg.SubServiceName;
                args.CallTime = DateTime.Now;

                //获取或创建一个对象
                TimeStatus status = statuslist.GetOrCreate(DateTime.Now);

                //处理cacheKey信息
                string cacheKey = string.Format("{0}_{1}_{2}", reqMsg.ServiceName, reqMsg.SubServiceName, reqMsg.Parameters);

                var resMsg = CacheHelper.Get<ResponseMessage>(cacheKey);
                if (resMsg == null)
                {
                    //调用请求方法
                    resMsg = CallMethod(client, reqMsg, out elapsedTimeout);

                    //处理时间
                    status.ElapsedTime += elapsedTimeout;

                    //计算缓存时间
                    int times = (int)elapsedTimeout / 1000;

                    //数据不为null，并且记录数大于0
                    if (resMsg != null && resMsg.Data != null && resMsg.Count > 0)
                    {
                        if (times > 0)
                            CacheHelper.Insert(cacheKey, resMsg, times);
                        else
                            CacheHelper.Insert(cacheKey, resMsg, 1);
                    }

                    //参数信息
                    args.ElapsedTime = elapsedTimeout;

                    if (resMsg.Error == null)
                        args.ReturnValue = resMsg.Data;
                    else
                        args.CallError = resMsg.Error;

                    args.ValueCount = resMsg.Count;
                }
                else
                {
                    //从缓存读取的数据需要修改TransactionId
                    resMsg.TransactionId = reqMsg.TransactionId;
                    resMsg.Expiration = reqMsg.Expiration;
                }

                //错误及成功计数
                if (resMsg.Error == null)
                    status.SuccessCount++;
                else
                    status.ErrorCount++;

                //实例化Message对象来进行发送
                var sendMessage = new ScsResultMessage(resMsg, reqMsg.TransactionId.ToString());

                //发送消息到客户端
                client.SendMessage(sendMessage);

                //计算流量
                status.DataFlow += sendMessage.DataLength;
            }
        }

        /// <summary>
        /// 调用 方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage CallMethod(IScsServerClient client, RequestMessage reqMsg, out long elapsedTimeout)
        {
            //获取返回的消息
            ResponseMessage resMsg = null;

            try
            {
                Thread thread = null;

                //生成一个异步调用委托
                var caller = new AsyncMethodCaller<ResponseMessage, RequestMessage>(state =>
                {
                    thread = Thread.CurrentThread;

                    //实例化当前上下文
                    if (callbackTypes.ContainsKey(state.ServiceName))
                        OperationContext.Current = new OperationContext(client, callbackTypes[state.ServiceName]);
                    else
                        OperationContext.Current = new OperationContext(client);

                    return container.CallService(state, config.LogTime); ;
                });

                //开始调用
                IAsyncResult ar = caller.BeginInvoke(reqMsg, iar => { }, caller);

                //开始计时
                Stopwatch watch = Stopwatch.StartNew();

                //等待信号，等待5分钟
                bool timeout = !ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(reqMsg.Timeout));

                watch.Stop();
                elapsedTimeout = watch.ElapsedMilliseconds;

                if (timeout)
                {
                    try
                    {
                        if (!ar.IsCompleted && thread != null)
                            thread.Abort();
                    }
                    catch (Exception ex)
                    {
                    }

                    string title = string.Format("Call service ({0},{1}) timeout.", reqMsg.ServiceName, reqMsg.SubServiceName);
                    string body = string.Format("【{3}】Call service ({0},{1}) timeout ({2} ms)！", reqMsg.ServiceName, reqMsg.SubServiceName, watch.ElapsedMilliseconds, reqMsg.TransactionId);
                    throw new WarningException(body)
                    {
                        ApplicationName = reqMsg.AppName,
                        ServiceName = reqMsg.ServiceName,
                        ExceptionHeader = string.Format("Application【{0}】call service timeout. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                    };
                }

                //获取返回结果
                resMsg = caller.EndInvoke(ar);

                //关闭句柄
                ar.AsyncWaitHandle.Close();
            }
            catch (Exception ex)
            {
                //耗时时间
                elapsedTimeout = (long)TimeSpan.FromSeconds(reqMsg.Timeout).TotalMilliseconds;

                //抛出错误信息
                container_OnError(ex);

                resMsg = new ResponseMessage();
                resMsg.TransactionId = reqMsg.TransactionId;
                resMsg.ServiceName = reqMsg.ServiceName;
                resMsg.SubServiceName = reqMsg.SubServiceName;
                resMsg.Parameters = reqMsg.Parameters;
                resMsg.Expiration = reqMsg.Expiration;
                resMsg.ReturnType = reqMsg.ReturnType;
                resMsg.Error = ex;
            }

            return resMsg;
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
