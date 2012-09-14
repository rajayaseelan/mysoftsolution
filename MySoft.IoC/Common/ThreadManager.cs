using System;
using System.Collections;
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
        private Func<IService, OperationContext, RequestMessage, ResponseMessage> caller;

        //实例化队列
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化线程管理器
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="caller"></param>
        public ThreadManager(ICacheStrategy cache, Func<IService, OperationContext, RequestMessage, ResponseMessage> caller)
        {
            this.cache = cache;
            this.caller = caller;
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

                        //开始调用服务
                        IoCHelper.WriteLine(ConsoleColor.Green, "[{0}] => Begin refresh worker item: {1}.", DateTime.Now, worker.CallKey);

                        //异步调用服务
                        caller.BeginInvoke(worker.Service, worker.Context, worker.Request, AsyncCallback, worker);
                    }
                }
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var worker = ar.AsyncState as WorkerItem;

            try
            {
                var resMsg = caller.EndInvoke(ar);
                ar.AsyncWaitHandle.Close();

                //不为null而且未出错
                if (resMsg != null && !resMsg.IsError && resMsg.Count > 0)
                {
                    cache.InsertCache(worker.CallKey, resMsg, worker.SlidingTime * 10);

                    //结束调用服务
                    IoCHelper.WriteLine(ConsoleColor.Green, "[{0}] => End refresh worker item: {1}, Elapsed time {2} ms.", DateTime.Now, worker.CallKey, resMsg.ElapsedTime);
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