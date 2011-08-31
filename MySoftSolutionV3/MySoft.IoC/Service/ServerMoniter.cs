using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Logger;
using MySoft.IoC.Configuration;
using MySoft.Net.Sockets;
using System.Collections;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务监控
    /// </summary>
    public class ServerMoniter : IStatusService, ILogable, IErrorLogable
    {
        protected IServiceContainer container;
        protected CastleServiceConfiguration config;
        protected SocketServer server;
        protected TimeStatusCollection statuslist;
        protected HighestStatus highest;
        private DateTime startTime;

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
            this.highest = new HighestStatus();
            this.startTime = DateTime.Now;

            //实例化Socket服务
            server = new SocketServer();
            server.MaxAccept = config.MaxConnect;
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

        protected void container_OnError(Exception exception)
        {
            try
            {
                if (OnError != null) OnError(exception);
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
        /// 获取服务信息列表
        /// </summary>
        /// <returns></returns>
        public IList<ServiceInfo> GetServiceInfoList()
        {
            var list = new List<ServiceInfo>();
            foreach (Type type in container.GetInterfaces<ServiceContractAttribute>())
            {
                var service = new ServiceInfo
                {
                    Assembly = type.Assembly.FullName,
                    Name = type.FullName,
                    Methods = CoreHelper.GetMethodsFromType(type)
                };

                list.Add(service);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 清除所有服务器状态
        /// </summary>
        public void ClearStatus()
        {
            lock (statuslist)
            {
                statuslist.Clear();
                highest = new HighestStatus();
            }
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
        /// 获取最后一次服务状态
        /// </summary>
        /// <returns></returns>
        public TimeStatus GetLatestStatus()
        {
            return statuslist.GetLast();
        }

        /// <summary>
        /// 获取最高状态信息
        /// </summary>
        /// <returns></returns>
        public HighestStatus GetHighestStatus()
        {
            return highest;
        }

        /// <summary>
        /// 汇总状态信息
        /// </summary>
        /// <returns></returns>
        public SummaryStatus GetSummaryStatus()
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
        public IList<ConnectInfo> GetConnectInfoList()
        {
            try
            {
                return server.SocketClientList.Cast<SocketClient>().GroupBy(p => p.IpAddress)
                     .Select(p => new ConnectInfo { IP = p.Key, Count = p.Count() })
                     .ToList();
            }
            catch (Exception ex)
            {
                container_OnError(ex);

                return new List<ConnectInfo>();
            }
        }

        #endregion
    }
}