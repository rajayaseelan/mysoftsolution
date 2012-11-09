using System.Collections;
using MySoft.IoC.Communication.Scs.Server;

namespace MySoft.IoC.Services
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
        /// <param name="worker"></param>
        public static void Set(IScsServerClient channel, WorkerItem worker)
        {
            if (channel == null) return;
            var key = channel.ClientId;

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(key))
                {
                    //将当前线程放入队列中
                    hashtable[key] = worker;
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
                    var worker = hashtable[key] as WorkerItem;

                    //判断是否已经完成
                    if (!worker.IsCompleted)
                    {
                        try
                        {
                            worker.Cancel();
                        }
                        catch
                        {
                        }
                    }

                    hashtable.Remove(key);
                }
            }
        }
    }
}