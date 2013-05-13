using System;
using System.Xml.Serialization;

namespace MySoft.IoC.HttpServer.Config
{
    /// <summary>
    /// HttpApiConfig类
    /// </summary>
    [Serializable]
    [XmlRoot("apiconfig")]
    public class HttpApiConfig
    {
        private HttpApiService[] apiServices;
        /// <summary>
        /// Api服务集合
        /// </summary>
        [XmlElement("service", typeof(HttpApiService))]
        public HttpApiService[] ApiServices
        {
            get
            {
                return apiServices;
            }
            set
            {
                apiServices = value;
            }
        }
    }
}
