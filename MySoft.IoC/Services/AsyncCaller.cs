using System;
using System.Collections;
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

                //异步处理器
                var handler = new AsyncHandler(callKey, service, context, reqMsg);

                //同步响应
                if (!isAsync) return handler.Invoke();

                using (var channelResult = new ChannelResult(reqMsg))
                {
                    //Invoke响应
                    handler.BeginInvoke(WaitCallback, new ArrayList { handler, channelResult });

                    //超时返回
                    if (!channelResult.WaitOne(timeout))
                    {
                        return GetTimeoutResponse(reqMsg);
                    }

                    return channelResult.Message;
                }
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 异步返回结果
        /// </summary>
        /// <param name="ar"></param>
        private void WaitCallback(IAsyncResult ar)
        {
            if (ar.AsyncState == null) return;

            var arr = ar.AsyncState as ArrayList;

            try
            {
                var handler = arr[0] as AsyncHandler;
                var channelResult = arr[1] as ChannelResult;

                //返回响应
                var item = handler.EndInvoke(ar);

                channelResult.Set(item);
            }
            catch (Exception ex)
            {
                //TODO
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