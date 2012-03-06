using System;
using System.Collections;
using System.Collections.Generic;
using MySoft.IoC.Aspect;
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
        private string hostName;
        private string ipAddress;

        /// <summary>
        /// 实例化LocalInvocationHandler
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="serviceType"></param>
        public LocalInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, Type serviceType)
        {
            this.config = config;
            this.container = container;
            this.serviceType = serviceType;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();

            this.cacheTimes = new Dictionary<string, int>();
            var methods = CoreHelper.GetMethodsFromType(serviceType);
            foreach (var method in methods)
            {
                var contract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
                if (contract != null && contract.ServerCacheTime > 0)
                    cacheTimes[method.ToString()] = contract.ServerCacheTime;
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
            object returnValue = null;
            string cacheKey = string.Format("LocalCache_{0}_{1}", method, jsonString);
            var cacheValue = CacheHelper.Get<CacheObject>(cacheKey);

            //缓存无值
            if (cacheValue == null)
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
                    returnValue = DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);

                    //如果需要缓存，则存入本地缓存
                    if (cacheTimes.ContainsKey(method.ToString()))
                    {
                        int cacheTime = cacheTimes[method.ToString()];
                        cacheValue = new CacheObject
                        {
                            Value = returnValue,
                            Arguments = parameters
                        };

                        CacheHelper.Insert(cacheKey, cacheValue, cacheTime);
                    }
                }
                finally
                {
                    //释放资源
                    container.Release(instance);

                    //初始化上下文
                    OperationContext.Current = null;
                }
            }
            else
            {
                returnValue = cacheValue.Value;
                var index = 0;
                foreach (var p in method.GetParameters())
                {
                    if (p.ParameterType.IsByRef)
                        parameters[index] = cacheValue.Arguments[index];

                    index++;
                }
            }

            return returnValue;
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
            var hashtable = CreateHashtable(method, parameters);
            var jsonString = SerializationManager.SerializeJson(hashtable);

            //调用方法
            return InvokeMethod(method, parameters, jsonString);
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
                ServiceCache = container.ServiceCache,
                Container = container,
                Caller = caller
            };
        }

        /// <summary>
        /// 创建一个Hashtable
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Hashtable CreateHashtable(System.Reflection.MethodInfo method, object[] parameters)
        {
            var hashtable = new Hashtable();
            int index = 0;
            foreach (var p in method.GetParameters())
            {
                hashtable[p.Name] = parameters[index];
                index++;
            }

            return hashtable;
        }

        #endregion

        private class CacheObject
        {
            /// <summary>
            /// 值
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// 参数
            /// </summary>
            public object[] Arguments { get; set; }
        }
    }
}
