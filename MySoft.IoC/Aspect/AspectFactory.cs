using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// AOP工厂类
    /// </summary>
    public static class AspectFactory
    {
        private static IDictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>
        /// 创建一个实例方式的拦截器（支持Aspect方式）
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static object CreateProxyService(Type serviceType, object target)
        {
            lock (services)
            {
                if (services.ContainsKey(serviceType))
                {
                    return services[serviceType];
                }
                else
                {
                    var service = CreateService(serviceType, target);
                    services[serviceType] = service;

                    return service;
                }
            }
        }

        private static object CreateService(Type serviceType, object target)
        {
            var interceptors = new List<Castle.DynamicProxy.IInterceptor>();
            var classType = target.GetType();
            var attributes = CoreHelper.GetTypeAttributes<AspectProxyAttribute>(classType);

            if (attributes != null && attributes.Length > 0)
            {
                foreach (var attribute in attributes)
                {
                    if (typeof(Castle.DynamicProxy.IInterceptor).IsAssignableFrom(attribute.InterceptorType))
                    {
                        object value = null;
                        if (attribute.Arguments == null)
                            value = Activator.CreateInstance(attribute.InterceptorType);
                        else
                        {

                            var arguments = new List<object>();
                            foreach (var argument in attribute.Arguments)
                            {
                                if (argument == null)
                                {
                                    arguments.Add(argument);
                                    continue;
                                }

                                //如果类，则创建实例
                                if (argument.GetType().IsClass)
                                {
                                    var instance = Activator.CreateInstance(argument.GetType());
                                    arguments.Add(instance);
                                }
                                else
                                {
                                    arguments.Add(argument);
                                }
                            }

                            value = Activator.CreateInstance(attribute.InterceptorType, arguments.ToArray());
                        }

                        interceptors.Add(value as Castle.DynamicProxy.IInterceptor);
                    }
                }
            }

            if (interceptors.Count > 0)
            {
                //创建代理服务
                return CreateProxy(serviceType, target, interceptors.ToArray());
            }
            else
            {
                return target;
            }
        }

        /// <summary>
        /// 创建一个实例方式的拦截器
        /// </summary>
        /// <param name="proxyType"></param>
        /// <param name="target"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type proxyType, object target, params Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            //如果拦截器为0
            if (interceptors.Length == 0)
            {
                return target;
            }

            Castle.DynamicProxy.ProxyGenerator proxy = new Castle.DynamicProxy.ProxyGenerator();
            Castle.DynamicProxy.ProxyGenerationOptions options = new Castle.DynamicProxy.ProxyGenerationOptions(new ProxyGenerationHook())
            {
                Selector = new InterceptorSelector()
            };

            return proxy.CreateInterfaceProxyWithTargetInterface(proxyType, target, options, interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, params Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            return CreateProxy(classType, new object[0], interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器（可传入参数）
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="arguments"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, object[] arguments, params Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            //如果拦截器为0
            if (interceptors.Length == 0)
            {
                return Activator.CreateInstance(classType, arguments);
            }

            Castle.DynamicProxy.ProxyGenerator proxy = new Castle.DynamicProxy.ProxyGenerator();
            Castle.DynamicProxy.ProxyGenerationOptions options = new Castle.DynamicProxy.ProxyGenerationOptions(new ProxyGenerationHook())
            {
                Selector = new InterceptorSelector()
            };

            if (arguments == null || arguments.Length == 0)
                return proxy.CreateClassProxy(classType, options, interceptors);
            else
                return proxy.CreateClassProxy(classType, options, arguments, interceptors);
        }
    }
}
