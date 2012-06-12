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
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 当前线程总数
        /// </summary>
        public static int Count
        {
            get { return hashtable.Count; }
        }

        /// <summary>
        /// 添加线程
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="thread"></param>
        public static void Add(Guid guid, Thread thread)
        {
            //将当前线程放入队列中
            hashtable[guid] = thread;
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="guid"></param>
        public static void Cancel(Guid guid)
        {
            if (hashtable.ContainsKey(guid))
            {
                try
                {
                    //中止当前线程
                    var thread = hashtable[guid] as Thread;
                    if ((thread.ThreadState & (ThreadState.Stopped | ThreadState.Unstarted)) == ThreadState.Running)
                    {
                        thread.Abort();
                    }
                }
                catch (Exception ex)
                {
                    //TODO
                }
                finally
                {
                    hashtable.Remove(guid);
                }
            }
        }

        /// <summary>
        /// 移除线程
        /// </summary>
        /// <param name="guid"></param>
        public static void Remove(Guid guid)
        {
            if (hashtable.ContainsKey(guid))
            {
                hashtable.Remove(guid);
            }
        }
    }
}
