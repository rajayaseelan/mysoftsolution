using System.IO;
using System.Text;

namespace MySoft.RESTful
{
    /// <summary>
    /// xml系列化
    /// </summary>
    public class XmlSerializer : ISerializer
    {
        public string Serialize(object data, bool jsonp)
        {
            return SerializationManager.SerializeXml(data, Encoding.UTF8);
        }
    }
}
