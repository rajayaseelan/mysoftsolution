using System;
using System.Collections;
using System.Collections.Generic;
using MySoft.IoC.Aspect;
using MySoft.IoC.Cache;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 本地拦截器
    /// </summary>
    public sealed class LocalInvocationHandler : IProxyInvocationHandler
    {
        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private Type serviceType;
        private IDictionary<string, int> cacheTimes;
        private IServiceCache cache;
        private string hostName;
        private string ipAddress;

        /// <summary>
        /// 实例化LocalInvocationHandler
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="serviceType"></param>
        /// <param name="cache"></param>
        public LocalInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, Type serviceType, IServiceCache cache)
        {
            this.config = config;
            this.container = container;
            this.serviceType = serviceType;
            this.cache = cache;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();

            this.cacheTimes = new Dictionary<string, int>();
            var methods = CoreHelper.GetMethodsFromType(serviceType);
            foreach (var method in methods)
            {
                var contract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
                if (contract != null && contract.CacheTime > 0)
                    cacheTimes[method.ToString()] = contract.CacheTime;
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private object InvokeMethod(System.Reflection.MethodInfo method, object[] parameters, string jsonString)
        {
            //容器实例对象
            object instance = null;

            try
            {
                //从容器中获取对象
                instance = container.Resolve(serviceType);

                //返回拦截服务
                var service = AspectFactory.CreateProxyService(serviceType, instance);

                //设置上下文
                SetOperationContext(serviceType.FullName, method.ToString(), jsonString);

                //动态调用方法
                return DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);
            }
            finally
            {
                //释放资源
                container.Release(instance);

                //初始化上下文
                OperationContext.Current = null;
            }

        }

        #region IProxyInvocationHandler 成员

        /// <summary>
        /// 实现方法调用
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, System.Reflection.MethodInfo method, object[] parameters)
        {
            //定义返回值
            object returnValue = null;

            var hashtable = ServiceConfig.CreateParameters(method, parameters);
            string cacheKey = ServiceConfig.GetCacheKey(serviceType, method, hashtable);
            var cacheValue = cache.Get<CacheObject>(cacheKey);

            //缓存无值
            if (cacheValue == null)
            {
                //调用方法
                var jsonString = hashtable.ToString();
                returnValue = InvokeMethod(method, parameters, jsonString);

                //如果需要缓存，则存入本地缓存
                if (returnValue != null && cacheTimes.ContainsKey(method.ToString()))
                {
                    int cacheTime = cacheTimes[method.ToString()];
                    cacheValue = new CacheObject
                    {
                        Value = returnValue,
                        Parameters = hashtable
                    };

                    cache.Insert(cacheKey, cacheValue, cacheTime);
                }
            }
            else
            {
                //处理返回值
                returnValue = cacheValue.Value;

                //处理参数
                ServiceConfig.SetParameterValue(method, parameters, cacheValue.Parameters);
            }

            //返回结果
            return returnValue;
        }

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        private void SetOperationContext(string serviceName, string methodName, string parameters)
        {
            //创建AppCaller对象
            var caller = new AppCaller
            {
                AppName = config.AppName,
                HostName = hostName,
                IPAddress = ipAddress,
                ServiceName = serviceName,
                MethodName = methodName,
                Parameters = parameters
            };

            //初始化上下文
            OperationContext.Current = new OperationContext
            {
                Container = container,
                Caller = caller
            };
        }

        #endregion
    }
}
