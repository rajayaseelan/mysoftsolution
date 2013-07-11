using MySoft.Cache;
using MySoft.IoC.Messages;
using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步处理器
    /// </summary>
    internal class AsyncHandler
    {
        private IService service;
        private ServiceCacheType type;

        /// <summary>
        /// 实例化AsyncHandler
        /// </summary>
        /// <param name="service"></param>
        /// <param name="type"></param>
        public AsyncHandler(IService service, ServiceCacheType type)
        {
            this.service = service;
            this.type = type;
        }

        /// <summary>
        /// 直接响应结果
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage DoTask(OperationContext context, RequestMessage reqMsg)
        {
            if (NeedCacheResult(reqMsg))
                return GetResponseFromCache(context, reqMsg);
            else
                return GetResponseFromService(context, reqMsg);
        }

        /// <summary>
        /// 开始请求
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginDoTask(OperationContext context, RequestMessage reqMsg, AsyncCallback callback, object state)
        {
            //定义委托
            var func = new Func<OperationContext, RequestMessage, ResponseMessage>(DoTask);

            return func.BeginInvoke(context, reqMsg, callback, state);
        }

        /// <summary>
        /// 结束请求
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public ResponseMessage EndDoTask(IAsyncResult ar)
        {
            try
            {
                //异步委托
                var @delegate = (ar as AsyncResult).AsyncDelegate;

                //异步回调
                var func = @delegate as Func<OperationContext, RequestMessage, ResponseMessage>;

                return func.EndInvoke(ar);
            }
            finally
            {
                //释放资源，必写
                ar.AsyncWaitHandle.Close();
            }
        }

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
            catch (Exception ex)
            {
                //如果是线程异常
                if (ex is ThreadAbortException)
                {
                    Thread.ResetAbort();
                    ex = new Exception("The current request thread is interrupted!", ex);
                }

                return IoCHelper.GetResponse(reqMsg, ex);
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
            var cacheKey = GetCacheKey(context.Caller);

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
        /// <param name="appCaller"></param>
        /// <returns></returns>
        private string GetCacheKey(AppCaller appCaller)
        {
            var cacheKey = appCaller.Parameters.ToLower();

            cacheKey = cacheKey.Replace("\r\n", "").Replace("\t", "").Replace(" ", "");

            //对Key进行组装
            var methodName = appCaller.MethodName.Substring(appCaller.MethodName.IndexOf(' ') + 1);

            return string.Join("_$$_", new[] { appCaller.ServiceName, methodName, cacheKey });
        }
    }
}
