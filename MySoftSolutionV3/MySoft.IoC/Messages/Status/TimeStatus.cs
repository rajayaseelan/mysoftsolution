using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 时间状态信息
    /// </summary>
    [Serializable]
    public class TimeStatus : SecondStatus
    {
        private DateTime counterTime;
        /// <summary>
        /// 记数时间
        /// </summary>
        public DateTime CounterTime
        {
            get
            {
                return counterTime;
            }
            set
            {
                counterTime = value;
            }
        }
    }

    /// <summary>
    /// 时间状态集合
    /// </summary>
    [Serializable]
    internal class TimeStatusCollection
    {
        private int maxCount;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
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

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(key))
                {
                    //如果总数大于传入的总数
                    if (hashtable.Count > 0 && hashtable.Count >= maxCount)
                    {
                        var firstKey = hashtable.Keys.Cast<string>().Min();
                        if (firstKey != null) hashtable.Remove(firstKey);
                    }

                    hashtable[key] = new TimeStatus { CounterTime = value };
                }
            }

            return hashtable[key] as TimeStatus;
        }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count
        {
            get { return hashtable.Count; }
        }

        /// <summary>
        /// 返回列表
        /// </summary>
        /// <returns></returns>
        public IList<TimeStatus> ToList()
        {
            lock (hashtable.SyncRoot)
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
            lock (hashtable.SyncRoot)
            {
                if (hashtable.Count > 0)
                {
                    var lastKey = hashtable.Keys.Cast<string>().Max();
                    if (lastKey != null) return hashtable[lastKey] as TimeStatus;
                }
            }

            return new TimeStatus { CounterTime = DateTime.Now };
        }

        /// <summary>
        /// 清除字典中的数据
        /// </summary>
        public void Clear()
        {
            lock (hashtable.SyncRoot)
            {
                hashtable.Clear();
            }
        }
    }
}

