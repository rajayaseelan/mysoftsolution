using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MySoft.Data;
using MySoft.IoC.Configuration;
using MySoft.IoC.DataReport.Models;
using MySoft.IoC.Logger;
using MySoft.Logger;

namespace MySoft.IoC.DataReport
{
    /// <summary>
    /// 调用报表
    /// </summary>
    public class CallingReport : IServiceRecorder
    {
        private IDictionary<string, IList<RecordEventArgs>> calls;
        private CastleServiceConfiguration config;

        /// <summary>
        /// 初始化调用报表统计
        /// </summary>
        /// <param name="_config"></param>
        public CallingReport(CastleServiceConfiguration config)
        {
            this.config = config;
            this.calls = new Dictionary<string, IList<RecordEventArgs>>();

            ThreadPool.QueueUserWorkItem(TimerSaveCalling);
        }

        /// <summary>
        /// 定时记录
        /// </summary>
        /// <param name="state"></param>
        private void TimerSaveCalling(object state)
        {
            while (true)
            {
                try
                {
                    IList<RecordEventArgs> records = null;

                    lock (calls)
                    {
                        if (calls.Count > 0)
                        {
                            var key = calls.Keys.ElementAtOrDefault(0);

                            if (!string.IsNullOrEmpty(key))
                            {
                                records = calls[key];
                                calls.Remove(key);
                            }
                        }
                    }

                    //记录统计数据
                    if (records != null && records.Count > 0)
                    {
                        SaveCaller(records);
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("Timer", ex);
                }

                //30秒存一次
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }

        #region IServiceRecorder 成员

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Call(object sender, RecordEventArgs e)
        {
            lock (calls)
            {
                var key = DateTime.Now.ToString("yyyyMMddHHmm");
                if (!calls.ContainsKey(key))
                {
                    calls[key] = new List<RecordEventArgs>();
                }

                calls[key].Add(e);
            }

            if (e.IsError)
            {
                AddError(e);
            }
            else if (e.ElapsedTime > 1000)
            {
                AddTimeout(e);
            }
        }

        #endregion

        /// <summary>
        /// 添加到异常库
        /// </summary>
        /// <param name="e"></param>
        private void AddError(RecordEventArgs e)
        {
            var error = new DbServiceError
            {
                AppName = e.Caller.AppName,
                IPAddress = e.Caller.IPAddress,
                HostName = e.Caller.HostName,
                AppPath = e.Caller.AppPath,
                ServiceName = e.Caller.ServiceName,
                MethodName = e.Caller.MethodName,
                Parameters = e.Caller.Parameters,
                CallTime = e.Caller.CallTime,
                ElapsedTime = Convert.ToInt32(e.ElapsedTime),
                ServerHostName = e.ServerHostName,
                ServerIPAddress = e.ServerIPAddress,
                ServerPort = e.ServerPort,
                ErrMessage = ErrorHelper.GetHtmlError(e.Error),
                AddTime = DateTime.Now
            };

            SaveData(error);
        }

        /// <summary>
        /// 添加到超时库
        /// </summary>
        /// <param name="e"></param>
        private void AddTimeout(RecordEventArgs e)
        {
            var timeout = new DbServiceTimeout
            {
                AppName = e.Caller.AppName,
                IPAddress = e.Caller.IPAddress,
                HostName = e.Caller.HostName,
                AppPath = e.Caller.AppPath,
                ServiceName = e.Caller.ServiceName,
                MethodName = e.Caller.MethodName,
                Parameters = e.Caller.Parameters,
                CallTime = e.Caller.CallTime,
                ElapsedTime = Convert.ToInt32(e.ElapsedTime),
                ServerHostName = e.ServerHostName,
                ServerIPAddress = e.ServerIPAddress,
                ServerPort = e.ServerPort,
                AddTime = DateTime.Now
            };

            SaveData(timeout);
        }

        private int SaveCaller(IList<RecordEventArgs> list)
        {
            var dblist = list.GroupBy(p => new
            {
                AppName = p.Caller.AppName,
                IPAddress = p.Caller.IPAddress,
                HostName = p.Caller.HostName,
                AppPath = p.Caller.AppPath,
                ServiceName = p.Caller.ServiceName,
                MethodName = p.Caller.MethodName,
                ServerHostName = p.ServerHostName,
                ServerIPAddress = p.ServerIPAddress,
                ServerPort = p.ServerPort
            }).Select(p => new DbServiceCaller
            {
                AppName = p.Key.AppName,
                IPAddress = p.Key.IPAddress,
                HostName = p.Key.HostName,
                AppPath = p.Key.AppPath,
                ServiceName = p.Key.ServiceName,
                MethodName = p.Key.MethodName,
                CallCount = p.Count(),
                ElapsedTime = Convert.ToInt32(p.Sum(c => c.ElapsedTime)),
                ServerHostName = p.Key.ServerHostName,
                ServerIPAddress = p.Key.ServerIPAddress,
                ServerPort = p.Key.ServerPort,
                AddTime = DateTime.Now
            })
            .ToList();

            return SaveData(dblist);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        private int SaveData<T>(T item)
            where T : class
        {
            try
            {
                return InsertOperate<T>.Create("sdp").Execute(item);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Insert", ex);

                return -1;
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        private int SaveData<T>(T[] items)
            where T : class
        {
            try
            {
                int count = 0;
                using (IDataSession session = new DataSession("sdp"))
                {
                    session.Open();
                    session.BeginTran();

                    try
                    {
                        foreach (var item in items)
                        {
                            count += InsertOperate<T>.Create().Execute(item, session);
                        }

                        session.Commit();
                    }
                    catch
                    {
                        session.RollBack();
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("Insert", ex);

                return -1;
            }
        }
    }
}
