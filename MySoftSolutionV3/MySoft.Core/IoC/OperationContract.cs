using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// Attribute used to mark service interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OperationContractAttribute : ContractAttribute
    {
        private bool compress;
        /// <summary>
        /// 是否压缩
        /// </summary>
        public bool Compress
        {
            get { return compress; }
            set { compress = value; }
        }

        private bool encrypt;
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool Encrypt
        {
            get { return encrypt; }
            set { encrypt = value; }
        }

        /// <summary>
        /// 实例化OperationContractAttribute
        /// </summary>
        public OperationContractAttribute()
        {
            this.allowCache = false;
        }

        /// <summary>
        /// 实例化OperationContractAttribute
        /// </summary>
        /// <param name="allowCache"></param>
        public OperationContractAttribute(bool allowCache)
        {
            this.allowCache = allowCache;
        }

        /// <summary>
        /// 实例化OperationContractAttribute
        /// </summary>
        /// <param name="cacheTime"></param>
        public OperationContractAttribute(int cacheTime)
            : this(true)
        {
            this.cacheTime = cacheTime;
        }
    }

}
