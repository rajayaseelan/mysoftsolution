using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace MySoft.IoC
{
    public class ProxyGenerationHook : IProxyGenerationHook
    {
        #region IProxyGenerationHook 成员

        public void MethodsInspected() { }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo) { }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            var att = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(methodInfo);
            if (att == null) return true;
            return att.UseAspect;
        }

        #endregion
    }
}
