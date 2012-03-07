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
        private EndPoint endPoint;
        private IContainer container;
        private ICache cache;
        private AppCaller caller;

        /// <summary>
        /// 服务缓存
        /// </summary>
        public ICache Cache
        {
            get { return cache; }
            internal set { cache = value; }
        }

        /// <summary>
        /// 容器对象
        /// </summary>
        public IContainer Container
        {
            get { return container; }
            internal set { container = value; }
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
                if (client == null)
                    return null;
                else
                    return endPoint;
            }
        }

        internal OperationContext() { }

        internal OperationContext(IScsServerClient client, Type callbackType)
        {
            this.client = client;
            this.callbackType = callbackType;

            if (client != null)
            {
                var ep = client.RemoteEndPoint as ScsTcpEndPoint;
                this.endPoint = new IPEndPoint(IPAddress.Parse(ep.IpAddress), ep.TcpPort);
            }
        }

        #region 缓存处理

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        public void RemoveCache(string cacheKey)
        {
            if (cache != null)
                cache.Remove(cacheKey);
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
            if (cache != null)
                cache.Insert(cacheKey, cacheObject, (int)timeSpan.TotalSeconds);
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
            if (cache != null)
                return cache.Get<T>(cacheKey);
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
