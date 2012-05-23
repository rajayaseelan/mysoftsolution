using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// Jsonp序列化
    /// </summary>
    public class JsonpSerializer : ISerializer
    {
        public string Serialize(object data)
        {
            return SerializationManager.SerializeJson(data, new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
        }
    }
}
