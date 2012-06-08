using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// Http调用属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpInvokeAttribute : ContractAttribute
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

        /// <summary>
        /// 初始化HttpInvokeAttribute
        /// </summary>
        public HttpInvokeAttribute()
        {
            this.authorized = false;
            this.authParameter = "username";
        }

        /// <summary>
        /// 初始化HttpInvokeAttribute
        /// </summary>
        /// <param name="name"></param>
        public HttpInvokeAttribute(string name)
            : this()
        {
            this.Name = name;
        }
    }
}
