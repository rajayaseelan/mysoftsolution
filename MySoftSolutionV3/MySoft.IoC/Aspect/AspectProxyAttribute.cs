using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 拦截器属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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

        private object arguments;
        /// <summary>
        /// 拦截器参数
        /// </summary>
        public object Arguments
        {
            get
            {
                return arguments;
            }
            set
            {
                arguments = value;
            }
        }

        public AspectProxyAttribute(Type interceptorType)
        {
            this.interceptorType = interceptorType;
        }

        public AspectProxyAttribute(Type interceptorType, object arguments)
        {
            this.interceptorType = interceptorType;
            this.arguments = arguments;
        }
    }
}
