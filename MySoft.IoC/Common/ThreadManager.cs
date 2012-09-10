using System;
using System.Collections;
using System.Linq;
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

            //定时更新数据
            ThreadPool.QueueUserWorkItem(DoRefreshData);
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="state"></param>
        private void DoRefreshData(object state)
        {
            while (true)
            {
                //1秒钟检测一次
                Thread.Sleep(TimeSpan.FromSeconds(1));

                try
                {
                    if (hashtable.Keys.Count == 0) continue;

                    var keys = hashtable.Keys.Cast<string>().ToList();

                    foreach (var key in keys)
                    {
                        if (hashtable.ContainsKey(key))
                        {
                            var worker = hashtable[key] as WorkerItem;

                            try
                            {
                                if (DateTime.Now.Subtract(worker.UpdateTime).TotalSeconds >= 0)
                                {
                                    //自动刷新缓存
                                    if (worker.IsRefresh)
                                    {
                                        //最后更新时间
                                        worker.UpdateTime = DateTime.MaxValue;

                                        ThreadPool.QueueUserWorkItem(UpdateData, worker);
                                    }
                                    else
                                    {
                                        //失效时移除缓存
                                        cache.RemoveCache(key);

                                        //不刷新项进行移除
                                        RemoveWorker(key);

                                        IoCHelper.WriteLine(ConsoleColor.Red, "[{0}] => Remove worker item: {1}.", DateTime.Now, key);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //不处理的异常
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO
                }
            }
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

                if (worker.IsRefresh) return;

                worker.IsRefresh = true;
            }
        }

        /// <summary>
        /// 移除工作项
        /// </summary>
        /// <param name="key"></param>
        private void RemoveWorker(string key)
        {
            if (hashtable.ContainsKey(key))
            {
                hashtable.Remove(key);
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
                    cache.InsertCache(worker.CallKey, resMsg, worker.SlidingTime * 5);

                    IoCHelper.WriteLine(ConsoleColor.Green, "[{0}] => Refresh worker item: {1}.", DateTime.Now, worker.CallKey);
                }
            }
            finally
            {
                //下次更新时间
                worker.UpdateTime = DateTime.Now.AddSeconds(worker.SlidingTime);

                //重置刷新状态为false
                worker.IsRefresh = false;
            }
        }
    }
}