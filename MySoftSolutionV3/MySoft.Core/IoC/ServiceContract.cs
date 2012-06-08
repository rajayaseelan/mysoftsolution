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

        private int timeout = -1;
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
    }
}
