using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// Text系列化
    /// </summary>
    public class TextSerializer : ISerializer
    {
        public string Serialize(object data, bool jsonp)
        {
            if (data is string)
                return data.ToString();
            else
            {
                var jo = JObject.Parse(SerializationManager.SerializeJson(data));
                List<string> list = new List<string>();
                foreach (var p in jo.Properties())
                {
                    list.Add(p.Name + "=" + SerializationManager.DeserializeJson<string>(p.Value.ToString(Formatting.None)));
                }

                return string.Join("&", list.ToArray());
            }
        }
    }
}
