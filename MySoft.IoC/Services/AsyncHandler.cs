using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Security;
using System;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步处理器
    /// </summary>
    internal class AsyncHandler
    {
        private IService service;
        private OperationContext context;
        private RequestMessage reqMsg;
        private bool serverCache;

        /// <summary>
        /// 实例化AsyncHandler
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="serverCache"></param>
        public AsyncHandler(IService service, OperationContext context, RequestMessage reqMsg, bool serverCache)
        {
            this.service = service;
            this.context = context;
            this.reqMsg = reqMsg;
            this.serverCache = serverCache;
        }

        /// <summary>
        /// 直接响应结果
        /// </summary>
        /// <returns></returns>
        public ResponseMessage DoTask()
        {
            if (NeedCacheResult(reqMsg))
                return GetResponseFromCache();
            else
                return GetResponseFromService();
        }

        /// <summary>
        /// 开始请求
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginDoTask(AsyncCallback callback, object state)
        {
            //定义委托
            var func = new Func<ResponseMessage>(DoTask);

            return func.BeginInvoke(callback, state);
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

                var func = @delegate as Func<ResponseMessage>;

                //异步回调
                return func.EndInvoke(ar);
            }
            catch (Exception ex)
            {
                return IoCHelper.GetResponse(reqMsg, ex);
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
        /// <returns></returns>
        private ResponseMessage GetResponseFromService()
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
                if (ex is ThreadAbortException)
                    Thread.ResetAbort();

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
        /// <returns></returns>
        private ResponseMessage GetResponseFromCache()
        {
            //获取cacheKey
            var cacheKey = GetCacheKey(context.Caller);

            //服务端采用文件缓存
            var type = serverCache ? LocalCacheType.File : LocalCacheType.Memory;

            //获取内存缓存
            var cacheMsg = CacheHelper<ResponseMessage>.Get(type, cacheKey, TimeSpan.FromSeconds(reqMsg.CacheTime), () =>
            {
                //同步请求响应数据
                var resMsg = GetResponseFromService();

                if (CheckResponse(resMsg))
                {
                    resMsg = new ResponseBuffer
                    {
                        TransactionId = reqMsg.TransactionId,
                        ServiceName = resMsg.ServiceName,
                        MethodName = resMsg.MethodName,
                        Parameters = resMsg.Parameters,
                        ElapsedTime = resMsg.ElapsedTime,
                        Error = resMsg.Error,
                        Buffer = IoCHelper.SerializeObject(resMsg.Value)
                    };
                }

                return resMsg;

            }, p => p is ResponseBuffer);

            //传输Id不同
            cacheMsg.TransactionId = reqMsg.TransactionId;

            return cacheMsg;
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
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCacheKey(AppCaller caller)
        {
            var cacheKey = caller.Parameters.ToLower();

            cacheKey = cacheKey.Replace("\r\n", "").Replace("\t", "").Replace(" ", "");

            //对Key进行组装
            return string.Join("_$$_", new[] { caller.ServiceName, caller.MethodName.Split(' ')[1], cacheKey });
        }
    }
}
