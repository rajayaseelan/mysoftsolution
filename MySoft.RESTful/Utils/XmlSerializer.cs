using System.Text;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// xml序列化
    /// </summary>
    public class XmlSerializer : ISerializer
    {
        public string Serialize(object data)
        {
            return SerializationManager.SerializeXml(data, Encoding.UTF8);
        }
    }
}
