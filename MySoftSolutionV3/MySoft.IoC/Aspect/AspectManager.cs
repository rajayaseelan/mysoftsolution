using System;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// Aspect代理管理器
    /// </summary>
    public static class AspectManager
    {
        /// <summary>
        /// 获取Aspect服务
        /// </summary>
        /// <typeparam name="IServiceType"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IServiceType GetService<IServiceType>(object service)
        {
            if (service == null)
            {
                return default(IServiceType);
            }

            return (IServiceType)GetService(service);
        }

        /// <summary>
        /// 获取Aspect服务
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static object GetService(object service)
        {
            if (service != null)
            {
                var aspect = CreateService(service.GetType());
                if (aspect != null) service = aspect;
            }

            return service;
        }

        /// <summary>
        /// 创建Aspect服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object CreateService(Type serviceType)
        {
            string aspectKey = string.Format("AspectManager_{0}", serviceType);
            var service = CacheHelper.Get(aspectKey);
            if (service == null)
            {
                var attributes = CoreHelper.GetTypeAttributes<AspectProxyAttribute>(serviceType);
                if (attributes != null && attributes.Length > 0)
                {
                    IList<object> list = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (typeof(AspectInterceptor).IsAssignableFrom(attribute.InterceptorType))
                        {
                            object value = null;
                            if (attribute.Arguments == null)
                                value = Activator.CreateInstance(attribute.InterceptorType);
                            else
                            {
                                if (attribute.Arguments.GetType().IsClass)
                                {
                                    var arg = Activator.CreateInstance(attribute.Arguments.GetType());
                                    value = Activator.CreateInstance(attribute.InterceptorType, arg);
                                }
                                else
                                    value = Activator.CreateInstance(attribute.InterceptorType, attribute.Arguments);
                            }

                            list.Add(value);
                        }
                    }

                    var interceptors = list.Cast<AspectInterceptor>();
                    service = AspectFactory.CreateProxy(serviceType, interceptors.ToArray());
                    CacheHelper.Insert(aspectKey, service, 60);
                }
            }

            return service;
        }
    }
}
