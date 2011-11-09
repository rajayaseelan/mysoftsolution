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
        public ServiceContractAttribute()
        {
            this.allowCache = true;
        }

        /// <summary>
        /// 实例化ServiceContractAttribute
        /// </summary>
        /// <param name="allowCache"></param>
        public ServiceContractAttribute(bool allowCache)
        {
            this.allowCache = allowCache;
        }

        /// <summary>
        /// 实例化ServiceContractAttribute
        /// </summary>
        /// <param name="cacheTime"></param>
        public ServiceContractAttribute(int cacheTime)
            : this(true)
        {
            this.cacheTime = cacheTime;
        }
    }
}
