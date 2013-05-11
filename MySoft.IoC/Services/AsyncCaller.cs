using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Security;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        private IDictionary<string, QueueManager> hashtable;
        private Semaphore semaphore;
        private bool fromServer;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="fromServer"></param>
        /// <param name="maxCaller"></param>
        public AsyncCaller(bool fromServer, int maxCaller)
        {
            this.fromServer = fromServer;
            this.semaphore = new Semaphore(maxCaller, maxCaller);
            this.hashtable = new Dictionary<string, QueueManager>();
        }

        /// <summary>
        /// 同步调用服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseItem Run(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                //获取callerKey
                var callKey = GetCallerKey(reqMsg, context.Caller);

                QueueManager manager = null;

                lock (hashtable)
                {
                    if (!hashtable.ContainsKey(callKey))
                    {
                        hashtable[callKey] = new QueueManager();

                        //定义委托
                        Func<string, IService, OperationContext, RequestMessage, ResponseItem> func = null;

                        if (NeedServiceCache(reqMsg))
                        {
                            if (fromServer)
                                func = GetResponseFromFileCache;
                            else
                                func = GetResponseFromMemoryCache;
                        }
                        else
                        {
                            func = GetResponseFromLocalService;
                        }

                        //开始异步调用
                        func.BeginInvoke(callKey, service, context, reqMsg, AsyncCallback, new ArrayList { callKey, func });
                    }

                    manager = hashtable[callKey];
                }

                //返回响应消息
                return manager.GetMessage(reqMsg);
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 回调方法
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var arr = ar.AsyncState as ArrayList;

            //回调方法
            var _callKey = Convert.ToString(arr[0]);

            lock (hashtable)
            {
                try
                {
                    //获取管理Key
                    var manager = hashtable[_callKey] as QueueManager;

                    //转换成函数
                    var _func = arr[1] as Func<string, IService, OperationContext, RequestMessage, ResponseItem>;

                    //响应请求
                    manager.Set(_func.EndInvoke(ar));
                }
                finally
                {
                    //移除元素
                    hashtable.Remove(_callKey);
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            //设置上下文
            OperationContext.Current = context;

            try
            {
                //响应结果，清理资源
                return service.CallService(reqMsg);
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                //获取异常响应
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromLocalService(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //从本地获取数据
            var resMsg = GetResponse(service, context, reqMsg);

            //实例化ResponseItem
            return resMsg == null ? null : new ResponseItem(resMsg);
        }

        /// <summary>
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedServiceCache(RequestMessage reqMsg)
        {
            return reqMsg.EnableCache && reqMsg.CacheTime > 0;
        }

        /// <summary>
        /// 从本地获取数据
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromMemoryCache(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //本地缓存Key
            callKey = string.Format("memory_{0}", callKey);

            //开始一个记时器
            var watch = Stopwatch.StartNew();

            try
            {
                //计算缓存时间
                int timeout = Math.Min(30, reqMsg.CacheTime);

                //调用缓存处理结果
                var resMsg = CacheHelper<ResponseMessage>.Get(callKey, TimeSpan.FromSeconds(timeout), state =>
                {
                    //获取响应信息项
                    var arr = state as ArrayList;
                    var _service = arr[0] as IService;
                    var _context = arr[1] as OperationContext;
                    var _reqMsg = arr[2] as RequestMessage;

                    return GetResponse(_service, _context, _reqMsg);

                }, new ArrayList { service, context, reqMsg }, CheckResponse);

                if (resMsg != null)
                {
                    //设置耗时时间
                    resMsg.ElapsedTime = Math.Min(resMsg.ElapsedTime, watch.ElapsedMilliseconds);
                    resMsg.TransactionId = reqMsg.TransactionId;
                }

                return (resMsg == null) ? null : new ResponseItem(resMsg);
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromFileCache(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //双缓存保护获取方式
            var key = new CacheKey
            {
                UniqueId = callKey,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName.Substring(reqMsg.MethodName.IndexOf(' ') + 1)
            };

            return ServiceCacheHelper.Get(key, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
            {
                //获取响应信息项
                var arr = state as ArrayList;

                //回调方法
                var _callKey = Convert.ToString(arr[0]);
                var _service = arr[1] as IService;
                var _context = arr[2] as OperationContext;
                var _reqMsg = arr[3] as RequestMessage;

                //同步请求响应数据
                var item = GetResponseFromLocalService(_callKey, _service, _context, _reqMsg);

                if (item != null && CheckResponse(item.Message))
                {
                    var buffer = SerializationManager.SerializeBin(item.Message);
                    item.Buffer = CompressionManager.CompressGZip(buffer);
                }

                return item;

            }, new ArrayList { callKey, service, context, reqMsg });
        }

        /// <summary>
        /// 获取CallerKey
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(RequestMessage reqMsg, AppCaller caller)
        {
            //对Key进行组装
            var callKey = string.Format("{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);

            //返回加密Key
            callKey = MD5.HexHash(Encoding.Default.GetBytes(callKey.ToLower()));

            //如果是状态服务，则使用内部缓存
            if (reqMsg.InvokeMethod)
            {
                callKey = string.Format("invoke_{0}", callKey);
            }

            return callKey;
        }

        /// <summary>
        /// 检测响应是否有效
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool CheckResponse(ResponseMessage resMsg)
        {
            if (resMsg == null) return false;

            //如果符合条件，则缓存 
            if (!resMsg.IsError && resMsg.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}