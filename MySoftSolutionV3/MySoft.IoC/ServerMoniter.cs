using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySoft.IoC.Configuration;
using MySoft.IoC.Services;
using MySoft.IoC.Status;
using MySoft.Logger;

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
        public IList<Type> GetServiceList()
        {
            //获取拥有ServiceContract约束的服务
            var types = container.GetInterfaces<ServiceContractAttribute>();

            return types.ToList();
        }

        /// <summary>
        /// 清除所有服务器状态
        /// </summary>
        public void ClearStatus()
        {
            lock (statuslist)
            {
                statuslist.Clear();
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
            return statuslist.GetNewest();
        }

        /// <summary>
        /// 获取最高状态信息
        /// </summary>
        /// <returns></returns>
        public HighestStatus GetHighestStatus()
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
        public abstract IList<ClientInfo> GetClientInfoList();

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
    }
}