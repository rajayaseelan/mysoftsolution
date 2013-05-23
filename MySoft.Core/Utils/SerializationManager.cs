﻿using System;
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
        #region Nested classes

        /// <summary>
        /// This class is used in deserializing to allow deserializing objects that are defined
        /// in assemlies that are load in runtime (like PlugIns).
        /// </summary>
        internal sealed class DeserializationAppDomainBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                var toAssemblyName = assemblyName.Split(',')[0];
                return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        where assembly.FullName.Split(',')[0] == toAssemblyName
                        select assembly.GetType(typeName)).FirstOrDefault();
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// The serialize delegate.
        /// </summary>
        /// <param name="obj">obj to be serialized.</param>
        /// <returns></returns>
        public delegate string TypeSerializeHandler(object obj);
        /// <summary>
        /// The deserialize delegate.
        /// </summary>
        /// <param name="data">the data to be deserialied.</param>
        /// <returns></returns>
        public delegate object TypeDeserializeHandler(string data);

        #endregion

        private static IDictionary<Type, KeyValuePair<TypeSerializeHandler, TypeDeserializeHandler>> handlers = new Dictionary<Type, KeyValuePair<TypeSerializeHandler, TypeDeserializeHandler>>();

        static SerializationManager()
        {
            InitDefaultSerializeHandlers();
        }

        #region 数据系列化

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
                new BinaryFormatter().Serialize(memoryStream, obj);
                memoryStream.Close();

                return memoryStream.ToArray();
            }
        }

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
            var format = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            if (converters == null || converters.Length == 0)
                return JsonConvert.SerializeObject(obj, format, new Newtonsoft.Json.Converters.IsoDateTimeConverter());
            else
                return JsonConvert.SerializeObject(obj, format, converters);
        }

        #region 支持Contract解析的方法

        /// <summary>
        /// 将对象系列化成字符串
        /// </summary>
        /// <param name="obj"></param>
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
        /// <param name="converters"></param>
        /// <returns></returns>
        public static string SerializeJson(object obj, bool indented, IContractResolver resolver, params JsonConverter[] converters)
        {
            var format = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            if (converters == null || converters.Length == 0)
                return JsonConvert.SerializeObject(obj, format, new JsonSerializerSettings { ContractResolver = resolver, Converters = new[] { new Newtonsoft.Json.Converters.IsoDateTimeConverter() } });
            else
                return JsonConvert.SerializeObject(obj, format, new JsonSerializerSettings { ContractResolver = resolver, Converters = converters });
        }

        #endregion

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
                var binaryFormatter = new BinaryFormatter
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    FilterLevel = TypeFilterLevel.Full,
                    Binder = new DeserializationAppDomainBinder()
                };

                return binaryFormatter.Deserialize(deserializeMemoryStream);
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
        /// 将字符串反系列化成对象
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object DeserializeJson(Type returnType, string data, params JsonConverter[] converters)
        {
            if (string.IsNullOrEmpty(data)) return null;

            if (converters == null || converters.Length == 0)
                return JsonConvert.DeserializeObject(data, returnType, new Newtonsoft.Json.Converters.IsoDateTimeConverter());
            else
                return JsonConvert.DeserializeObject(data, returnType, converters);
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
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
        /// <returns></returns>
        public static object DeserializeJson(Type returnType, string data, IContractResolver resolver, params JsonConverter[] converters)
        {
            if (string.IsNullOrEmpty(data)) return null;

            if (converters == null || converters.Length == 0)
                return JsonConvert.DeserializeObject(data, returnType, new JsonSerializerSettings { ContractResolver = resolver, Converters = new[] { new Newtonsoft.Json.Converters.IsoDateTimeConverter() } });
            else
                return JsonConvert.DeserializeObject(data, returnType, new JsonSerializerSettings { ContractResolver = resolver, Converters = converters });
        }

        /// <summary>
        /// 将字符串反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
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
        /// <returns></returns>
        public static T DeserializeJson<T>(string data, T anonymousObject, IContractResolver resolver, params JsonConverter[] converters)
        {
            return DeserializeJson<T>(data, resolver, converters);
        }

        #endregion

        #endregion

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static string SerializeXml(object obj)
        {
            return SerializeXml(obj, Encoding.Default);
        }

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static string SerializeXml(object obj, Encoding encoding)
        {
            if (obj == null)
            {
                return null;
            }

            if (handlers.ContainsKey(obj.GetType()))
            {
                return handlers[obj.GetType()].Key(obj);
            }
            else
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");

                if (encoding == Encoding.Default)
                {
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    {
                        XmlSerializer serializer = GetSerializer(obj.GetType());
                        serializer.Serialize(sw, obj, namespaces);
                        return sb.ToString();
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (XmlTextWriter xw = new XmlTextWriter(ms, encoding))
                    {
                        xw.Formatting = System.Xml.Formatting.Indented;
                        XmlSerializer serializer = GetSerializer(obj.GetType());
                        serializer.Serialize(xw, obj, namespaces);
                        return encoding.GetString(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes the specified return type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static T DeserializeXml<T>(string data)
        {
            return DeserializeXml<T>(data, Encoding.Default);
        }

        /// <summary>
        /// Deserializes the specified return type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static object DeserializeXml(Type returnType, string data)
        {
            return DeserializeXml(returnType, data, Encoding.Default);
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

            if (encoding == Encoding.Default)
            {
                using (StringReader sr = new StringReader(data))
                {
                    XmlSerializer serializer = GetSerializer(returnType);
                    return serializer.Deserialize(sr);
                }
            }
            else
            {
                using (MemoryStream ms = new MemoryStream(encoding.GetBytes(data)))
                using (StreamReader sr = new StreamReader(ms, encoding))
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    XmlSerializer serializer = GetSerializer(returnType);
                    return serializer.Deserialize(xr);
                }
            }
        }

        private static readonly IDictionary<string, XmlSerializer> cacheSerializer = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// 获取序列化器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static XmlSerializer GetSerializer(Type type)
        {
            var key = type.ToString();

            XmlSerializer serializer;
            if (!cacheSerializer.TryGetValue(key, out serializer))
            {
                serializer = new XmlSerializer(type);
                cacheSerializer.Add(key, serializer);
            }

            return serializer;
        }

        /// <summary>
        /// Registers the serialize handler.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializeHandler">The serialize handler.</param>
        /// <param name="deserializeHandler">The deserialize handler.</param>
        public static void RegisterSerializeHandler(Type type, TypeSerializeHandler serializeHandler, TypeDeserializeHandler deserializeHandler)
        {
            lock (handlers)
            {
                if (handlers.ContainsKey(type))
                {
                    handlers[type] = new KeyValuePair<TypeSerializeHandler, TypeDeserializeHandler>(serializeHandler, deserializeHandler);
                }
                else
                {
                    handlers.Add(type, new KeyValuePair<TypeSerializeHandler, TypeDeserializeHandler>(serializeHandler, deserializeHandler));
                }
            }
        }

        /// <summary>
        /// Unregisters the serialize handler.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void UnregisterSerializeHandler(Type type)
        {
            lock (handlers)
            {
                if (handlers.ContainsKey(type))
                {
                    handlers.Remove(type);
                }
            }
        }

        #region InitDefaultSerializeHandlers

        private static void InitDefaultSerializeHandlers()
        {
            RegisterSerializeHandler(typeof(string), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadString));
            RegisterSerializeHandler(typeof(int), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadInt));
            RegisterSerializeHandler(typeof(long), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadLong));
            RegisterSerializeHandler(typeof(short), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadShort));
            RegisterSerializeHandler(typeof(byte), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadByte));
            RegisterSerializeHandler(typeof(bool), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadBool));
            RegisterSerializeHandler(typeof(decimal), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadDecimal));
            RegisterSerializeHandler(typeof(char), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadChar));
            RegisterSerializeHandler(typeof(sbyte), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadSbyte));
            RegisterSerializeHandler(typeof(float), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadFloat));
            RegisterSerializeHandler(typeof(double), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadDouble));
            RegisterSerializeHandler(typeof(byte[]), new TypeSerializeHandler(ByteArrayToString), new TypeDeserializeHandler(LoadByteArray));
            RegisterSerializeHandler(typeof(Guid), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadGuid));
            RegisterSerializeHandler(typeof(DateTime), new TypeSerializeHandler(ToString), new TypeDeserializeHandler(LoadDateTime));
        }

        private static string ToString(object obj)
        {
            return obj.ToString();
        }

        private static object LoadString(string data)
        {
            return data;
        }

        private static object LoadInt(string data)
        {
            return int.Parse(data);
        }

        private static object LoadLong(string data)
        {
            return long.Parse(data);
        }

        private static object LoadShort(string data)
        {
            return short.Parse(data);
        }

        private static object LoadByte(string data)
        {
            return byte.Parse(data);
        }

        private static object LoadBool(string data)
        {
            return bool.Parse(data);
        }

        private static object LoadDecimal(string data)
        {
            return decimal.Parse(data);
        }

        private static object LoadChar(string data)
        {
            return char.Parse(data);
        }

        private static object LoadSbyte(string data)
        {
            return sbyte.Parse(data);
        }

        private static object LoadFloat(string data)
        {
            return float.Parse(data);
        }

        private static object LoadDouble(string data)
        {
            return double.Parse(data);
        }

        private static string ByteArrayToString(object obj)
        {
            return Convert.ToBase64String((byte[])obj);
        }

        private static object LoadByteArray(string data)
        {
            return Convert.FromBase64String(data);
        }

        private static object LoadGuid(string data)
        {
            return new Guid(data);
        }

        private static object LoadDateTime(string data)
        {
            return DateTime.Parse(data);
        }

        #endregion
    }
}
