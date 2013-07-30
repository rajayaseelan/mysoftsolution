using MySoft.Cache;
using MySoft.IoC.Messages;
using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步方法调用
    /// </summary>
    /// <param name="context"></param>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncMethodCaller(OperationContext context, RequestMessage reqMsg);

    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : IDisposable
    {
        private IService service;
        private ServiceCacheType type;
        private AsyncMethodCaller caller;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="type"></param>
        public AsyncCaller(IService service, ServiceCacheType type)
        {
            this.service = service;
            this.type = type;
            this.caller = new AsyncMethodCaller(SyncRun);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ResponseMessage AsyncRun(OperationContext context, RequestMessage reqMsg, TimeSpan timeout)
        {
            using (var waitResult = new WaitResult(reqMsg))
            {
                //异步请求
                caller.BeginInvoke(context, reqMsg, AsyncCallback, waitResult);

                //超时返回
                if (!waitResult.WaitOne(timeout))
                {
                    throw new TimeoutException(string.Format("The current request timeout {0} ms!", timeout.TotalMilliseconds));
                }

                return waitResult.Message;
            }
        }

        /// <summary>
        /// 异步回调
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var waitResult = ar.AsyncState as WaitResult;

            try
            {
                var @delegate = (ar as AsyncResult).AsyncDelegate;
                var func = @delegate as AsyncMethodCaller;

                //异步响应
                var resMsg = func.EndInvoke(ar);

                waitResult.Set(resMsg);
            }
            catch (Exception ex)
            {
                waitResult.Set(ex);
            }
            finally
            {
                //释放资源
                ar.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage SyncRun(OperationContext context, RequestMessage reqMsg)
        {
            if (NeedCacheResult(reqMsg))
                return GetResponseFromCache(context, reqMsg);
            else
                return GetResponseFromService(context, reqMsg);
        }

        #region 私有方法

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseFromService(OperationContext context, RequestMessage reqMsg)
        {
            //设置上下文
            OperationContext.Current = context;

            try
            {
                //响应结果，清理资源
                return service.CallService(reqMsg);
            }
            catch (ThreadAbortException ex)
            {
                //取消中止线程
                Thread.ResetAbort();

                throw new ThreadStateException("The current request thread is aborted!", ex);
            }
            finally
            {
                OperationContext.Current = null;
            }
        }

        /// <summary>
        /// 从内存获取数据项
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseFromCache(OperationContext context, RequestMessage reqMsg)
        {
            //获取cacheKey
            var cacheKey = GetCacheKey(reqMsg, context.Caller);

            //转换成对应的缓存类型
            var cacheType = type == ServiceCacheType.Memory ? LocalCacheType.Memory : LocalCacheType.File;

            //获取内存缓存
            return CacheHelper<ResponseMessage>.Get(cacheType, cacheKey, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
            {
                var arr = state as ArrayList;
                var _context = arr[0] as OperationContext;
                var _reqMsg = arr[1] as RequestMessage;

                //同步请求响应数据
                var resMsg = GetResponseFromService(_context, _reqMsg);

                if (CheckResponse(resMsg))
                {
                    resMsg = new ResponseBuffer
                    {
                        ServiceName = resMsg.ServiceName,
                        MethodName = resMsg.MethodName,
                        Parameters = resMsg.Parameters,
                        ElapsedTime = resMsg.ElapsedTime,
                        Error = resMsg.Error,
                        Buffer = IoCHelper.SerializeObject(resMsg.Value)
                    };
                }

                return resMsg;

            }, new ArrayList { context, reqMsg }, p => p is ResponseBuffer);
        }

        /// <summary>
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedCacheResult(RequestMessage reqMsg)
        {
            if (type == ServiceCacheType.None) return false;

            return reqMsg.EnableCache && reqMsg.CacheTime > 0;
        }

        /// <summary>
        /// 检测响应是否有效
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool CheckResponse(ResponseMessage resMsg)
        {
            if (resMsg == null) return false;
            if (resMsg is ResponseBuffer) return false;

            //如果符合条件，则缓存 
            return !resMsg.IsError && resMsg.Count > 0;
        }

        /// <summary>
        /// 获取cacheKey
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="appCaller"></param>
        /// <returns></returns>
        private string GetCacheKey(RequestMessage reqMsg, AppCaller appCaller)
        {
            var cacheKey = appCaller.Parameters.ToLower();
            cacheKey = cacheKey.Replace("\r\n", "").Replace("\t", "").Replace(" ", "");

            //对Key进行组装
            var methodName = appCaller.MethodName.Substring(appCaller.MethodName.IndexOf(' ') + 1);

            if (reqMsg.InvokeMethod)
                return string.Join("_$$_", new[] { "invoke", appCaller.ServiceName, methodName, cacheKey });
            else
                return string.Join("_$$_", new[] { appCaller.ServiceName, methodName, cacheKey });
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.caller = null;
        }
    }
}