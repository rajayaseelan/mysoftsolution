using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;

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
        public static IServiceType GetService<IServiceType>(object service, params StandardInterceptor[] interceptors)
        {
            if (service == null)
            {
                return default(IServiceType);
            }

            return (IServiceType)GetService(service, interceptors);
        }

        /// <summary>
        /// 获取Aspect服务
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static object GetService(object service, params StandardInterceptor[] interceptors)
        {
            if (service != null)
            {
                var aspect = CreateService(service.GetType(), interceptors);
                if (aspect != null) service = aspect;
            }

            return service;
        }

        /// <summary>
        /// 创建Aspect服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object CreateService(Type serviceType, params StandardInterceptor[] interceptors)
        {
            object service = null;
            var attributes = CoreHelper.GetTypeAttributes<AspectProxyAttribute>(serviceType);
            if (attributes != null && attributes.Length > 0)
            {
                IList<object> list = new List<object>();
                foreach (var attribute in attributes)
                {
                    if (typeof(StandardInterceptor).IsAssignableFrom(attribute.InterceptorType))
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

                var interceptorlist = new List<StandardInterceptor>();
                interceptorlist.AddRange(interceptors);
                interceptorlist.AddRange(list.Cast<StandardInterceptor>());

                service = AspectFactory.CreateProxy(serviceType, interceptorlist.ToArray());
            }
            else if (interceptors != null && interceptors.Length > 0)
            {
                service = AspectFactory.CreateProxy(serviceType, interceptors.ToArray());
            }

            return service;
        }
    }
}
