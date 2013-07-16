using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.IoC.Services.Tasks;
using MySoft.Threading;
using System;
using System.Collections;
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
            var ar = new AsyncResult<ResponseMessage>(callback, state);

            //开始线程处理
            ManagedThreadPool.QueueUserWorkItem(DoTaskOnAsync, new ArrayList { ar, context, reqMsg });

            return ar;
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="state"></param>
        private void DoTaskOnAsync(object state)
        {
            var arr = state as ArrayList;

            var _ar = arr[0] as AsyncResult<ResponseMessage>;

            var _context = arr[1] as OperationContext;
            var _reqMsg = arr[2] as RequestMessage;

            try
            {
                _ar.CurrentThread = Thread.CurrentThread;

                //同步执行
                var resMsg = DoTask(_context, _reqMsg);

                _ar.SetAsCompleted(resMsg, false);
            }
            catch (Exception ex)
            {
                _ar.SetAsCompleted(ex, false);
            }
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="ar"></param>
        public void CancelTask(IAsyncResult ar)
        {
            try
            {
                //异步委托
                var _ar = ar as AsyncResult<ResponseMessage>;

                //结束线程
                if (_ar.CurrentThread != null)
                {
                    AbortThread(_ar.CurrentThread);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="t"></param>
        private void AbortThread(Thread t)
        {
            var state = t.ThreadState & (ThreadState.Unstarted |
                                    ThreadState.WaitSleepJoin |
                                    ThreadState.Stopped);

            if (state == ThreadState.Running)
            {
                t.Abort();
            }
            else if (state == ThreadState.WaitSleepJoin)
            {
                t.Interrupt();
            }
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
                var _ar = ar as AsyncResult<ResponseMessage>;

                //异步回调
                return _ar.EndInvoke();
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
            catch (ThreadInterruptedException ex)
            {
                throw new ThreadStateException("The current request thread is interrupted!", ex);
            }
            catch (ThreadAbortException ex)
            {
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
