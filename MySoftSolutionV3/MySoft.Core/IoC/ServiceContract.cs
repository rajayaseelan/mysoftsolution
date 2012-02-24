using System;

namespace MySoft.IoC
{
    /// <summary>
    /// Attribute used to mark service interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ServiceContractAttribute : Attribute
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
    }
}
