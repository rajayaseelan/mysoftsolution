using System;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 拦截器属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class AspectProxyAttribute : Attribute
    {
        private Type interceptorType;
        /// <summary>
        /// 拦截器对象
        /// </summary>
        public Type InterceptorType
        {
            get
            {
                return interceptorType;
            }
        }

        public AspectProxyAttribute(Type interceptorType)
        {
            this.interceptorType = interceptorType;
        }
    }
}
