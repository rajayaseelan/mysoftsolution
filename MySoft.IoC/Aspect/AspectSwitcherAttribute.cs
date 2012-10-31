using System;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 切面方法选择器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AspectSwitcherAttribute : Attribute
    {
        private bool useAspect = false;
        private Type[] interceptorTypes;

        /// <summary>
        /// 使用拦截器的类型
        /// </summary>
        public Type[] InterceptorTypes
        {
            get
            {
                return interceptorTypes;
            }
            set
            {
                interceptorTypes = value;
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

        /// <summary>
        /// 响应的消息
        /// </summary>
        public string Description { get; set; }

        public AspectSwitcherAttribute(params Type[] interceptorTypes)
        {
            this.useAspect = true;
            this.interceptorTypes = interceptorTypes;
        }

        public AspectSwitcherAttribute(bool useAop)
        {
            this.useAspect = useAop;
        }
    }
}
