using System;
using System.Linq;
using System.Reflection;
using MySoft.IoC;

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
            var service = instance.GetType().GetMethod("GetService", Type.EmptyTypes)
                .MakeGenericMethod(serviceType).Invoke(instance, null);

            //查找认证用户并直接传递
            if (AuthenticationContext.Current != null && AuthenticationContext.Current.User != null)
            {
                var attribute = CoreHelper.GetMemberAttribute<PublishMethodAttribute>(method);
                if (attribute != null && !string.IsNullOrEmpty(attribute.AuthParameter))
                {
                    var index = method.GetParameters().ToList().FindIndex(p => p.Name == attribute.AuthParameter);
                    if (index >= 0)
                    {
                        Type pType = method.GetParameters()[index].ParameterType;
                        if (pType == typeof(int))
                            parameters[index] = AuthenticationContext.Current.User.AuthID;
                        else if (pType == typeof(string))
                            parameters[index] = AuthenticationContext.Current.User.AuthName;
                        else if (pType == typeof(AuthenticationUser))
                            parameters[index] = AuthenticationContext.Current.User;
                    }
                }
            }

            return DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);
        }

        #endregion
    }
}
