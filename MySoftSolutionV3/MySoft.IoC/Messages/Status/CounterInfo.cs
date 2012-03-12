using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private IDictionary<string, CounterInfo> dictCounter;

        /// <summary>
        /// 实例化CounterInfoCollection
        /// </summary>
        /// <param name="maxCount"></param>
        public CounterInfoCollection(ILog logger, int maxCount)
        {
            this.dictCounter = new Dictionary<string, CounterInfo>();
            this.logger = logger;
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="args"></param>
        public void CallCounter(CallEventArgs args)
        {
            //计数按应用走
            string callKey = string.Format("Call_{0}_{1}_{2}", args.Caller.AppName, args.Caller.ServiceName, args.Caller.MethodName);
            lock (dictCounter)
            {
                if (!dictCounter.ContainsKey(callKey))
                {
                    dictCounter[callKey] = new CounterInfo
                    {
                        AppName = args.Caller.AppName,
                        ServiceName = args.Caller.ServiceName,
                        MethodName = args.Caller.MethodName
                    };
                }
            }

            var counter = dictCounter[callKey];
            lock (counter)
            {
                if (counter.NeedReset)
                {
                    //如果调用次数超过最大允许数，则提示警告
                    if (counter.Count >= maxCount)
                    {
                        var warning = new WarningException(string.Format("【{0}】 One minute call service ({1}, {2}) {3} times more than {4} times.",
                          counter.AppName, counter.ServiceName, counter.MethodName, counter.Count, maxCount));

                        //内部异常
                        var error = new IoCException(string.Format("【{0}】 One minute call service ({1}) {2} times.",
                            counter.AppName, counter.ServiceName, counter.Count), warning);

                        //写错误日志
                        logger.WriteError(error);

                        //抛出异常
                        args.Error = error;
                    }

                    //重置计数器
                    counter.Reset();
                }

                //计数器加1
                counter.Count++;
            }
        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        public void Reset()
        {
            lock (dictCounter)
            {
                //将计数清零
                foreach (var kvp in dictCounter)
                {
                    kvp.Value.NeedReset = true;
                }
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
        /// 调用次数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 是否重置状态
        /// </summary>
        public bool NeedReset { get; set; }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            lock (this)
            {
                this.NeedReset = false;
                this.Count = 0;
            }
        }
    }
}
