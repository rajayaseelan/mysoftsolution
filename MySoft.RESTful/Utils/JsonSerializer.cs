
namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// Json序列化
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object data)
        {
            return SerializationManager.SerializeJson(data);
        }
    }
}
