using System.Collections.Generic;
using System.Text;
using System.Threading;
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
        private bool isAsync;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="isAsync"></param>
        /// <param name="maxCaller"></param>
        public AsyncCaller(bool isAsync, int maxCaller)
        {
            this.isAsync = isAsync;
            this.semaphore = new Semaphore(maxCaller, maxCaller);
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
                //获取callerKey
                var callKey = GetCallerKey(reqMsg, context.Caller);

                return InvokeResponse(callKey, service, context, reqMsg);
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 响应信息
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem InvokeResponse(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //返回响应信息
            bool newStart;

            var manager = GetManager(callKey, out newStart);

            if (newStart)
            {
                //响应服务
                var item = GetResponse(callKey, manager, service, context, reqMsg);

                //设置响应消息
                manager.Set(item);

                return item;
            }

            //等待响应
            using (var channelResult = new ChannelResult(reqMsg))
            {
                manager.Add(channelResult);

                channelResult.WaitOne();

                //返回响应消息
                return channelResult.Message;
            }
        }

        /// <summary>
        /// 响应服务并返回队列请求
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="manager"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponse(string callKey, QueueManager manager, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //异步处理器
            var handler = new AsyncHandler(callKey, service, context, reqMsg);

            if (isAsync)
            {
                //开始异步请求
                var ar = handler.BeginInvoke(null);

                //等待响应
                ar.AsyncWaitHandle.WaitOne();

                //获取响应信息
                return handler.EndInvoke(ar);
            }
            else
            {
                //获取响应信息
                return handler.Invoke();
            }
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
                }

                if (hashtable[callKey].Count == 0)
                {
                    newStart = true;
                }

                return hashtable[callKey];
            }
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
    }
}