using System;
using System.Collections;
using System.Threading;
using MySoft.IoC.Communication.Scs.Server;

namespace MySoft.IoC
{
    /// <summary>
    /// 线程管理
    /// </summary>
    internal static class ThreadManager
    {
        //实例化队列
        private static Hashtable hashtable = new Hashtable();

        /// <summary>
        /// 添加线程
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="thread"></param>
        public static void Add(IScsServerClient channel, Thread thread)
        {
            if (channel == null) return;
            var key = channel.ClientId;

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(key))
                {
                    //将当前线程放入队列中
                    hashtable[key] = thread;
                }
            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="channel"></param>
        public static void Remove(IScsServerClient channel)
        {
            if (channel == null) return;
            var key = channel.ClientId;

            lock (hashtable.SyncRoot)
            {
                if (hashtable.ContainsKey(key))
                {
                    hashtable.Remove(key);
                }
            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="channel"></param>
        public static void Cancel(IScsServerClient channel)
        {
            if (channel == null) return;
            var key = channel.ClientId;

            lock (hashtable.SyncRoot)
            {
                if (hashtable.ContainsKey(key))
                {
                    var item = hashtable[key];
                    CancelThread(item as Thread);

                    hashtable.Remove(key);
                }
            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        private static void CancelThread(Thread thread)
        {
            //结束线程
            if (thread != null)
            {
                try
                {
                    thread.Abort();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}