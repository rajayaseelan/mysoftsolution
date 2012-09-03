using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 线程管理
    /// </summary>
    internal static class ThreadManager
    {
        //实例化队列
        private static IDictionary<string, WorkerItem> hashtable = new Dictionary<string, WorkerItem>();

        static ThreadManager()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true)
                {
                    //1分钟检测一次
                    var timeSpan = TimeSpan.FromMinutes(1);
                    Thread.Sleep(timeSpan);

                    var list = new List<string>();
                    lock (hashtable)
                    {
                        if (hashtable.Count == 0) continue;
                        list = new List<string>(hashtable.Keys);
                    }

                    //清理项
                    foreach (var key in list)
                    {
                        lock (hashtable)
                        {
                            if (hashtable.ContainsKey(key))
                            {
                                var item = hashtable[key];
                                timeSpan = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_DATA_CACHE_TIME);

                                //如果超过指定时间，则移除项
                                if (DateTime.Now.Subtract(item.UpdateTime) > timeSpan)
                                {
                                    hashtable.Remove(key);
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 添加工作项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        public static void AddWorker(string key, WorkerItem item)
        {
            lock (hashtable)
            {
                if (hashtable.ContainsKey(key)) return;

                //将当前线程放入队列中
                hashtable[key] = item;
            }
        }

        /// <summary>
        /// 刷新工作项
        /// </summary>
        /// <param name="key"></param>
        public static void RefreshWorker(string key)
        {
            lock (hashtable)
            {
                if (hashtable.ContainsKey(key))
                {
                    var item = hashtable[key];

                    lock (item)
                    {
                        if (item.IsRunning) return;
                        item.IsRunning = true;

                        ManagedThreadPool.QueueUserWorkItem(RefreshData, new ArrayList { key, item });
                    }
                }
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="state"></param>
        private static void RefreshData(object state)
        {
            var arr = state as ArrayList;
            var callKey = arr[0] as string;
            var worker = arr[1] as WorkerItem;

            try
            {
                var resMsg = GetResponse(worker.Service, worker.Context, worker.Request);

                //不为null而且未出错
                if (resMsg != null && !resMsg.IsError)
                {
                    CacheHelper.Insert(callKey, resMsg, ServiceConfig.DEFAULT_DATA_CACHE_TIME);
                }
            }
            catch (Exception ex)
            {
                //no exception
            }
            finally
            {
                var timeSpan = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_SYNC_CACHE_TIME);
                Thread.Sleep(timeSpan);

                worker.UpdateTime = DateTime.Now;
                worker.IsRunning = false;
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public static ResponseMessage GetResponse(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            try
            {
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }
    }

    /// <summary>
    /// Worker item
    /// </summary>
    internal class WorkerItem
    {
        /// <summary>
        /// Service
        /// </summary>
        public IService Service { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContext Context { get; set; }

        /// <summary>
        /// 是否在运行
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        public WorkerItem()
        {
            this.UpdateTime = DateTime.Now;
        }
    }
}
