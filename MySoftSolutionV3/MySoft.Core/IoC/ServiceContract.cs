using System;

namespace MySoft.IoC
{
    /// <summary>
    /// Attribute used to mark service interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ServiceContractAttribute : ContractAttribute
    {
        private Type callbackType;
        /// <summary>
        /// 回调类型
        /// </summary>
        public Type CallbackType
        {
            get { return callbackType; }
            set { callbackType = value; }
        }

        /// <summary>
        /// 实例化ServiceContractAttribute
        /// </summary>
        public ServiceContractAttribute() : base() { }

        /// <summary>
        /// 实例化OperationContractAttribute
        /// </summary>
        /// <param name="name"></param>
        public ServiceContractAttribute(string name) : base(name) { }
    }
}
