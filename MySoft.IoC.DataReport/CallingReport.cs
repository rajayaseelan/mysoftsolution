using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Configuration;
using MySoft.IoC.Logger;
using MySoft.IoC.Messages;

namespace MySoft.IoC.DataReport
{
    /// <summary>
    /// 调用报表
    /// </summary>
    public class CallingReport : IServiceRecorder
    {
        private IDictionary<string, IList<CallEventArgs>> calls;
        private CastleServiceConfiguration config;

        /// <summary>
        /// 初始化调用报表统计
        /// </summary>
        /// <param name="_config"></param>
        public CallingReport(CastleServiceConfiguration config)
        {
            this.config = config;
            this.calls = new Dictionary<string, IList<CallEventArgs>>();

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
            }
        }

        #region IServiceRecorder 成员

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Call(object sender, CallEventArgs e)
        {
            e.Value = null;

            if (e.IsError)
            {
                AddError(e);
            }
            else if (e.IsTimeout)
            {
                AddTimeout(e);
            }

            lock (calls)
            {
                var key = DateTime.Now.ToString("yyyyMMddHHmm");
                if (!calls.ContainsKey(key))
                {
                    calls[key] = new List<CallEventArgs>();
                }

                calls[key].Add(e);
            }
        }

        #endregion

        private void AddError(CallEventArgs e)
        {
        }

        public void AddTimeout(CallEventArgs e)
        {
        }
    }
}
