using System;
using System.Linq;
using System.Collections.Generic;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// AOP工厂类
    /// </summary>
    public static class AspectFactory
    {
        /// <summary>
        /// 创建一个实例方式的拦截器（支持Aspect方式）
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static object CreateProxyService(Type serviceType, object target)
        {
            var cacheKey = string.Format("AspectCache_{0}", serviceType);
            var interceptors = CacheHelper.Get<List<Castle.DynamicProxy.IInterceptor>>(cacheKey);

            if (interceptors == null)
            {
                interceptors = new List<Castle.DynamicProxy.IInterceptor>();
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
                                if (attribute.Arguments.GetType().IsClass)
                                {
                                    var arg = Activator.CreateInstance(attribute.Arguments.GetType());
                                    value = Activator.CreateInstance(attribute.InterceptorType, arg);
                                }
                                else
                                    value = Activator.CreateInstance(attribute.InterceptorType, attribute.Arguments);
                            }

                            interceptors.Add(value as Castle.DynamicProxy.IInterceptor);
                        }
                    }

                    //将拦截器进行缓存
                    CacheHelper.Permanent(cacheKey, interceptors);
                }
            }

            if (interceptors.Count > 0)
                return AspectFactory.CreateProxy(serviceType, target, interceptors.ToArray());
            else
                return target;
        }

        /// <summary>
        /// 创建一个实例方式的拦截器
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="target"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type proxyType, object target, params Castle.DynamicProxy.IInterceptor[] interceptors)
        {
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
            return CreateProxy(classType, null, interceptors);
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
