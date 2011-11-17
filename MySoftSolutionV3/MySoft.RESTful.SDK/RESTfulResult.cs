using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful结果
    /// </summary>
    [Serializable]
    [XmlRoot("result")]
    public class RESTfulResult
    {
        /// <summary>
        /// 代码
        /// </summary>
        [XmlElement("code")]
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [XmlElement("msg")]
        [JsonProperty("msg")]
        public string Message { get; set; }
    }

    /// <summary>
    /// RESTful响应
    /// </summary>
    [Serializable]
    [XmlRoot("result")]
    public class RESTfulResponse
    {
        [XmlElement("val")]
        [JsonProperty("val")]
        public object Value { get; set; }
    }
}
