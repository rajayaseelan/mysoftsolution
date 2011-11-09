using System;
using System.Linq;
using System.Reflection;
using MySoft.IoC;
using MySoft.RESTful.Auth;
using System.Collections.Generic;

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 代理委托
    /// </summary>
    public class ProxyInvocationHandler : IProxyInvocationHandler
    {
        private Type serviceType;
        public ProxyInvocationHandler(Type serviceType)
        {
            this.serviceType = serviceType;
        }

        #region IProxyInvocationHandler 成员

        /// <summary>
        /// 委托调用
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, MethodInfo method, object[] parameters)
        {
            var instance = CastleFactory.Create();
            var service = instance.GetType().GetMethod("CreateChannel", Type.EmptyTypes)
                .MakeGenericMethod(serviceType).Invoke(instance, null);

            return DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);
        }

        #endregion
    }
}
