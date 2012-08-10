using System;
using System.Collections;
using System.Reflection;
using Castle.DynamicProxy;

namespace MySoft.IoC.Aspect
{
    public class ProxyGenerationHook : IProxyGenerationHook
    {
        #region IProxyGenerationHook 成员

        public void MethodsInspected() { }

        public void NonProxyableMemberNotification(Type type, MemberInfo method) { }

        public bool ShouldInterceptMethod(Type type, MethodInfo method)
        {
            var att = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(method);
            if (att == null) return true;

            return att.UseAspect;
        }

        #endregion
    }
}
