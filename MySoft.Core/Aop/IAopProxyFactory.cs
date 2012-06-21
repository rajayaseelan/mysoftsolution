using System;

namespace MySoft.Aop
{
    /// <summary>
    /// IAopProxyFactory 用于创建特定的Aop代理的实例，IAopProxyFactory的作用是使AopProxyAttribute独立于具体的AOP代理类。
    /// 2010.11.09
    /// </summary>
    public interface IAopProxyFactory
    {
        /// <summary>
        /// 创建Aop代理对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        AopProxyBase CreateAopProxyInstance(MarshalByRefObject obj, Type type);
    }
}
