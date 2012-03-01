using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 内部调用对象
    /// </summary>
    internal class InnerInvocation : IInvocation
    {
        private Castle.DynamicProxy.IInvocation invocation;
        public InnerInvocation(Castle.DynamicProxy.IInvocation invocation)
        {
            this.invocation = invocation;
        }

        #region IInvocation 成员

        /// <summary>
        /// 调用参数
        /// </summary>
        public object[] Arguments
        {
            get { return invocation.Arguments; }
        }

        /// <summary>
        /// 泛型参数
        /// </summary>
        public Type[] GenericArguments
        {
            get { return invocation.GenericArguments; }
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetArgumentValue(int index)
        {
            return invocation.GetArgumentValue(index);
        }

        /// <summary>
        /// 具体方法
        /// </summary>
        /// <returns></returns>
        public MethodInfo GetConcreteMethod()
        {
            return invocation.GetConcreteMethod();
        }

        /// <summary>
        /// 具体调用方法
        /// </summary>
        /// <returns></returns>
        public MethodInfo GetConcreteMethodInvocationTarget()
        {
            return invocation.GetConcreteMethodInvocationTarget();
        }

        /// <summary>
        /// 调用对象
        /// </summary>
        public object InvocationTarget
        {
            get { return invocation.InvocationTarget; }
        }

        /// <summary>
        /// 当前方法
        /// </summary>
        public MethodInfo Method
        {
            get { return invocation.Method; }
        }

        /// <summary>
        /// 当前调用方法
        /// </summary>
        public MethodInfo MethodInvocationTarget
        {
            get { return invocation.MethodInvocationTarget; }
        }

        /// <summary>
        /// 调用
        /// </summary>
        public void Proceed()
        {
            invocation.Proceed();
        }

        /// <summary>
        /// 代理对象
        /// </summary>
        public object Proxy
        {
            get { return invocation.Proxy; }
        }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue
        {
            get
            {
                return invocation.ReturnValue;
            }
            set
            {
                invocation.ReturnValue = value;
            }
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetArgumentValue(int index, object value)
        {
            invocation.SetArgumentValue(index, value);
        }

        /// <summary>
        /// 目标对象类型
        /// </summary>
        public Type TargetType
        {
            get { return invocation.TargetType; }
        }

        #endregion
    }
}
