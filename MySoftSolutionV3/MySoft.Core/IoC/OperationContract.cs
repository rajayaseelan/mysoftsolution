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
        private int clientCacheTime = -1;
        /// <summary>
        /// 客户端缓存时间（单位：秒）
        /// </summary>
        public int ClientCacheTime
        {
            get
            {
                return clientCacheTime;
            }
            set
            {
                clientCacheTime = value;
            }
        }

        private int serverCacheTime = -1;
        /// <summary>
        /// 服务端缓存时间（单位：秒）
        /// </summary>
        public int ServerCacheTime
        {
            get
            {
                return serverCacheTime;
            }
            set
            {
                serverCacheTime = value;
            }
        }

        private string cacheKey;
        /// <summary>
        /// 获取或设置CacheKey
        /// </summary>
        public string CacheKey
        {
            get
            {
                return cacheKey;
            }
            set
            {
                cacheKey = value;
            }
        }

        /// <summary>
        /// 实例化OperationContractAttribute
        /// </summary>
        public OperationContractAttribute() { }
    }
}
