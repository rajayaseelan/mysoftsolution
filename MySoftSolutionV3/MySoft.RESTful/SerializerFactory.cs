
using MySoft.RESTful.Utils;
namespace MySoft.RESTful
{
    /// <summary>
    /// 系列化工厂
    /// </summary>
    public sealed class SerializerFactory
    {
        /// <summary>
        /// 创建系列化
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static ISerializer Create(ParameterFormat format)
        {
            switch (format)
            {
                case ParameterFormat.Xml:
                    return new XmlSerializer();
                case ParameterFormat.Text:
                case ParameterFormat.Html:
                case ParameterFormat.Json:
                case ParameterFormat.Jsonp:
                    return new JsonSerializer();
                default:
                    return new JsonSerializer();
            }
        }
    }
}

