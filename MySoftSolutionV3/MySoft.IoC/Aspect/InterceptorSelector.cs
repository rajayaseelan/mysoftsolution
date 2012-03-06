using System;
using System.Linq;
using System.Reflection;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 拦截器选择
    /// </summary>
    public class InterceptorSelector : Castle.DynamicProxy.IInterceptorSelector
    {
        #region IInterceptorSelector 成员

        public Castle.DynamicProxy.IInterceptor[] SelectInterceptors(Type type, MethodInfo method, Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            if (interceptors == null) return interceptors;

            var att = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(method);
            if (att == null)
            {
                return interceptors;
            }
            else if (att.UseAspect)
            {
                if (att.InterceptorTypes == null)
                {
                    return interceptors;
                }
                else
                {
                    return interceptors.Where(p => att.InterceptorTypes.Contains(p.GetType())).ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
