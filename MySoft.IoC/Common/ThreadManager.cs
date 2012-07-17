using System;
using System.Collections.Generic;
using System.Threading;

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
        /// 当前队列总数
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
        /// <param name="id"></param>
        /// <param name="thread"></param>
        public static void Add(Guid id, Thread thread)
        {
            lock (hashtable)
            {
                //将当前线程放入队列中
                hashtable[id] = thread;
            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="id"></param>
        public static void Cancel(Guid id)
        {
            if (hashtable.Count == 0) return;

            Thread thread = null;

            //获取指定Key的线程
            lock (hashtable)
            {
                if (hashtable.ContainsKey(id))
                {
                    thread = hashtable[id];

                    hashtable.Remove(id);
                }
            }

            //Cancel thread
            CancelThread(thread);
        }

        /// <summary>
        /// Cancel thread
        /// </summary>
        /// <param name="thread"></param>
        private static void CancelThread(Thread thread)
        {
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
        /// <param name="id"></param>
        public static void Remove(Guid id)
        {
            if (hashtable.Count == 0) return;

            lock (hashtable)
            {
                if (hashtable.ContainsKey(id))
                {
                    //移除线程
                    hashtable.Remove(id);
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
