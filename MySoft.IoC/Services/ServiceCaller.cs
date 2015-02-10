using MySoft.Cache;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用委托
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncCaller(MessageItem item);

    /// <summary>
    /// 服务调用者
    /// </summary>
    internal class ServiceCaller : IDisposable
    {
        private readonly IServiceContainer container;
        private IDictionary<string, Type> callbackTypes;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="container"></param>
        public ServiceCaller(IServiceContainer container)
        {
            this.container = container;

            this.callbackTypes = new Dictionary<string, Type>();

            //初始化服务
            InitTypes(container);
        }

        private void InitTypes(IServiceContainer container)
        {
            callbackTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);
            var types = container.GetServiceTypes<ServiceContractAttribute>();

            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="appCaller"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage HandleResponse(IScsServerClient channel, AppCaller appCaller, RequestMessage reqMsg)
        {
            string serviceKey = "Service_" + appCaller.ServiceName;

            if (!container.Kernel.HasComponent(serviceKey))
            {
                string body = string.Format("The server not find matching service ({0}).", appCaller.ServiceName);

                //获取异常
                throw IoCHelper.GetException(appCaller, body);
            }

            //获取上下文
            var context = GetOperationContext(channel, appCaller);

            //同步调用服
            return GetResponseFromCache(context, reqMsg);
        }

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private OperationContext GetOperationContext(IScsServerClient channel, AppCaller caller)
        {
            //实例化当前上下文
            Type callbackType = null;

            if (callbackTypes.ContainsKey(caller.ServiceName))
            {
                callbackType = callbackTypes[caller.ServiceName];
            }

            return new OperationContext(channel, callbackType)
            {
                Container = container,
                Caller = caller
            };
        }

        #region 私有方法

        /// <summary>
        /// 从内存获取数据项
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseFromCache(OperationContext context, RequestMessage reqMsg)
        {
            //如果不需要缓存,直接响应服务
            if (!NeedCacheResult(reqMsg))
            {
                return GetResponseFromService(context, reqMsg);
            }

            //获取cacheKey
            var cacheKey = GetCacheKey(reqMsg, context.Caller);

            //获取内存缓存
            return CacheHelper<ResponseMessage>.Get(LocalCacheType.Memory, cacheKey, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
            {
                if (state == null) return null;

                //解析上下文
                var arr = state as ArrayList;
                var _context = arr[0] as OperationContext;
                var _reqMsg = arr[1] as RequestMessage;

                //同步请求响应数据
                var response = GetResponseFromService(_context, _reqMsg);

                if (!(response is ResponseBuffer) && !response.IsError && response.Count > 0)
                {
                    response = new ResponseBuffer(response);
                }

                return response;

            }, new ArrayList { context, reqMsg }, p => p is ResponseBuffer);
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
                string serviceKey = "Service_" + reqMsg.ServiceName;

                //解析服务
                IService service = container.Resolve<IService>(serviceKey);

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
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedCacheResult(RequestMessage reqMsg)
        {
            return reqMsg.EnableCache && reqMsg.CacheTime > 0;
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

        #region IDisposable 成员

        public void Dispose()
        {
            callbackTypes.Clear();
        }

        #endregion
    }
}
