using System;
using System.Net;
using System.Runtime.Remoting.Messaging;
using MySoft.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 操作上下文对象
    /// </summary>
    public class OperationContext
    {
        /// <summary>
        /// 当前上下文对象
        /// </summary>
        public static OperationContext Current
        {
            get
            {
                return CallContext.HostContext as OperationContext;
            }
            internal set
            {
                CallContext.HostContext = value;
            }
        }

        /// <summary>
        /// 回调类型
        /// </summary>
        private Type callbackType;
        private IScsServerClient client;
        private AppCaller caller;
        private IServiceCache serviceCache;

        /// <summary>
        /// 服务缓存
        /// </summary>
        public IServiceCache ServiceCache
        {
            get { return serviceCache; }
            internal set { serviceCache = value; }
        }

        /// <summary>
        /// 调用者
        /// </summary>
        public AppCaller Caller
        {
            get { return caller; }
            internal set { caller = value; }
        }

        /// <summary>
        /// 远程节点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (client == null) return null;
                var endPoint = client.RemoteEndPoint as ScsTcpEndPoint;
                return new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort);
            }
        }

        internal OperationContext() { }

        internal OperationContext(IScsServerClient client, Type callbackType)
        {
            this.client = client;
            this.callbackType = callbackType;
        }

        #region 缓存处理

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="cacheKey"></param>
        public void RemoveCache<TService>(string cacheKey)
        {
            RemoveCache(string.Format("{0}_{1}", typeof(TService).FullName, cacheKey));
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        public void RemoveCache(string cacheKey)
        {
            if (serviceCache != null)
                serviceCache.RemoveCache(cacheKey);
            else
                CacheHelper.Remove(cacheKey);
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheObject"></param>
        /// <param name="timeSpan"></param>
        public void AddCache(string cacheKey, object cacheObject, TimeSpan timeSpan)
        {
            if (serviceCache != null)
                serviceCache.AddCache(cacheKey, cacheObject, (int)timeSpan.TotalSeconds);
            else
                CacheHelper.Insert(cacheKey, cacheObject, (int)timeSpan.TotalSeconds);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public T GetCache<T>(string cacheKey)
        {
            if (serviceCache != null)
                return serviceCache.GetCache<T>(cacheKey);
            else
                return CacheHelper.Get<T>(cacheKey);
        }

        #endregion

        /// <summary>
        /// 获取回调代理服务
        /// </summary>
        /// <typeparam name="ICallbackService"></typeparam>
        /// <returns></returns>
        public ICallbackService GetCallbackChannel<ICallbackService>()
        {
            if (callbackType == null || typeof(ICallbackService) != callbackType)
            {
                throw new IoCException("Please set the current of callback interface type!");
            }
            else
            {
                var callback = new CallbackInvocationHandler(callbackType, client, 60);
                var instance = ProxyFactory.GetInstance().Create(callback, typeof(ICallbackService), true);
                return (ICallbackService)instance;
            }
        }
    }
}
