using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.RESTful;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务监控
    /// </summary>
    public abstract class ServerMoniter : IStatusService, ILogable, IErrorLogable, IDisposable
    {
        protected IServiceContainer container;
        protected CastleServiceConfiguration config;
        protected TimeStatusCollection statuslist;
        private DateTime startTime;

        /// <summary>
        /// 实例化ServerMoniter
        /// </summary>
        /// <param name="config"></param>
        public ServerMoniter(CastleServiceConfiguration config)
        {
            this.config = config;

            //注入内部的服务
            Hashtable hashTypes = new Hashtable();
            hashTypes[typeof(IStatusService)] = this;

            this.container = new SimpleServiceContainer(CastleFactoryType.Local, hashTypes);
            this.container.OnError += error => { if (OnError != null) OnError(error); };
            this.container.OnLog += (log, type) => { if (OnLog != null) OnLog(log, type); };
            this.statuslist = new TimeStatusCollection(config.Records);
            this.startTime = DateTime.Now;

            //加载cacheType
            if (!string.IsNullOrEmpty(config.CacheType))
            {
                Type type = Type.GetType(config.CacheType);
                object instance = Activator.CreateInstance(type);
                this.container.ServiceCache = instance as IServiceCache;
            }

            //启动定义推送线程
            ThreadPool.QueueUserWorkItem(DoPushWork);
        }

        void DoPushWork(object state)
        {
            while (true)
            {
                if (statuslist == null)
                    break;

                //响应定时信息
                if (statuslist.Count > 0)
                {
                    var status = GetServerStatus();
                    MessageCenter.Instance.Notify(status);
                }

                //每秒推送一次
                Thread.Sleep(1000);
            }
        }

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

        #region IDisposable 成员

        /// <summary>
        /// 销毁资源
        /// </summary>
        public abstract void Dispose();

        #endregion

        #region IStatusService 成员

        /// <summary>
        /// 是否存在服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool ContainsService(string serviceName)
        {
            return container.Contains<ServiceContractAttribute>(serviceName);
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
                var types = container.GetInterfaces<ServiceContractAttribute>();

                services = new List<ServiceInfo>();
                foreach (var type in types)
                {
                    string description1 = null;
                    var contract1 = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                    if (contract1 != null) description1 = contract1.Description;
                    var s = new ServiceInfo
                    {
                        Assembly = type.Assembly.FullName,
                        Name = type.Name,
                        FullName = type.FullName,
                        Description = description1
                    };

                    //读取方法
                    foreach (var method in CoreHelper.GetMethodsFromType(type))
                    {
                        string description2 = null;
                        var contract2 = CoreHelper.GetMemberAttribute<OperationContractAttribute>(type);
                        if (contract2 != null) description2 = contract2.Description;
                        var m = new MethodInfo
                        {
                            Name = method.Name,
                            FullName = method.ToString(),
                            Description = description2
                        };

                        //读取参数
                        foreach (var parameter in method.GetParameters())
                        {
                            var p = new ParameterInfo
                            {
                                Name = parameter.Name,
                                TypeName = GetTypeName(parameter.ParameterType),
                                TypeFullName = parameter.ParameterType.FullName,
                                IsByRef = parameter.ParameterType.IsByRef,
                                IsOut = parameter.IsOut,
                                IsEnum = parameter.ParameterType.IsEnum,
                                IsPrimitive = CheckPrimitive(parameter.ParameterType),
                                SubParameters = GetSubParameters(parameter.ParameterType)
                            };

                            if (p.IsEnum)
                            {
                                p.EnumValue = GetEnumValue(parameter.ParameterType);
                            }

                            m.Parameters.Add(p);
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
            if (type.IsByRef) type = type.GetElementType();
            if (type.IsArray) type = type.GetElementType();
            if (type.IsGenericType) type = type.GetGenericArguments()[0];

            if (GetTypeClass(type) && !typeof(ICollection).IsAssignableFrom(type))
            {
                var plist = new List<ParameterInfo>();
                foreach (var p in CoreHelper.GetPropertiesFromType(type))
                {
                    var pi = new ParameterInfo
                    {
                        Name = p.Name,
                        TypeName = GetTypeName(p.PropertyType),
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

        private string GetTypeName(Type type)
        {
            string typeName = type.Name;
            if (type.IsGenericType) type = type.GetGenericArguments()[0];
            if (typeName.Contains("`1"))
            {
                typeName = typeName.Replace("`1", "<" + type.Name + ">");
            }
            return typeName;
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
                TotalSeconds = (int)DateTime.Now.Subtract(startTime).TotalSeconds,
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

            //处理最高值 
            #region 处理最高值

            if (list.Count > 0)
            {
                //流量
                highest.DataFlow = list.Max(p => p.DataFlow);
                if (highest.DataFlow > 0)
                {
                    var data = list.FirstOrDefault(p => p.DataFlow == highest.DataFlow);
                    highest.DataFlowCounterTime = data == null ? DateTime.MinValue : data.CounterTime;
                }

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
            SummaryStatus status = new SummaryStatus
            {
                RunningSeconds = list.Count,
                RequestCount = list.Sum(p => p.RequestCount),
                SuccessCount = list.Sum(p => p.SuccessCount),
                ErrorCount = list.Sum(p => p.ErrorCount),
                ElapsedTime = list.Sum(p => p.ElapsedTime),
                DataFlow = list.Sum(p => p.DataFlow),
            };

            return status;
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
        /// 获取连接客户信息
        /// </summary>
        /// <returns></returns>
        public abstract IList<ClientInfo> GetClientList();

        /// <summary>
        /// 获取所有应用客户端
        /// </summary>
        /// <returns></returns>
        public abstract IList<AppClient> GetAppClients();

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
        /// <param name="callTimeout">调用超时时间</param>
        public void Subscribe(double callTimeout, params string[] subscribeTypes)
        {
            Subscribe(new SubscribeOptions
            {
                CallTimeout = callTimeout
            }, subscribeTypes);
        }

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="statusTimer">定时推送时间</param>
        public void Subscribe(int statusTimer, params string[] subscribeTypes)
        {
            Subscribe(new SubscribeOptions
            {
                StatusTimer = statusTimer
            }, subscribeTypes);
        }

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="callTimeout">调用超时时间</param>
        /// <param name="statusTimer">定时推送时间</param>
        public void Subscribe(double callTimeout, int statusTimer, params string[] subscribeTypes)
        {
            Subscribe(new SubscribeOptions
            {
                CallTimeout = callTimeout,
                StatusTimer = statusTimer
            }, subscribeTypes);
        }

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="options">订阅选项</param>
        public void Subscribe(SubscribeOptions options, params string[] subscribeTypes)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IStatusListener>();
            var endPoint = OperationContext.Current.RemoteEndPoint;
            MessageCenter.Instance.AddListener(new MessageListener(endPoint, callback, options, subscribeTypes));

            //推送客户端连接信息
            MessageCenter.Instance.Notify(GetClientList());
        }

        /// <summary>
        /// 退订
        /// </summary>
        public void Unsubscribe()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IStatusListener>();
            var endPoint = OperationContext.Current.RemoteEndPoint;
            MessageCenter.Instance.RemoveListener(new MessageListener(endPoint, callback));
        }

        /// <summary>
        /// 获取订阅的类型
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSubscribeTypes()
        {
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
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
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
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
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
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
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
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
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
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
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
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