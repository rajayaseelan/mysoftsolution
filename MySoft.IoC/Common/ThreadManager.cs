using System;
using System.Collections;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 线程管理
    /// </summary>
    internal class ThreadManager
    {
        private ICacheStrategy cache;
        private Func<IService, OperationContext, RequestMessage, ResponseMessage> func;

        //实例化队列
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化线程管理器
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="func"></param>
        public ThreadManager(ICacheStrategy cache, Func<IService, OperationContext, RequestMessage, ResponseMessage> func)
        {
            this.cache = cache;
            this.func = func;
        }

        /// <summary>
        /// 添加工作项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        public void AddWorker(string key, WorkerItem item)
        {
            if (!hashtable.ContainsKey(key))
            {
                //将当前线程放入队列中
                hashtable[key] = item;

                IoCHelper.WriteLine(ConsoleColor.Blue, "[{0}] => Worker item count: {1}.", DateTime.Now, hashtable.Count);
            }
        }

        /// <summary>
        /// 刷新工作项
        /// </summary>
        /// <param name="key"></param>
        public void RefreshWorker(string key)
        {
            if (hashtable.ContainsKey(key))
            {
                //将当前线程放入队列中
                var worker = hashtable[key] as WorkerItem;

                lock (worker)
                {
                    if (worker.IsRunning) return;

                    if (DateTime.Now.Subtract(worker.UpdateTime).TotalSeconds >= 0)
                    {
                        //正在运行
                        worker.IsRunning = true;

                        ThreadPool.QueueUserWorkItem(UpdateData, worker);
                    }
                }
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="state"></param>
        private void UpdateData(object state)
        {
            var worker = state as WorkerItem;

            try
            {
                var resMsg = func(worker.Service, worker.Context, worker.Request);

                //不为null而且未出错
                if (resMsg != null && !resMsg.IsError)
                {
                    cache.InsertCache(worker.CallKey, resMsg, worker.SlidingTime * 10);

                    IoCHelper.WriteLine(ConsoleColor.Green, "[{0}] => Refresh worker item: {1}.", DateTime.Now, worker.CallKey);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }
            finally
            {
                //下次更新时间
                worker.UpdateTime = DateTime.Now.AddSeconds(worker.SlidingTime);

                //重置运行状态为false
                worker.IsRunning = false;
            }
        }
    }
}