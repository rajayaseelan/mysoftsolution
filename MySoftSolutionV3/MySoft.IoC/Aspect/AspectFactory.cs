using System;
using Castle.DynamicProxy;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// AOP工厂类
    /// </summary>
    public static class AspectFactory
    {
        /// <summary>
        /// 创建一个实例方式的拦截器
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type serviceType, params AspectInterceptor[] interceptors)
        {
            string aspectKey = string.Format("AspectFactory_{0}", serviceType);
            var service = CacheHelper.Get(aspectKey);
            if (service == null)
            {
                ProxyGenerator proxy = new ProxyGenerator();
                ProxyGenerationOptions options = new ProxyGenerationOptions(new ProxyGenerationHook())
                {
                    Selector = new InterceptorSelector()
                };

                service = proxy.CreateClassProxy(serviceType, options, interceptors);
                CacheHelper.Insert(aspectKey, service, 60);
            }

            return service;
        }

        /// <summary>
        /// 创建一个实例方式的拦截器
        /// </summary>
        /// <typeparam name="TServiceType"></typeparam>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static TServiceType CreateProxy<TServiceType>(params AspectInterceptor[] interceptors)
            where TServiceType : class
        {
            return (TServiceType)CreateProxy(typeof(TServiceType));
        }

        /// <summary>
        /// 创建一个接口方式的拦截器
        /// </summary>
        /// <typeparam name="IServiceType"></typeparam>
        /// <typeparam name="TServiceType"></typeparam>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static IServiceType CreateProxy<IServiceType, TServiceType>(params AspectInterceptor[] interceptors)
            where TServiceType : class, IServiceType
        {
            return (IServiceType)CreateProxy<TServiceType>(interceptors);
        }
    }
}
