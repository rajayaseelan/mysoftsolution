using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CounterInfoCollection
    {
        private int maxCount;
        private IDictionary<string, CounterInfo> dictCounter;

        /// <summary>
        /// 实例化CounterInfoCollection
        /// </summary>
        /// <param name="maxCount"></param>
        public CounterInfoCollection(int maxCount)
        {
            this.dictCounter = new Dictionary<string, CounterInfo>();
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="args"></param>
        public void CallCounter(CallEventArgs args)
        {
            string callKey = string.Format("Call_{0}_{1}", args.Caller.ServiceName, args.Caller.MethodName);
            lock (dictCounter)
            {
                if (!dictCounter.ContainsKey(callKey))
                {
                    dictCounter[callKey] = new CounterInfo
                    {
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
                        args.Error = new WarningException(string.Format("One minute call method ({0}, {1}) {2} times more than {3} times.",
                            counter.ServiceName, counter.MethodName, counter.Count, maxCount));
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
