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
        private int cacheTime = -1;
        /// <summary>
        /// 数据缓存时间（单位：秒）
        /// </summary>
        public int CacheTime
        {
            get
            {
                return cacheTime;
            }
            set
            {
                cacheTime = value;
            }
        }

        private string errorMessage;
        /// <summary>
        /// 异常信息（当方法出现错误时抛出此异常信息，默认抛出系统异常）
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }
    }
}
