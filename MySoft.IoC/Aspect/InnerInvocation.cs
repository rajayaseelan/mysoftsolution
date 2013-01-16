using System;
using System.Reflection;
using IOC = MySoft.IoC.Messages;

namespace MySoft.IoC.Aspect
{
    /// <summary>
    /// 内部调用对象
    /// </summary>
    internal class InnerInvocation : IInvocation
    {
        private Castle.DynamicProxy.IInvocation invocation;
        private IOC.ParameterCollection parameters;
        private string description;

        public InnerInvocation(Castle.DynamicProxy.IInvocation invocation)
        {
            this.invocation = invocation;
            this.description = string.Empty;
            this.parameters = new IOC.ParameterCollection();

            if (invocation.InvocationTarget != null)
            {
                var description = string.Empty;
                var attr = CoreHelper.GetMemberAttribute<AspectSwitcherAttribute>(invocation.MethodInvocationTarget);
                if (attr != null) description = attr.Description;

                this.description = description;

                var pis = invocation.MethodInvocationTarget.GetParameters();
                if (pis != null && pis.Length > 0)
                {
                    for (var index = 0; index < pis.Length; index++)
                    {
                        this.parameters[pis[index].Name] = invocation.GetArgumentValue(index);
                    }
                }
            }
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

        #region 内部接口

        /// <summary>
        /// 参数集合信息
        /// </summary>
        public IOC.ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// 响应的消息
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
        }

        #endregion

        #endregion
    }
}
