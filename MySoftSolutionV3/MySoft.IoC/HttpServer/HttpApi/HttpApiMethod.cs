using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// Http接口方法
    /// </summary>
    [Serializable]
    public class HttpApiMethod
    {
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

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

        private HttpMethod httpMethod;
        /// <summary>
        /// Http响应方式
        /// </summary>
        public HttpMethod HttpMethod
        {
            get
            {
                return httpMethod;
            }
            set
            {
                httpMethod = value;
            }
        }

        private MethodInfo method;
        /// <summary>
        /// 调用的方法
        /// </summary>
        internal MethodInfo Method
        {
            get
            {
                return method;
            }
        }

        /// <summary>
        /// 初始化HttpApiMethod
        /// </summary>
        /// <param name="name"></param>
        public HttpApiMethod(MethodInfo method)
        {
            this.method = method;
            this.httpMethod = HttpMethod.GET;
            this.authorized = false;
            this.authParameter = "username";
        }

        /// <summary>
        /// 初始化HttpApiMethod
        /// </summary>
        /// <param name="method"></param>
        /// <param name="authParameter"></param>
        public HttpApiMethod(MethodInfo method, string authParameter)
            : this(method)
        {
            this.authorized = true;
            this.authParameter = authParameter;
        }
    }
}
