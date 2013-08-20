using MySoft.Cache;
using MySoft.IoC.Messages;
using System;
using System.Collections;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 同步调用器
    /// </summary>
    internal class SyncCaller : IDisposable
    {
        private IService service;

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="service"></param>
        public SyncCaller(IService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public virtual ResponseMessage Invoke(OperationContext context, RequestMessage reqMsg)
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

            //参数为0，文件缓存
            var type = reqMsg.Parameters.Count == 0 ? LocalCacheType.File : LocalCacheType.Memory;

            //获取内存缓存
            var resMsg = CacheHelper<ResponseMessage>.Get(type, cacheKey, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
            {
                var arr = state as ArrayList;
                var _context = arr[0] as OperationContext;
                var _reqMsg = arr[1] as RequestMessage;

                //获取响应数据
                return GetResponse(_context, _reqMsg);

            }, new ArrayList { context, reqMsg }, p => p is ResponseBuffer);

            //临时缓存处理，减小压力
            if (resMsg != null && resMsg.Count == 0)
            {
                //如果数据为0，则缓存30秒
                CacheHelper.Insert(cacheKey, resMsg, 30);
            }

            return resMsg;
        }

        /// <summary>
        /// 获取响应数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            //同步请求响应数据
            var resMsg = GetResponseFromService(context, reqMsg);

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
        }

        /// <summary>
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedCacheResult(RequestMessage reqMsg)
        {
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
        /// 清理资源
        /// </summary>
        public virtual void Dispose()
        {
            //TODO
        }
    }
}
