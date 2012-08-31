using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Messages;
using System.Collections;
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
            ThreadPool.QueueUserWorkItem(DoWorkerItem);
        }

        private static void DoWorkerItem(object state)
        {
            var timeSpan = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_RECORD_TIMEOUT);

            while (true)
            {
                //Sleep second
                Thread.Sleep(timeSpan);

                if (hashtable.Count == 0) continue;

                lock (hashtable)
                {
                    foreach (var kvp in hashtable)
                    {
                        if (kvp.Value.IsRunning) continue;
                        kvp.Value.IsRunning = true;

                        ManagedThreadPool.QueueUserWorkItem(obj =>
                        {
                            var arr = obj as ArrayList;
                            var callKey = arr[0] as string;
                            var worker = arr[1] as WorkerItem;

                            try
                            {
                                var resMsg = GetResponse(worker.Service, worker.Context, worker.Request);

                                //不为null而且未出错
                                if (resMsg != null && !resMsg.IsError)
                                {
                                    CacheHelper.Permanent(callKey, resMsg);
                                }

                                worker.IsRunning = false;
                            }
                            catch (Exception ex)
                            {
                                //no exception
                            }
                        }, new ArrayList { kvp.Key, kvp.Value });
                    }
                }
            }
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
    }
}
