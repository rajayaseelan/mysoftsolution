using System;

namespace MySoft.IoC
{
    /// <summary>
    /// Attribute used to mark service interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ServiceContractAttribute : ContractAttribute
    {
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
