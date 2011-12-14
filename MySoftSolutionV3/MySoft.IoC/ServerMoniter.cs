using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MySoft.IoC.Configuration;
using MySoft.IoC.Status;
using MySoft.Logger;
using System.Threading;

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
            this.container.OnError += new ErrorLogEventHandler(container_OnError);
            this.container.OnLog += new LogEventHandler(container_OnLog);
            this.statuslist = new TimeStatusCollection(config.Records);
            this.startTime = DateTime.Now;

            //启动定义推送线程
            ThreadPool.QueueUserWorkItem(DoPushWork);
        }

        void DoPushWork(object state)
        {
            while (true)
            {
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

        protected void container_OnLog(string log, LogType type)
        {
            try
            {
                if (OnLog != null) OnLog(log, type);
            }
            catch (Exception)
            {
            }
        }

        protected void container_OnError(Exception error)
        {
            try
            {
                if (OnError != null) OnError(error);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;

        /// <summary>
        /// OnError event.
        /// </summary>
        public event ErrorLogEventHandler OnError;

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

        /// <summary>
        /// 获取服务信息列表
        /// </summary>
        /// <returns></returns>
        public IList<ServiceInfo> GetServiceList()
        {
            //获取拥有ServiceContract约束的服务
            var types = container.GetInterfaces<ServiceContractAttribute>();

            var services = new List<ServiceInfo>();
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
                            TypeName = parameter.ParameterType.Name,
                            TypeFullName = parameter.ParameterType.FullName
                        };
                        m.Parameters.Add(p);
                    }

                    s.Methods.Add(m);
                }

                services.Add(s);
            }

            return services;
        }

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

        #region 服务器状态信息

        /// <summary>
        /// 获取最后一次服务状态
        /// </summary>
        /// <returns></returns>
        public TimeStatus GetLatestStatus()
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
                    highest.DataFlowCounterTime = list.First(p => p.DataFlow == highest.DataFlow).CounterTime;

                //成功
                highest.SuccessCount = list.Max(p => p.SuccessCount);
                if (highest.SuccessCount > 0)
                    highest.SuccessCountCounterTime = list.First(p => p.SuccessCount == highest.SuccessCount).CounterTime;

                //失败
                highest.ErrorCount = list.Max(p => p.ErrorCount);
                if (highest.ErrorCount > 0)
                    highest.ErrorCountCounterTime = list.First(p => p.ErrorCount == highest.ErrorCount).CounterTime;

                //请求总数
                highest.RequestCount = list.Max(p => p.RequestCount);
                if (highest.RequestCount > 0)
                    highest.RequestCountCounterTime = list.First(p => p.RequestCount == highest.RequestCount).CounterTime;

                //耗时
                highest.ElapsedTime = list.Max(p => p.ElapsedTime);
                if (highest.ElapsedTime > 0)
                    highest.ElapsedTimeCounterTime = list.First(p => p.ElapsedTime == highest.ElapsedTime).CounterTime;
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

        #endregion

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

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 销毁资源
        /// </summary>
        public virtual void Dispose()
        {
            container.Dispose();
        }

        #endregion

        #region IStatusService 成员

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
            MessageCenter.Instance.Push(GetClientList());
        }

        /// <summary>
        /// 获取订阅的类型
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSubscribeTypes()
        {
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
            if (listener == null) return new List<string>();

            return listener.SubscribeTypes;
        }

        /// <summary>
        /// 添加发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        public void AddSubscribeType(string subscribeType)
        {
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
            if (listener == null) return;

            if (subscribeType != null)
            {
                if (!listener.SubscribeTypes.Contains(subscribeType))
                    listener.SubscribeTypes.Add(subscribeType);
                else
                    throw new WarningException("Already exists subscribe type " + subscribeType);
            }
        }

        /// <summary>
        /// 添加发布类型
        /// </summary>
        /// <param name="subscribeType"></param>
        public void RemoveSubscribeType(string subscribeType)
        {
            var endPoint = OperationContext.Current.RemoteEndPoint;
            var listener = MessageCenter.Instance.GetListener(endPoint);
            if (listener == null) return;

            if (subscribeType != null)
            {
                if (listener.SubscribeTypes.Contains(subscribeType))
                    listener.SubscribeTypes.Remove(subscribeType);
                else
                    throw new WarningException("Don't exist subscribe type " + subscribeType);
            }
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

        #endregion
    }
}