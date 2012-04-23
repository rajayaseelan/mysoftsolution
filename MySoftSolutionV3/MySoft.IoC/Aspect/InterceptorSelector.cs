using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 拦截器选择
    /// </summary>
    public class InterceptorSelector : Castle.DynamicProxy.IInterceptorSelector
    {
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        #region IInterceptorSelector 成员

        public Castle.DynamicProxy.IInterceptor[] SelectInterceptors(Type type, MethodInfo method, Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            if (interceptors == null || interceptors.Length == 0)
                return interceptors;

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(method))
                {
                    Castle.DynamicProxy.IInterceptor[] ilist = null;
                    var att = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(method);
                    if (att != null && att.UseAspect)
                    {
                        if (att.InterceptorTypes == null)
                            ilist = interceptors;
                        else
                            ilist = interceptors.Where(p => att.InterceptorTypes.Contains(p.GetType())).ToArray();
                    }

                    hashtable[method] = ilist;
                }
            }

            //返回拦截器
            return hashtable[method] as Castle.DynamicProxy.IInterceptor[];
        }

        #endregion
    }
}
