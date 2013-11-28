using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MySoft
{
    /// <summary>
    /// The serialization manager.
    /// </summary>
    public static class SerializationManager
    {
        /// <summary>
        /// 将对象系列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeJson(object obj, params JsonConverter[] converters)
        {
            return SerializeJson(obj, true, converters);
        }

        /// <summary>
        /// 将对象系列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="indented"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static string SerializeJson(object obj, bool indented, params JsonConverter[] converters)
        {
            return SerializeJson(obj, indented, null, converters);
        }

        #region 支持Contract解析的方法

        /// <summary>
        /// 将对象系列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resolver"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static string SerializeJson(object obj, IContractResolver resolver, params JsonConverter[] converters)
        {
            return SerializeJson(obj, true, resolver, converters);
        }

        /// <summary>
        /// 将对象系列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="indented"></param>
        /// <param name="resolver"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static string SerializeJson(object obj, bool indented, IContractResolver resolver, params JsonConverter[] converters)
        {
            var format = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;

            return JsonConvert.SerializeObject(obj, format, GetJsonSerializerSettings(resolver, converters));
        }

        #endregion

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="data"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static object DeserializeJson(Type returnType, string data, params JsonConverter[] converters)
        {
            return DeserializeJson(returnType, data, null, converters);
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static T DeserializeJson<T>(string data, params JsonConverter[] converters)
        {
            return (T)DeserializeJson(typeof(T), data, converters);
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="anonymousObject"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static T DeserializeJson<T>(string data, T anonymousObject, params JsonConverter[] converters)
        {
            return DeserializeJson<T>(data, converters);
        }

        #region 支持Contract解析的方法

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="data"></param>
        /// <param name="resolver"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static object DeserializeJson(Type returnType, string data, IContractResolver resolver, params JsonConverter[] converters)
        {
            if (string.IsNullOrEmpty(data)) return null;

            return JsonConvert.DeserializeObject(data, returnType, GetJsonSerializerSettings(resolver, converters));
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="resolver"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static T DeserializeJson<T>(string data, IContractResolver resolver, params JsonConverter[] converters)
        {
            return (T)DeserializeJson(typeof(T), data, resolver, converters);
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="anonymousObject"></param>
        /// <param name="resolver"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static T DeserializeJson<T>(string data, T anonymousObject, IContractResolver resolver, params JsonConverter[] converters)
        {
            return DeserializeJson<T>(data, resolver, converters);
        }

        #endregion

        /// <summary>
        /// 将对象系列化成二进制
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] SerializeBin(object obj)
        {
            if (obj == null) return new byte[0];

            using (var memoryStream = new MemoryStream())
            {
                //Serialize the message
                new BinaryFormatter().Serialize(memoryStream, obj);

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 将数据反系列化成对象
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static object DeserializeBin(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }

            using (var deserializeMemoryStream = new MemoryStream(buffer))
            {
                deserializeMemoryStream.Position = 0;

                //Deserialize the message
                return new BinaryFormatter().Deserialize(deserializeMemoryStream);
            }
        }

        /// <summary>
        /// 将数据反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeBin<T>(byte[] data)
        {
            return (T)DeserializeBin(data);
        }

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static string SerializeXml(object obj)
        {
            return SerializeXml(obj, Encoding.UTF8);
        }

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="encoding">The obj.</param>
        /// <returns></returns>
        public static string SerializeXml(object obj, Encoding encoding)
        {
            if (obj == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            using (XmlTextWriter xw = new XmlTextWriter(ms, encoding))
            {
                xw.Formatting = System.Xml.Formatting.Indented;
                XmlSerializer serializer = GetSerializer(obj.GetType());

                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                serializer.Serialize(xw, obj, ns);
                return encoding.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Deserializes the specified return type.
        /// </summary>
        /// <param name="T">Type of the return.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static T DeserializeXml<T>(string data)
        {
            return DeserializeXml<T>(data, Encoding.UTF8);
        }

        /// <summary>
        /// Deserializes the specified return type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static object DeserializeXml(Type returnType, string data)
        {
            return DeserializeXml(returnType, data, Encoding.UTF8);
        }

        /// <summary>
        /// Deserializes the specified return type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static T DeserializeXml<T>(string data, Encoding encoding)
        {
            return (T)DeserializeXml(typeof(T), data, encoding);
        }

        /// <summary>
        /// Deserializes the specified return type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static object DeserializeXml(Type returnType, string data, Encoding encoding)
        {
            if (data == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(data)))
            using (XmlReader xr = XmlReader.Create(ms))
            {
                XmlSerializer serializer = GetSerializer(returnType);
                return serializer.Deserialize(xr);
            }
        }

        private static readonly IDictionary<Type, XmlSerializer> cacheSerializer = new Dictionary<Type, XmlSerializer>();

        /// <summary>
        /// 获取序列化器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static XmlSerializer GetSerializer(Type type)
        {
            lock (cacheSerializer)
            {
                XmlSerializer serializer;
                if (!cacheSerializer.TryGetValue(type, out serializer))
                {
                    serializer = new XmlSerializer(type);
                    cacheSerializer[type] = serializer;
                }

                return serializer;
            }
        }

        /// <summary>
        /// GetJsonSerializerSettings
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        private static JsonSerializerSettings GetJsonSerializerSettings(IContractResolver resolver, JsonConverter[] converters)
        {
            if (converters == null || converters.Length == 0) converters = null;

            var settings = new JsonSerializerSettings
            {
                ContractResolver = resolver,
                Converters = converters,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = DateParseHandling.None,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff"
            };

            return settings;
        }
    }
}
