using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// AOP工厂类
    /// </summary>
    public static class AspectFactory
    {
        private static IDictionary<Type, object> hashtable = new Dictionary<Type, object>();

        /// <summary>
        /// 创建一个实例方式的拦截器（支持Aspect方式）
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object CreateProxy(Type serviceType, object target)
        {
            lock (hashtable)
            {
                if (!hashtable.ContainsKey(serviceType))
                {
                    var interceptors = GetInterceptorList(target);

                    //创建代理服务
                    var service = CreateProxy(serviceType, target, interceptors.ToArray());

                    hashtable[serviceType] = service;
                }
            }

            return hashtable[serviceType];
        }

        /// <summary>
        /// 获取拦截器列表
        /// </summary>
        /// <param name="target"></param>
        private static IList<IInterceptor> GetInterceptorList(object target)
        {
            var interceptors = new List<IInterceptor>();

            //判断对象是否为null
            if (target == null) return interceptors;

            var classType = target.GetType();
            var attributes = CoreHelper.GetTypeAttributes<AspectProxyAttribute>(classType);

            if (attributes != null && attributes.Length > 0)
            {
                foreach (var attribute in attributes)
                {
                    if (typeof(IInterceptor).IsAssignableFrom(attribute.InterceptorType))
                    {
                        object value = null;
                        if (attribute.Arguments == null)
                        {
                            value = Activator.CreateInstance(attribute.InterceptorType);
                        }
                        else
                        {
                            var arguments = new List<object>();
                            foreach (var argument in attribute.Arguments)
                            {
                                if (argument == null) continue;

                                //如果类，则创建实例
                                var type = argument.GetType();
                                if (type.IsClass)
                                {
                                    var instance = Activator.CreateInstance(type);
                                    arguments.Add(instance);
                                }
                                else
                                {
                                    arguments.Add(argument);
                                }
                            }

                            value = Activator.CreateInstance(attribute.InterceptorType, arguments.ToArray());
                        }

                        interceptors.Add(value as IInterceptor);
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
            //如果拦截器为0
            if (interceptors == null || interceptors.Length == 0)
            {
                return target;
            }

            ProxyGenerator proxy = new ProxyGenerator();
            ProxyGenerationOptions options = new ProxyGenerationOptions(new ProxyGenerationHook())
            {
                Selector = new InterceptorSelector()
            };

            return proxy.CreateInterfaceProxyWithTarget(proxyType, target, options, interceptors);
        }

        /// <summary>
        /// 创建一个类型方式的拦截器
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type classType, params IInterceptor[] interceptors)
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
        public static object CreateProxy(Type classType, object[] arguments, params IInterceptor[] interceptors)
        {
            //如果拦截器为0
            if (interceptors.Length == 0)
            {
                return Activator.CreateInstance(classType, arguments);
            }

            ProxyGenerator proxy = new ProxyGenerator();
            ProxyGenerationOptions options = new ProxyGenerationOptions(new ProxyGenerationHook())
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
