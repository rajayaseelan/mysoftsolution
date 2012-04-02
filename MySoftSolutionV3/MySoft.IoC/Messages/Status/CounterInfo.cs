using System;
using System.Linq;
using System.Collections;
using MySoft.Logger;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 计数器集合
    /// </summary>
    [Serializable]
    internal class CounterInfoCollection
    {
        private ILog logger;
        private int maxCount;
        private int count;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化CounterInfoCollection
        /// </summary>
        /// <param name="maxCount"></param>
        public CounterInfoCollection(ILog logger, int maxCount)
        {
            this.logger = logger;
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="args"></param>
        public void Call(CallEventArgs args)
        {
            //计数按方法
            var jsonString = ServiceConfig.FormatJson(args.Caller.Parameters);
            string callKey = string.Format("{0}${1}${2}${3}", args.Caller.AppName, args.Caller.ServiceName, args.Caller.MethodName, jsonString);

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(callKey))
                {
                    var counterInfo = new CounterInfo
                    {
                        AppName = args.Caller.AppName,
                        ServiceName = args.Caller.ServiceName,
                        MethodName = args.Caller.MethodName,
                        Parameters = args.Caller.Parameters,
                        NeedReset = false,
                        Count = 1
                    };

                    hashtable[callKey] = counterInfo;

                    return;
                }
            }

            var counter = hashtable[callKey] as CounterInfo;
            if (counter.NeedReset)
            {
                //重置计数器
                hashtable.Remove(callKey);

                //如果调用次数超过最大允许数，则提示警告
                if (counter.Count >= maxCount)
                {
                    var warning = new WarningException(string.Format("【{0}】 One minute call service ({1}, {2}) {3} times more than {4} times.\r\nParameters => {5}",
                      counter.AppName, counter.ServiceName, counter.MethodName, counter.Count, maxCount, counter.Parameters));

                    //内部异常
                    var error = new IoCException(string.Format("【{0}】 One minute call service ({1}) {2} times.",
                        counter.AppName, counter.ServiceName, counter.Count), warning);

                    //写错误日志
                    logger.WriteError(error);

                    //抛出异常
                    args.Error = error;
                }
            }
            else
            {
                //计数器加1
                counter.Count++;
            }
        }

        /// <summary>
        /// 计数器加一
        /// </summary>
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        public void Reset()
        {
            //将计数清零
            lock (hashtable.SyncRoot)
            {
                var list = hashtable.Values.Cast<CounterInfo>().ToList();
                list.ForEach(counter => counter.NeedReset = true);

                this.count = 0;
            }
        }
    }

    /// <summary>
    /// 调用计数器
    /// </summary>
    [Serializable]
    public class CounterInfo
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// 调用次数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 是否重置状态
        /// </summary>
        public bool NeedReset { get; set; }
    }
}
