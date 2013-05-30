using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 时间状态信息
    /// </summary>
    [Serializable]
    public class TimeStatus : SecondStatus
    {
        /// <summary>
        /// 记数时间
        /// </summary>
        public DateTime CounterTime { get; set; }
    }

    /// <summary>
    /// 时间状态集合
    /// </summary>
    [Serializable]
    internal class TimeStatusCollection
    {
        private int maxCount;
        private IDictionary<string, TimeStatus> hashtable = new Dictionary<string, TimeStatus>();

        public TimeStatusCollection(int maxCount)
        {
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 获取或创建
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TimeStatus GetOrCreate(DateTime value)
        {
            string key = value.ToString("yyyyMMddHHmmss");

            lock (hashtable)
            {
                if (!hashtable.ContainsKey(key))
                {
                    //如果总数大于传入的总数
                    if (hashtable.Count > 0 && hashtable.Count >= maxCount)
                    {
                        var firstKey = hashtable.Keys.Min();
                        if (firstKey != null) hashtable.Remove(firstKey);
                    }

                    hashtable[key] = new TimeStatus { CounterTime = value };
                }

                return hashtable[key];
            }
        }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count
        {
            get
            {
                lock (hashtable)
                {
                    return hashtable.Count;
                }
            }
        }

        /// <summary>
        /// 返回列表
        /// </summary>
        /// <returns></returns>
        public IList<TimeStatus> ToList()
        {
            lock (hashtable)
            {
                return hashtable.Values.Cast<TimeStatus>().ToList();
            }
        }

        /// <summary>
        /// 获取最后一条
        /// </summary>
        /// <returns></returns>
        public TimeStatus GetNewest()
        {
            lock (hashtable)
            {
                if (hashtable.Count > 0)
                {
                    var lastKey = hashtable.Keys.Max();
                    if (lastKey != null)
                    {
                        return hashtable[lastKey] as TimeStatus;
                    }
                }
            }

            return new TimeStatus { CounterTime = DateTime.Now };
        }

        /// <summary>
        /// 清除字典中的数据
        /// </summary>
        public void Clear()
        {
            lock (hashtable)
            {
                hashtable.Clear();
            }
        }
    }
}

