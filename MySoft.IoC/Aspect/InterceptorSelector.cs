using System;
using System.Linq;
using System.Reflection;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 拦截器选择
    /// </summary>
    internal class InterceptorSelector : Castle.DynamicProxy.IInterceptorSelector
    {
        #region IInterceptorSelector 成员

        public Castle.DynamicProxy.IInterceptor[] SelectInterceptors(Type type, MethodInfo method, Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            if (interceptors == null || interceptors.Length == 0)
            {
                return interceptors;
            }

            Castle.DynamicProxy.IInterceptor[] array = null;

            method = CoreHelper.GetMethodFromType(type, method.ToString());
            var attr = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(method);
            if (attr == null || attr.UseAspect)
            {
                array = interceptors;
            }
            else if (attr.UseAspect && attr.InterceptorTypes != null)
            {
                array = interceptors.Where(p => attr.InterceptorTypes.Contains(p.GetType())).ToArray();
            }

            return array;
        }

        #endregion
    }
}
