using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.IoC.Configuration;
using System.Threading;
using System.Linq;

namespace MySoft.IoC.DataReport
{
    /// <summary>
    /// 调用报表
    /// </summary>
    public class CallingReport
    {
        private static IDictionary<string, IList<CallEventArgs>> calls;
        private static IList<CallEventArgs> errors;
        private static CastleService server;
        private static CastleServiceConfiguration config;

        /// <summary>
        /// 初始化调用报表统计
        /// </summary>
        /// <param name="_config"></param>
        /// <param name="_server"></param>
        public static void Init(CastleServiceConfiguration _config, CastleService _server)
        {
            if (server == null)
            {
                config = _config;
                server = _server;
                calls = new Dictionary<string, IList<CallEventArgs>>();
                errors = new List<CallEventArgs>();

                server.OnCalling += new EventHandler<CallEventArgs>(server_OnCalling);
            }

            ThreadPool.QueueUserWorkItem(TimerSaveCalling);
        }

        static void TimerSaveCalling(object state)
        {

        }

        static void server_OnCalling(object sender, CallEventArgs e)
        {
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
    }
}
