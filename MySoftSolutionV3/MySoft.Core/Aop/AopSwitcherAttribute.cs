using System;

namespace MySoft.Aop
{
    /// <summary>
    /// AopSwitcherAttribute 用于决定一个被AopProxyAttribute修饰的class的某个特定方法是否启用截获 。
    /// 创建原因：绝大多数时候我们只希望对某个类的一部分Method而不是所有Method使用截获。
    /// 使用方法：如果一个方法没有使用AopSwitcherAttribute特性或使用AopSwitcherAttribute(false)修饰，
    ///　　　 都不会对其进行截获。只对使用了AopSwitcherAttribute(true)启用截获。
    /// 2010.11.09
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AopSwitcherAttribute : Attribute
    {
        private bool useAspect = false;

        public AopSwitcherAttribute(bool useAop)
        {
            this.useAspect = useAop;
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
