using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful
{
    /// <summary>
    /// Json系列化
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object data, bool jsonp)
        {
            if (jsonp)
                return SerializationManager.SerializeJson(data, new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
            else
                return SerializationManager.SerializeJson(data);
        }
    }
}
