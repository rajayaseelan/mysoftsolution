using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 切面方法选择器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AspectSwitcherAttribute : Attribute
    {
        private bool useAspect = false;
        private Type[] interceptorTypes;

        public AspectSwitcherAttribute(params Type[] interceptorTypes)
        {
            this.useAspect = true;
            this.interceptorTypes = interceptorTypes;
        }

        public AspectSwitcherAttribute(bool useAop)
        {
            this.useAspect = useAop;
        }

        /// <summary>
        /// 使用拦截器的类型
        /// </summary>
        public Type[] InterceptorTypes
        {
            get
            {
                return interceptorTypes;
            }
        }

        /// <summary>
        /// 是否使用切面处理
        /// </summary>
        public bool UseAspect
        {
            get
            {
                return this.useAspect;
            }
        }
    }
}
