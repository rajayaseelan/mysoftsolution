using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// 约束的基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false)]
    public abstract class ContractAttribute : Attribute
    {
        private string description;
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        protected bool allowCache;
        /// <summary>
        /// 是否允许缓存
        /// </summary>
        public bool AllowCache
        {
            get
            {
                return allowCache;
            }
            set
            {
                allowCache = value;
            }
        }

        protected double timeout = -1;
        /// <summary>
        /// 超时时间（单位：秒）
        /// </summary>
        public double Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        protected double cacheTime = -1;
        /// <summary>
        /// 缓存时间（单位：秒）
        /// </summary>
        public double CacheTime
        {
            get
            {
                return cacheTime;
            }
            set
            {
                cacheTime = value;
                if (cacheTime > 0)
                {
                    allowCache = true;
                }
            }
        }
    }
}
