using System;
using System.Collections.Generic;
using Castle.DynamicProxy;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// AOP工厂类
    /// </summary>
    public static class AspectFactory
    {
        /// <summary>
        /// 获取拦截器列表
        /// </summary>
        /// <param name="classType"></param>
        internal static IList<Type> GetInterceptors(Type classType)
        {
            var interceptors = new List<Type>();
            var attributes = CoreHelper.GetMemberAttributes<AspectProxyAttribute>(classType);

            if (attributes != null && attributes.Length > 0)
            {
                foreach (var attribute in attributes)
                {
                    if (typeof(IInterceptor).IsAssignableFrom(attribute.InterceptorType))
                    {
                        interceptors.Add(attribute.InterceptorType);
                    }
                }
            }

            return interceptors;
        }

        /// <summary>
        /// 创建一个实例方式的拦截器
        /// </summary>
        /// <param name="proxyType"></param>
        /// <param name="target"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type proxyType, object target, params IInterceptor[] interceptors)
        {
            return CreateProxy(proxyType, target, null, interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, params IInterceptor[] interceptors)
        {
            return CreateProxy(classType, (ProxyGenerationOptions)null, interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器（可传入参数）
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="arguments"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, object[] arguments, params IInterceptor[] interceptors)
        {
            return CreateProxy(classType, arguments, null, interceptors);
        }

        #region 带options参数

        /// <summary>
        /// 创建一个实例方式的拦截器
        /// </summary>
        /// <param name="proxyType"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type proxyType, object target, ProxyGenerationOptions options, params IInterceptor[] interceptors)
        {
            //如果拦截器为0
            if (interceptors == null || interceptors.Length == 0)
            {
                return target;
            }

            ProxyGenerator proxy = new ProxyGenerator();

            if (options == null)
                return proxy.CreateInterfaceProxyWithTarget(proxyType, target, interceptors);
            else
                return proxy.CreateInterfaceProxyWithTarget(proxyType, target, options, interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="options"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, ProxyGenerationOptions options, params IInterceptor[] interceptors)
        {
            return CreateProxy(classType, new object[0], null, interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器（可传入参数）
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="arguments"></param>
        /// <param name="options"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, object[] arguments, ProxyGenerationOptions options, params IInterceptor[] interceptors)
        {
            //如果拦截器为0
            if (interceptors.Length == 0)
            {
                return Activator.CreateInstance(classType, arguments);
            }

            ProxyGenerator proxy = new ProxyGenerator();

            if (options == null)
            {
                if (arguments == null || arguments.Length == 0)
                    return proxy.CreateClassProxy(classType, interceptors);
                else
                    return proxy.CreateClassProxy(classType, arguments, interceptors);
            }
            else
            {
                if (arguments == null || arguments.Length == 0)
                    return proxy.CreateClassProxy(classType, options, interceptors);
                else
                    return proxy.CreateClassProxy(classType, options, arguments, interceptors);
            }
        }

        #endregion

        /// <summary>
        /// 创建服务代理
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <param name="service"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static IServiceInterfaceType CreateProxy<IServiceInterfaceType>(IServiceInterfaceType service, params IInterceptor[] interceptors)
            where IServiceInterfaceType : class
        {
            return CreateProxy<IServiceInterfaceType>(service, ProxyGenerationOptions.Default, interceptors);
        }

        /// <summary>
        /// 创建服务代理
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <param name="service"></param>
        /// <param name="options"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static IServiceInterfaceType CreateProxy<IServiceInterfaceType>(IServiceInterfaceType service, ProxyGenerationOptions options, params IInterceptor[] interceptors)
            where IServiceInterfaceType : class
        {
            ProxyGenerator proxy = new ProxyGenerator();
            return proxy.CreateInterfaceProxyWithTarget<IServiceInterfaceType>(service, options, interceptors);
        }
    }
}