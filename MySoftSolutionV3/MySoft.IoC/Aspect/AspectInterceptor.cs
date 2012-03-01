using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// Aspect拦截器
    /// </summary>
    public class AspectInterceptor : Castle.DynamicProxy.IInterceptor
    {
        #region IInterceptor 成员

        /// <summary>
        /// 调用拦截的方法
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            var invocate = new InnerInvocation(invocation);

            this.PreProceed(invocate);
            this.PerformProceed(invocate);
            this.PostProceed(invocate);
        }

        #endregion

        /// <summary>
        /// 准备处理
        /// </summary>
        /// <param name="invocation"></param>
        protected virtual void PreProceed(IInvocation invocation)
        {
            //TO DO
        }

        /// <summary>
        /// 处理进行中
        /// </summary>
        /// <param name="invocation"></param>
        protected virtual void PerformProceed(IInvocation invocation)
        {
            invocation.Proceed();
        }

        /// <summary>
        /// 处理之后
        /// </summary>
        /// <param name="invocation"></param>
        protected virtual void PostProceed(IInvocation invocation)
        {
            //TO DO
        }
    }
}
