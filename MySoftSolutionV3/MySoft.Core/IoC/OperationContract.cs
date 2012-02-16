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
        private string authParameter;
        /// <summary>
        /// 认证参数
        /// </summary>
        public string AuthParameter
        {
            get
            {
                return authParameter;
            }
            set
            {
                authParameter = value;
            }
        }

        private bool authorized;
        /// <summary>
        /// 是否认证
        /// </summary>
        public bool Authorized
        {
            get
            {
                return authorized;
            }
            set
            {
                authorized = value;
            }
        }

        private bool httpEnabled = false;
        /// <summary>
        /// 发布为http方法
        /// </summary>
        public bool HttpEnabled
        {
            get
            {
                return httpEnabled;
            }
            set
            {
                httpEnabled = value;
            }
        }

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
    }
}
