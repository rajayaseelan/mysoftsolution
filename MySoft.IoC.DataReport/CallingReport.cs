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
    public class CallingReport : IServiceCall
    {
        private IDictionary<string, IList<CallEventArgs>> calls;
        private IList<CallEventArgs> errors;
        private CastleService server;
        private CastleServiceConfiguration config;

        /// <summary>
        /// 初始化调用报表统计
        /// </summary>
        /// <param name="_config"></param>
        /// <param name="_server"></param>
        public CallingReport(CastleServiceConfiguration config)
        {
            this.config = config;
            calls = new Dictionary<string, IList<CallEventArgs>>();
            errors = new List<CallEventArgs>();

            ThreadPool.QueueUserWorkItem(TimerSaveCalling);
        }

        void TimerSaveCalling(object state)
        {

        }

        #region IServiceRecorder 成员

        public void Recorder(object sender, CallEventArgs e)
        {
            e.Value = null;

            if (e.IsError)
            {
                errors.Add(e);
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
    }
}
