using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        private IDictionary<string, QueueManager> hashtable;
        private Semaphore semaphore;
        private bool isAsync;
        private TimeSpan timeout;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="isAsync"></param>
        /// <param name="maxCaller"></param>
        public AsyncCaller(bool isAsync, int maxCaller)
        {
            this.isAsync = isAsync;
            this.semaphore = new Semaphore(maxCaller, maxCaller);
            this.timeout = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_SERVER_TIMEOUT);

            this.hashtable = new Dictionary<string, QueueManager>();
        }

        /// <summary>
        /// 异步调用服务
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
                //异步处理器
                var handler = new AsyncHandler(service, context, reqMsg);

                if (isAsync)
                {
                    //异步响应
                    return GetResponse(handler, context.Caller, reqMsg);
                }
                else
                {
                    //同步响应
                    return handler.DoTask();
                }
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="caller"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponse(AsyncHandler handler, AppCaller caller, RequestMessage reqMsg)
        {
            bool newStart = false;

            //获取调用Key
            var callerKey = GetCallerKey(reqMsg, caller);

            var manager = GetManager(callerKey, out newStart);

            if (newStart)
            {
                //获取异步响应
                var item = InvokeResponse(handler, reqMsg);

                lock (hashtable)
                {
                    hashtable.Remove(callerKey);
                }

                manager.Set(item);

                return item;
            }

            //等待响应
            using (var channelResult = new ChannelResult(reqMsg))
            {
                if (!channelResult.WaitOne(timeout))
                {
                    return GetTimeoutResponse(reqMsg);
                }

                return channelResult.Message;
            }
        }

        /// <summary>
        /// 获取异步响应
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem InvokeResponse(AsyncHandler handler, RequestMessage reqMsg)
        {
            //Invoke响应
            var ar = handler.BeginDoTask(null, null);

            //超时返回
            if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
            {
                return GetTimeoutResponse(reqMsg);
            }

            return handler.EndDoTask(ar);
        }

        /// <summary>
        /// 获取管理器
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="newStart"></param>
        /// <returns></returns>
        private QueueManager GetManager(string callKey, out bool newStart)
        {
            //是否异步调用变量
            newStart = false;

            lock (hashtable)
            {
                if (!hashtable.ContainsKey(callKey))
                {
                    hashtable[callKey] = new QueueManager();

                    newStart = true;
                }

                return hashtable[callKey];
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetTimeoutResponse(RequestMessage reqMsg)
        {
            var title = string.Format("Remote invoke service ({0}, {1}) timeout ({2}) ms.", reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            //获取异常
            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));

            return new ResponseItem(resMsg);
        }

        /// <summary>
        /// 获取调用Key
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(RequestMessage reqMsg, AppCaller caller)
        {
            var callerKey = string.Format("{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);
            if (reqMsg.InvokeMethod)
            {
                callerKey = string.Format("invoke_{0}", callerKey);
            }

            return callerKey;
        }
    }
}