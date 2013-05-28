using System;
using System.Xml.Serialization;

namespace MySoft.IoC.HttpServer.Config
{
    /// <summary>
    /// HttpApiItem类
    /// </summary>
    [Serializable]
    [XmlRoot("api")]
    public class HttpApiItem : HttpApiService
    {
        private int cacheTime = -1;
        /// <summary>
        /// 数据缓存时间（单位：秒）
        /// </summary>
        [XmlAttribute("cacheTime")]
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

        private bool authorized = false;
        /// <summary>
        /// 是否认证
        /// </summary>
        [XmlAttribute("authorized")]
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
        [XmlAttribute("authparameter")]
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

        private string method;
        /// <summary>
        /// Http响应方式
        /// </summary>
        [XmlAttribute("method")]
        public string Method
        {
            get
            {
                return method;
            }
            set
            {
                method = value;
            }
        }

        /// <summary>
        /// Http响应方式
        /// </summary>
        public HttpMethod HttpMethod
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(method))
                        return (HttpMethod)Enum.Parse(typeof(HttpMethod), method, true);
                    else
                        return HttpMethod.GET;
                }
                catch
                {
                    return HttpMethod.GET;
                }
            }
        }
    }
}
