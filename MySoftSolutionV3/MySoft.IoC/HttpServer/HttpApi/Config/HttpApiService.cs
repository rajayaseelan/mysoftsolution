using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MySoft.IoC.HttpServer.Config
{
    /// <summary>
    /// HttpApiService类
    /// </summary>
    [Serializable]
    [XmlRoot("service")]
    public class HttpApiService
    {
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        [XmlAttribute("name")]
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

        private string fullname;
        /// <summary>
        /// 名称全称
        /// </summary>
        [XmlAttribute("fullname")]
        public string FullName
        {
            get
            {
                return fullname;
            }
            set
            {
                fullname = value;
            }
        }

        private string description;
        /// <summary>
        /// 描述信息
        /// </summary>
        [XmlAttribute("description")]
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

        private HttpApiItem[] apiItems;
        /// <summary>
        /// Api集合
        /// </summary>
        [XmlElement("api", typeof(HttpApiItem))]
        public HttpApiItem[] ApiItems
        {
            get
            {
                return apiItems;
            }
            set
            {
                apiItems = value;
            }
        }
    }
}
