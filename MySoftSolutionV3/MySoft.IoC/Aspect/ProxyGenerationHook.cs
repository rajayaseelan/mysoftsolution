using System;
using System.Collections;
using System.Reflection;
using Castle.DynamicProxy;

namespace MySoft.IoC.Aspect
{
    public class ProxyGenerationHook : IProxyGenerationHook
    {
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        #region IProxyGenerationHook 成员

        public void MethodsInspected() { }

        public void NonProxyableMemberNotification(Type type, MemberInfo method) { }

        public bool ShouldInterceptMethod(Type type, MethodInfo method)
        {
            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(method))
                {
                    var att = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(method);
                    if (att == null) return true;
                    hashtable[method] = att.UseAspect;
                }
                else
                {
                    hashtable[method] = false;
                }
            }

            return Convert.ToBoolean(hashtable[method]);
        }

        #endregion
    }
}
