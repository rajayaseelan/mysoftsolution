using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace MySoft.IoC
{
    /// <summary>
    /// 线程管理
    /// </summary>
    internal static class ThreadManager
    {
        //实例化队列
        private static IDictionary<Guid, Thread> hashtable = new Dictionary<Guid, Thread>();

        /// <summary>
        /// 当前线程总数
        /// </summary>
        public static int Count
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
        /// 添加线程
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="thread"></param>
        public static void Add(Guid guid, Thread thread)
        {
            lock (hashtable)
            {
                //将当前线程放入队列中
                hashtable[guid] = thread;
            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="guid"></param>
        public static void Cancel(Guid guid)
        {
            Thread thread = null;

            //获取指定Key的线程
            lock (hashtable)
            {
                if (hashtable.ContainsKey(guid))
                {
                    thread = hashtable[guid];

                    //移除线程
                    hashtable.Remove(guid);
                }
            }

            if (thread == null) return;

            try
            {
                //中止当前线程
                var ts = GetThreadState(thread.ThreadState);

                if (ts == ThreadState.WaitSleepJoin)
                {
                    thread.Interrupt();
                }
                else if (ts == ThreadState.Running)
                {
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
        }

        /// <summary>
        /// 移除线程
        /// </summary>
        /// <param name="guid"></param>
        public static void Remove(Guid guid)
        {
            lock (hashtable)
            {
                if (hashtable.ContainsKey(guid))
                {
                    hashtable.Remove(guid);
                }
            }
        }

        /// <summary>
        /// Get thread state
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private static ThreadState GetThreadState(ThreadState ts)
        {
            return ts & (ThreadState.Aborted | ThreadState.AbortRequested |
                         ThreadState.Stopped | ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin);
        }
    }
}
