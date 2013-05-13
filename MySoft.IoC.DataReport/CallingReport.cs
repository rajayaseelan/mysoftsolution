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
        private long timeout = 1000;

        /// <summary>
        /// 初始化调用报表统计
        /// </summary>
        /// <param name="config"></param>
        /// <param name="timeout"></param>
        public CallingReport(CastleServiceConfiguration config, long timeout)
        {
            this.config = config;
            this.calls = new SortedList<string, IList<RecordEventArgs>>();
            if (timeout > 0) this.timeout = timeout;

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
                //1分钟保存一次
                Thread.Sleep(TimeSpan.FromMinutes(1));

                try
                {
                    IList<RecordEventArgs> records = null;
                    string callKey = null;

                    lock (calls)
                    {
                        if (calls.Count > 0)
                        {
                            callKey = calls.Keys.ElementAtOrDefault(0);

                            if (!string.IsNullOrEmpty(callKey))
                            {
                                records = calls[callKey];
                                calls.Remove(callKey);
                            }
                        }
                    }

                    //记录统计数据
                    if (records != null && records.Count > 0)
                    {
                        var count = SaveCaller(callKey, records);

                        if (count == -1) //存储失败，重新加入队列中
                        {
                            lock (calls)
                            {
                                calls[callKey] = records;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("SaveCaller", ex);
                }
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
            else if (e.ElapsedTime > timeout)
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
            var error = new ServiceError
            {
                AppName = e.Caller.AppName,
                IPAddress = e.Caller.IPAddress,
                HostName = e.Caller.HostName,
                AppPath = e.Caller.AppPath,
                ServiceName = e.Caller.ServiceName,
                MethodName = e.Caller.MethodName,
                Parameters = e.Caller.Parameters,
                CallTime = e.Caller.CallTime,
                ElapsedTime = e.ElapsedTime,
                ServerHostName = e.ServerHostName,
                ServerIPAddress = e.ServerIPAddress,
                ServerPort = e.ServerPort,
                ErrType = e.Error.GetType().FullName,
                ErrMessage = ErrorHelper.GetHtmlError(e.Error),
                AddTime = DateTime.Now
            };

            //存储数据
            SaveData(error);
        }

        /// <summary>
        /// 添加到超时库
        /// </summary>
        /// <param name="e"></param>
        private void AddTimeout(RecordEventArgs e)
        {
            var timeout = new ServiceTimeout
            {
                AppName = e.Caller.AppName,
                IPAddress = e.Caller.IPAddress,
                HostName = e.Caller.HostName,
                AppPath = e.Caller.AppPath,
                ServiceName = e.Caller.ServiceName,
                MethodName = e.Caller.MethodName,
                Parameters = e.Caller.Parameters,
                CallTime = e.Caller.CallTime,
                ElapsedTime = e.ElapsedTime,
                ServerHostName = e.ServerHostName,
                ServerIPAddress = e.ServerIPAddress,
                ServerPort = e.ServerPort,
                AddTime = DateTime.Now
            };

            //存储数据
            SaveData(timeout);
        }

        private int SaveCaller(string key, IList<RecordEventArgs> list)
        {
            var dblist = list.GroupBy(p => new
            {
                AppName = p.Caller.AppName,
                IPAddress = p.Caller.IPAddress,
                AppPath = p.Caller.AppPath,
                ServiceName = p.Caller.ServiceName,
                MethodName = p.Caller.MethodName,
                ServerIPAddress = p.ServerIPAddress,
                ServerPort = p.ServerPort
            }).Select(p => new ServiceCaller
            {
                CallKey = key,
                AppName = p.Key.AppName,
                IPAddress = p.Key.IPAddress,
                HostName = p.Max(c => c.Caller.HostName),
                AppPath = p.Key.AppPath,
                ServiceName = p.Key.ServiceName,
                MethodName = p.Key.MethodName,
                RowCount = p.Sum(c => c.Count),
                CallCount = p.Count(),
                ElapsedTime = p.Sum(c => c.ElapsedTime),
                ServerHostName = p.Max(c => c.ServerHostName),
                ServerIPAddress = p.Key.ServerIPAddress,
                ServerPort = p.Key.ServerPort,
                AddTime = DateTime.Now
            })
            .ToArray();

            return SaveData(dblist);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        private int SaveData<T>(T item)
            where T : Entity
        {
            try
            {
                return DbSession.Default.Insert(item);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("InsertItem", ex);

                return -1;
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        private int SaveData<T>(T[] items)
            where T : Entity
        {
            try
            {
                int count = 0;
                using (var dbTrans = DbSession.Default.BeginTrans())
                {
                    try
                    {
                        foreach (var item in items)
                        {
                            count += dbTrans.Insert(item);
                        }

                        dbTrans.Commit();
                    }
                    catch
                    {
                        dbTrans.Rollback();
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("InsertItems", ex);

                return -1;
            }
        }
    }
}
