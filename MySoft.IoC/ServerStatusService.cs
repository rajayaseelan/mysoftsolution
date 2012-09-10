using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Callback;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务端监控
    /// </summary>
    public class ServerStatusService : IStatusService
    {
        /// <summary>
        /// 刷新处理
        /// </summary>
        public event RefreshEventHandler OnRefresh;

        private const int DefaultDisconnectionAttemptTimeout = 15; //15 minutes.

        private CastleServiceConfiguration config;
        private IScsServer server;
        private IServiceContainer container;
        private TimeStatusCollection statuslist;
        private DateTime startTime;

        internal IServiceContainer Container { get { return container; } }
        internal CastleServiceConfiguration Config { get { return config; } }

        /// <summary>
        /// 实例化ServerStatusService
        /// </summary>
        /// <param name="server"></param>
        /// <param name="container"></param>
        /// <param name="config"></param>
        public ServerStatusService(IScsServer server, CastleServiceConfiguration config, IServiceContainer container)
        {
            this.config = config;
            this.server = server;
            this.container = container;
            this.startTime = DateTime.Now;
            this.statuslist = new TimeStatusCollection(config.RecordHours * 3600);

            //启动定义推送线程
            var thread1 = new Thread(DoPushWork);
            thread1.IsBackground = true;
            thread1.Start();

            //启动自动检测线程
            var thread2 = new Thread(DoCheckWork);
            thread2.IsBackground = true;
            thread2.Start();
        }

        void DoPushWork()
        {
            while (true)
            {
                //响应定时信息
                if (statuslist.Count > 0 && MessageCenter.Instance.Count > 0)
                {
                    ServerStatus status = null;

                    try
                    {
                        status = GetServerStatus();
                        MessageCenter.Instance.Notify(status);
                    }
                    catch (Exception ex)
                    {
                        //TODO
                    }
                    finally
                    {
                        status = null;
                    }

                    //每秒推送一次
                    Thread.Sleep(1000);
                }
                else
                {
                    //每秒推送一次
                    Thread.Sleep(10000);
                }
            }
        }

        void DoCheckWork()
        {
            while (true)
            {
                //每1分钟检测一次
                Thread.Sleep(TimeSpan.FromMinutes(1));

                try
                {
                    //处理客户端连接
                    var lastMinute = DateTime.Now.AddMinutes(-DefaultDisconnectionAttemptTimeout);

                    foreach (var client in server.Clients.GetAllItems())
                    {
                        if (client.LastReceivedMessageTime < lastMinute && client.LastSentMessageTime < lastMinute)
                        {
                            //如果超过15分钟没响应，则断开链接
                            client.Disconnect();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO
                    container.WriteError(ex);
                }
            }
        }

        /// <summary>
        /// 进行计数处理并响应
        /// </summary>
        /// <param name="args"></param>
        internal void Counter(CallEventArgs args)
        {
            //获取或创建一个对象
            var status = statuslist.GetOrCreate(args.Caller.CallTime);

            lock (status)
            {
                //处理时间
                status.ElapsedTime += args.ElapsedTime;

                //错误及成功计数
                if (args.IsError)
                    status.ErrorCount++;
                else
                    status.SuccessCount++;
            }
        }

        #region IStatusService 成员

        #region 获取客户端信息

        /// <summary>
        /// 获取所有的客户端信息
        /// </summary>
        /// <returns></returns>
        public IList<AppClient> GetAppClients()
        {
            try
            {
                var items = server.Clients.GetAllItems();

                //统计客户端数量
                var list = items.Where(p => p.ClientState != null)
                      .Select(p => p.ClientState as AppClient)
                      .Distinct(new AppClientComparer())
                      .ToList();

                return list;
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
        public IList<ClientInfo> GetClientList()
        {
            //客户端列表
            var clients = new List<ClientInfo>();

            try
            {
                var epServer = server.EndPoint as ScsTcpEndPoint;
                var items = server.Clients.GetAllItems();

                var list1 = items.Where(p => p.ClientState != null).ToList();
                var list2 = items.Where(p => p.ClientState == null).ToList();

                //如果list不为0
                if (list1.Count > 0)
                {
                    var ls = list1.Select(p => p.ClientState as AppClient)
                             .GroupBy(p => new
                             {
                                 AppName = p.AppName,
                                 IPAddress = p.IPAddress,
                                 AppPath = p.AppPath,
                             })
                             .Select(p => new ClientInfo
                             {
                                 AppPath = p.Key.AppPath,
                                 AppName = p.Key.AppName,
                                 IPAddress = p.Key.IPAddress,
                                 HostName = p.Max(c => c.HostName),
                                 ServerIPAddress = epServer.IpAddress ?? DnsHelper.GetIPAddress(),
                                 ServerPort = epServer.TcpPort,
                                 Count = p.Count()
                             }).ToList();

                    clients.AddRange(ls);
                }

                //如果list不为0
                if (list2.Count > 0)
                {
                    var ls = list2.Where(p => p.ClientState == null)
                            .Select(p => p.RemoteEndPoint).Cast<ScsTcpEndPoint>()
                            .GroupBy(p => p.IpAddress)
                            .Select(g => new ClientInfo
                            {
                                AppPath = "Unknown Path",
                                AppName = "Unknown",
                                IPAddress = g.Key,
                                ServerIPAddress = epServer.IpAddress ?? DnsHelper.GetIPAddress(),
                                ServerPort = epServer.TcpPort,
                                HostName = "Unknown",
                                Count = g.Count()
                            }).ToList();

                    clients.AddRange(ls);
                }
            }
            catch (Exception ex)
            {
                container.WriteError(ex);
            }

            return clients;
        }

        #endregion

        /// <summary>
        /// 是否存在服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool ContainsService(string serviceName)
        {
            return container.Contains<ServiceContractAttribute>(serviceName);
        }

        /// <summary>
        /// 刷新API服务
        /// </summary>
        /// <returns></returns>
        public void RefreshApi()
        {
            if (OnRefresh != null) OnRefresh();
        }

        #region GetServiceInfos

        private IList<ServiceInfo> services;
        /// <summary>
        /// 获取服务信息列表
        /// </summary>
        /// <returns></returns>
        public IList<ServiceInfo> GetServiceList()
        {
            if (services == null || services.Count == 0)
            {
                //获取拥有ServiceContract约束的服务
                var types = container.GetServiceTypes<ServiceContractAttribute>();

                services = new List<ServiceInfo>();
                foreach (var type in types)
                {
                    //状态服务跳过
                    if (type == typeof(IStatusService)) continue;

                    var contract1 = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                    var s = new ServiceInfo
                    {
                        Assembly = type.Assembly.FullName,
                        Name = type.Name,
                        FullName = type.FullName
                    };

                    if (contract1 != null)
                    {
                        s.ServiceName = contract1.Name;
                        s.ServiceDescription = contract1.Description;
                    }

                    //读取方法
                    foreach (var method in CoreHelper.GetMethodsFromType(type))
                    {
                        var contract2 = CoreHelper.GetMemberAttribute<OperationContractAttribute>(type);
                        var m = new MethodInfo
                        {
                            Name = method.Name,
                            FullName = method.ToString()
                        };

                        if (contract2 != null)
                        {
                            m.MethodName = contract2.Name;
                            m.MethodDescription = contract2.Description;
                        }

                        //读取参数
                        foreach (var p in method.GetParameters())
                        {
                            var pi = new ParameterInfo
                            {
                                Name = p.Name,
                                TypeName = CoreHelper.GetTypeName(p.ParameterType),
                                TypeFullName = p.ParameterType.FullName,
                                IsByRef = p.ParameterType.IsByRef,
                                IsOut = p.IsOut,
                                IsEnum = p.ParameterType.IsEnum,
                                IsPrimitive = CheckPrimitive(p.ParameterType),
                                SubParameters = GetSubParameters(p.ParameterType)
                            };

                            if (pi.IsEnum)
                            {
                                pi.EnumValue = GetEnumValue(p.ParameterType);
                            }

                            m.Parameters.Add(pi);
                        }

                        s.Methods.Add(m);
                    }

                    services.Add(s);
                }
            }

            return services;
        }

        private bool CheckPrimitive(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();
            return type.IsValueType || type == typeof(string);
        }

        private IList<ParameterInfo> GetSubParameters(Type type)
        {
            type = CoreHelper.GetPrimitiveType(type);
            if (GetTypeClass(type) && !typeof(ICollection).IsAssignableFrom(type))
            {
                var plist = new List<ParameterInfo>();
                foreach (var p in CoreHelper.GetPropertiesFromType(type))
                {
                    var pi = new ParameterInfo
                    {
                        Name = p.Name,
                        TypeName = CoreHelper.GetTypeName(p.PropertyType),
                        TypeFullName = p.PropertyType.FullName,
                        IsByRef = p.PropertyType.IsByRef,
                        IsOut = false,
                        IsEnum = p.PropertyType.IsEnum,
                        IsPrimitive = CheckPrimitive(p.PropertyType)
                    };

                    if (p.PropertyType != type)
                        pi.SubParameters = GetSubParameters(p.PropertyType);

                    if (pi.IsEnum)
                    {
                        pi.EnumValue = GetEnumValue(p.PropertyType);
                    }

                    plist.Add(pi);
                }

                return plist;
            }
            else
            {
                return new List<ParameterInfo>();
            }
        }

        private IList<EnumInfo> GetEnumValue(Type type)
        {
            var names = Enum.GetNames(type);
            var values = Enum.GetValues(type);

            IList<EnumInfo> list = new List<EnumInfo>();
            for (int i = 0; i < names.Length; i++)
            {
                list.Add(new EnumInfo
                {
                    Name = names[i],
                    Value = Convert.ToInt32(values.GetValue(i))
                });
            }

            return list;
        }

        private bool GetTypeClass(Type type)
        {
            if (type.IsGenericType)
                return GetTypeClass(type.GetGenericArguments()[0]);
            else
                return type.IsClass && type != typeof(string);
        }

        #endregion

        /// <summary>
        /// 服务状态信息
        /// </summary>
        /// <returns></returns>
        public ServerStatus GetServerStatus()
        {
            ServerStatus status = new ServerStatus
            {
                StartDate = startTime,
                TotalHours = config.RecordHours,
                Highest = GetHighestStatus(),
                Latest = GetLatestStatus(),
                Summary = GetSummaryStatus()
            };

            return status;
        }

        /// <summary>
        /// 清除所有服务器状态
        /// </summary>
        public void ClearServerStatus()
        {
            lock (statuslist)
            {
                //重置统计时间
                startTime = DateTime.Now;

                statuslist.Clear();
            }
        }

        /// <summary>
        /// 获取最后一次服务状态
        /// </summary>
        /// <returns></returns>
        private TimeStatus GetLatestStatus()
        {
            return statuslist.GetNewest();
        }

        /// <summary>
        /// 获取最高状态信息
        /// </summary>
        /// <returns></returns>
        private HighestStatus GetHighestStatus()
        {
            var highest = new HighestStatus();
            var list = statuslist.ToList();

            #region 处理最高值

            //处理最高值 
            if (list.Count > 0)
            {
                //成功
                highest.SuccessCount = list.Max(p => p.SuccessCount);
                if (highest.SuccessCount > 0)
                {
                    var data = list.FirstOrDefault(p => p.SuccessCount == highest.SuccessCount);
                    highest.SuccessCountCounterTime = data == null ? DateTime.MinValue : data.CounterTime;
                }

                //失败
                highest.ErrorCount = list.Max(p => p.ErrorCount);
                if (highest.ErrorCount > 0)
                {
                    var data = list.FirstOrDefault(p => p.ErrorCount == highest.ErrorCount);
                    highest.ErrorCountCounterTime = data == null ? DateTime.MinValue : data.CounterTime;
                }

                //请求总数
                highest.RequestCount = list.Max(p => p.RequestCount);
                if (highest.RequestCount > 0)
                {
                    var data = list.FirstOrDefault(p => p.RequestCount == highest.RequestCount);
                    highest.RequestCountCounterTime = data == null ? DateTime.MinValue : data.CounterTime;
                }

                //耗时
                highest.ElapsedTime = list.Max(p => p.ElapsedTime);
                if (highest.ElapsedTime > 0)
                {
                    var data = list.FirstOrDefault(p => p.ElapsedTime == highest.ElapsedTime);
                    highest.ElapsedTimeCounterTime = data == null ? DateTime.MinValue : data.CounterTime;
                }
            }

            #endregion

            return highest;
        }

        /// <summary>
        /// 汇总状态信息
        /// </summary>
        /// <returns></returns>
        private SummaryStatus GetSummaryStatus()
        {
            //获取状态列表
            var list = GetTimeStatusList();

            //统计状态信息
            return new SummaryStatus
            {
                RunningSeconds = list.Count,
                RequestCount = list.Sum(p => p.RequestCount),
                SuccessCount = list.Sum(p => p.SuccessCount),
                ErrorCount = list.Sum(p => p.ErrorCount),
                ElapsedTime = list.Sum(p => p.ElapsedTime)
            };
        }

        /// <summary>
        /// 获取服务状态列表
        /// </summary>
        /// <returns></returns>
        public IList<TimeStatus> GetTimeStatusList()
        {
            return statuslist.ToList();
        }

        /// <summary>
        /// 订阅服务
        /// </summary>
        public void Subscribe(params string[] subscribeTypes)
        {
            Subscribe(new SubscribeOptions(), subscribeTypes);
        }

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="options">订阅选项</param>
        public void Subscribe(SubscribeOptions options, params string[] subscribeTypes)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IStatusListener>();
            var client = OperationContext.Current.ServerClient;
            MessageCenter.Instance.AddListener(new MessageListener(client, callback, options, subscribeTypes));
        }

        /// <summary>
        /// 退订
        /// </summary>
        public void Unsubscribe()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IStatusListener>();
            var client = OperationContext.Current.ServerClient;
            MessageCenter.Instance.RemoveListener(new MessageListener(client, callback));
        }

        /// <summary>
        /// 获取订阅的类型
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSubscribeTypes()
        {
            var client = OperationContext.Current.ServerClient;
            var listener = MessageCenter.Instance.GetListener(client);
            if (listener == null)
                throw new WarningException("Please enable to subscribe.");

            return listener.Types;
        }

        /// <summary>
        /// 订阅发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        public void SubscribeType(string subscribeType)
        {
            var client = OperationContext.Current.ServerClient;
            var listener = MessageCenter.Instance.GetListener(client);
            if (listener == null)
                throw new WarningException("Please enable to subscribe.");

            if (subscribeType != null)
            {
                if (!listener.Types.Contains(subscribeType))
                    listener.Types.Add(subscribeType);
                else
                    throw new WarningException("Already exists subscribe type " + subscribeType + ".");
            }
        }

        /// <summary>
        /// 退订发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        public void UnsubscribeType(string subscribeType)
        {
            var client = OperationContext.Current.ServerClient;
            var listener = MessageCenter.Instance.GetListener(client);
            if (listener == null)
                throw new WarningException("Please enable to subscribe.");

            if (subscribeType != null)
            {
                if (listener.Types.Contains(subscribeType))
                    listener.Types.Remove(subscribeType);
                else
                    throw new WarningException("Don't exist subscribe type " + subscribeType + ".");
            }
        }

        /// <summary>
        /// 获取订阅的应用
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSubscribeApps()
        {
            var client = OperationContext.Current.ServerClient;
            var listener = MessageCenter.Instance.GetListener(client);
            if (listener == null)
                throw new WarningException("Please enable to subscribe.");

            return listener.Apps;
        }

        /// <summary>
        /// 订阅发布应用
        /// </summary>
        /// <param name="appName"></param>
        public void SubscribeApp(string appName)
        {
            var client = OperationContext.Current.ServerClient;
            var listener = MessageCenter.Instance.GetListener(client);
            if (listener == null)
                throw new WarningException("Please enable to subscribe.");

            if (appName != null)
            {
                if (!listener.Apps.Contains(appName))
                    listener.Apps.Add(appName);
                else
                    throw new WarningException("Already exists subscribe app " + appName + ".");
            }
        }

        /// <summary>
        /// 退订发布应用
        /// </summary>
        /// <param name="appName"></param>
        public void UnsubscribeApp(string appName)
        {
            var client = OperationContext.Current.ServerClient;
            var listener = MessageCenter.Instance.GetListener(client);
            if (listener == null)
                throw new WarningException("Please enable to subscribe.");

            if (appName != null)
            {
                if (listener.Apps.Contains(appName))
                    listener.Apps.Remove(appName);
                else
                    throw new WarningException("Don't exist subscribe app " + appName + ".");
            }
        }

        #endregion
    }
}
