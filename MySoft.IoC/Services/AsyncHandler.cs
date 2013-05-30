using System;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Security;

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

        /// <summary>
        /// 实例化AsyncHandler
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public AsyncHandler(IService service, OperationContext context, RequestMessage reqMsg)
        {
            this.service = service;
            this.context = context;
            this.reqMsg = reqMsg;
        }

        /// <summary>
        /// 直接响应结果
        /// </summary>
        /// <returns></returns>
        public ResponseItem DoTask()
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
            Func<ResponseItem> func = null;

            //是否需要缓存
            if (NeedCacheResult(reqMsg))
                func = GetResponseFromCache;
            else
                func = GetResponseFromService;

            return func.BeginInvoke(callback, state);
        }

        /// <summary>
        /// 结束请求
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public ResponseItem EndDoTask(IAsyncResult ar)
        {
            try
            {
                //异步委托
                var @delegate = (ar as AsyncResult).AsyncDelegate;

                var func = @delegate as Func<ResponseItem>;

                //异步回调
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
        /// <returns></returns>
        private ResponseItem GetResponseFromService()
        {
            //设置上下文
            OperationContext.Current = context;

            try
            {
                //响应结果，清理资源
                var resMsg = service.CallService(reqMsg);

                //实例化ResponseItem
                return new ResponseItem(resMsg);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                    Thread.ResetAbort();

                var resMsg = IoCHelper.GetResponse(reqMsg, ex);

                //实例化ResponseItem
                return new ResponseItem(resMsg);
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
        private ResponseItem GetResponseFromCache()
        {
            var type = LocalCacheType.Memory;

            //参数为0的采用文件缓存
            if (reqMsg.Parameters.Count == 0)
            {
                type = LocalCacheType.File;
            }

            //获取callerKey
            var callKey = GetCallerKey(reqMsg, context.Caller);

            //获取内存缓存
            return CacheHelper<ResponseItem>.Get(type, callKey, TimeSpan.FromSeconds(reqMsg.CacheTime), () =>
            {
                //同步请求响应数据
                var item = GetResponseFromService();

                if (CheckResponse(item.Message))
                {
                    item.Buffer = IoCHelper.SerializeObject(item.Message);
                    item.Message = null;
                }

                return item;

            }, response => response.Count > 0);
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

            //如果符合条件，则缓存 
            if (!resMsg.IsError && resMsg.Count > 0)
            {
                return true;
            }

            return false;
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
