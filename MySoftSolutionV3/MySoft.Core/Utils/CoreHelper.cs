using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MySoft.Converter;
using Newtonsoft.Json.Utilities;

namespace MySoft
{
    /// <summary>
    /// 常用方法
    /// </summary>
    public static class CoreHelper
    {
        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            if (HttpContext.Current == null) return "localhost";
            HttpRequest request = HttpContext.Current.Request;

            //获取客户端真实IP
            string clientIp = request.ServerVariables["HTTP_CDN_SRC_IP"];

            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }

            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = request.UserHostAddress;
            }

            return clientIp;
        }

        /// <summary>
        /// 获取当前某文件绝对路径
        /// </summary>
        /// <returns></returns>
        public static string GetFullPath(string path)
        {
            path = path.Replace("/", "\\").TrimStart('\\');

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        /// <summary>
        /// 获取指定类型的默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetTypeDefaultValue(Type type)
        {
            Type elementType = type;
            if (type.IsByRef)
            {
                elementType = type.GetElementType();
            }

            //如果是void类型，返回null
            if (type == typeof(void)) return null;

            return typeof(CoreHelper).GetMethod("DefaultValue", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(elementType).Invoke(null, null);
        }

        /// <summary>
        /// Defaults the value.
        /// </summary>
        /// <returns></returns>
        private static object DefaultValue<MemberType>()
        {
            return default(MemberType);
        }

        /// <summary>
        /// 移除多余的空格，保留一个空格
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveSurplusSpaces(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            RegexOptions opt = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", opt);
            string str = regex.Replace(value, " ").Trim();

            return str;
        }

        /// <summary>
        /// 检测是否结构类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CheckStructType(object value)
        {
            return CheckStructType(value.GetType());
        }

        /// <summary>
        /// 检测是否结构类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckStructType(Type type)
        {
            if (type.IsValueType && !type.IsEnum && !type.IsPrimitive && string.Compare(type.Namespace, "system", true) != 0)
            {
                return true;
            }
            return false;
        }

        #region 对象克隆

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object CloneObject(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }

        #endregion

        #region DynamicCalls

        /// <summary>
        /// 快速创建一个T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>()
        {
            return CreateInstance<T>(typeof(T));
        }

        /// <summary>
        /// 快速创建一个T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(Type type)
        {
            if (!type.IsPublic)
                return (T)Activator.CreateInstance(type);
            else
                return (T)GetFastInstanceCreator(type)();
        }

        /// <summary>
        /// 创建一个委托
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FastCreateInstanceHandler GetFastInstanceCreator(Type type)
        {
            if (type.IsInterface)
            {
                throw new MySoftException("可实例化的对象类型不能是接口！");
            }
            FastCreateInstanceHandler creator = DynamicCalls.GetInstanceCreator(type);
            return creator;
        }

        /// <summary>
        /// 快速调用方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static FastInvokeHandler GetFastMethodInvoke(MethodInfo method)
        {
            FastInvokeHandler invoke = DynamicCalls.GetMethodInvoker(method);
            return invoke;
        }

        #endregion

        #region 属性赋值及取值

        /// <summary>
        /// 快速设置属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, PropertyInfo property, object value)
        {
            if (obj == null) return;
            if (!property.CanWrite) return;
            try
            {
                FastPropertySetHandler setter = DynamicCalls.GetPropertySetter(property);
                value = ConvertValue(property.PropertyType, value);
                setter(obj, value);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 快速设置属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            if (obj == null) return;
            PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property != null)
            {
                SetPropertyValue(obj, property, value);
            }
        }

        /// <summary>
        /// 快速获取属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, PropertyInfo property)
        {
            if (obj == null) return null;
            if (!property.CanRead) return null;
            try
            {
                FastPropertyGetHandler getter = DynamicCalls.GetPropertyGetter(property);
                return getter(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 快速获取属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return GetPropertyValue(obj, property);
            }
            return null;
        }

        #endregion

        #region 值转换

        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertValue<T>(object value)
        {
            if (value == DBNull.Value || value == null)
                return default(T);

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                object obj = ConvertValue(typeof(T), value);
                if (obj == null)
                {
                    return default(T);
                }
                return (T)obj;
            }
        }

        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ConvertValue(Type type, object value)
        {
            if (value == DBNull.Value || value == null)
                return null;

            if (CoreHelper.CheckStructType(type))
            {
                //如果字段为结构，则进行系列化操作
                return SerializationManager.DeserializeJson(type, value.ToString());
            }
            else
            {
                Type valueType = value.GetType();

                //如果当前值是从类型Type分配
                if (type.IsAssignableFrom(valueType))
                {
                    return value;
                }
                else
                {
                    if (type.IsEnum)
                    {
                        try
                        {
                            return Enum.ToObject(type, value);
                        }
                        catch
                        {
                            return Enum.Parse(type, value.ToString(), true);
                        }
                    }
                    else
                    {
                        return ChangeType(value, type);
                    }
                }
            }
        }

        #endregion

        #region 属性操作

        /// <summary>
        /// Returns the PropertyInfo for properties defined as Instance, Public, NonPublic, or FlattenHierarchy
        /// </summary>
        /// <param retval="type">The type.</param>
        public static PropertyInfo[] GetPropertiesFromType(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        /// <summary>
        /// Returns the PropertyInfo for properties defined as Instance, Public, NonPublic, or FlattenHierarchy
        /// </summary>
        /// <param retval="type">The type.</param>
        public static PropertyInfo[] GetPropertiesFromType<T>()
        {
            return GetPropertiesFromType(typeof(T));
        }

        /// <summary>
        /// 获取所有方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsFromType(Type type)
        {
            return type.AllMethods().ToArray();
        }

        /// <summary>
        /// 获取所有方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsFromType<T>()
        {
            return typeof(T).AllMethods().ToArray();
        }

        /// <summary>
        /// 从类型中获取方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodFromType<T>(string methodName)
        {
            return GetMethodFromType(typeof(T), methodName);
        }

        /// <summary>
        /// 从类型中获取方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodFromType(Type type, string methodName)
        {
            return GetMethodsFromType(type).Where(p => p.ToString() == methodName).FirstOrDefault();
        }

        /// <summary>
        /// 获取自定义属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static T[] GetMemberAttributes<T>(MemberInfo member)
        {
            object[] attrs = member.GetCustomAttributes(typeof(T), false);
            if (attrs != null && attrs.Length > 0)
            {
                return new List<object>(attrs).ConvertAll<T>(p => (T)p).ToArray();
            }
            return null;
        }

        /// <summary>
        /// 获取自定义属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static T GetMemberAttribute<T>(MemberInfo member)
        {
            object[] attrs = member.GetCustomAttributes(typeof(T), false);
            if (attrs != null && attrs.Length > 0)
            {
                return (T)attrs[0];
            }
            return default(T);
        }

        /// <summary>
        /// 获取自定义属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetTypeAttribute<T>(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(T), false);
            if (attrs != null && attrs.Length > 0)
            {
                return (T)attrs[0];
            }
            return default(T);
        }

        /// <summary>
        /// 获取自定义属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T[] GetTypeAttributes<T>(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(T), false);
            if (attrs != null && attrs.Length > 0)
            {
                return new List<object>(attrs).ConvertAll<T>(p => (T)p).ToArray();
            }
            return null;
        }

        #endregion

        #region 数据转换

        /// <summary>
        /// 将value转换成对应的类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(string value, T defaultValue)
        {
            bool isNullable = false;
            Type conversionType = typeof(T);
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                conversionType = Nullable.GetUnderlyingType(conversionType);
                isNullable = true;
            }

            bool success;
            if (ConverterFactory.Converters.ContainsKey(conversionType))
            {
                //如果转换的值为空并且对象可为空时返回默认值
                if (string.IsNullOrEmpty(value) && isNullable) return defaultValue;

                object obj = ConverterFactory.Converters[conversionType].ConvertTo(value, out success);
                if (success) return (T)obj;
            }

            return defaultValue;
        }

        #endregion

        #region 常用方法

        /// <summary>
        /// Makes a unique key.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public static string MakeUniqueKey(int length, string prefix)
        {
            if (prefix != null)
            {
                //如果传入的前缀长度大于总长度，则抛出错误
                if (prefix.Length >= length)
                {
                    throw new ArgumentException("错误的前缀，传入的前缀长度大于总长度！");
                }
            }

            int prefixLength = prefix == null ? 0 : prefix.Length;

            string chars = "1234567890abcdefghijklmnopqrstuvwxyz";

            StringBuilder sb = new StringBuilder();
            if (prefixLength > 0) sb.Append(prefix);

            int dupCount = 0;
            int preIndex = 0;

            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < length - prefixLength; ++i)
            {
                int index = rnd.Next(0, 35);
                if (index == preIndex)
                {
                    ++dupCount;
                }
                sb.Append(chars[index]);
                preIndex = index;
            }
            if (dupCount >= length - prefixLength - 2)
            {
                rnd = new Random(Guid.NewGuid().GetHashCode());
                return MakeUniqueKey(length, prefix);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        private static object ChangeType(object value, Type conversionType)
        {
            if (value == null) return null;

            bool isNullable = false;
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                conversionType = Nullable.GetUnderlyingType(conversionType);
                isNullable = true;
            }

            //进行字符串类型转换
            if (value.GetType() == typeof(string))
            {
                string data = value.ToString();

                //如果转换的值为空并且对象可为空时返回null
                if (string.IsNullOrEmpty(data) && isNullable) return null;

                bool success;
                value = ConverterFactory.Converters[conversionType].ConvertTo(data, out success);
                if (success)
                    return value;
                else
                    throw new MySoftException(string.Format("【{0}】转换成数据类型【{1}】出错！", value, conversionType.Name));
            }
            else
                return Convert.ChangeType(value, conversionType);
        }

        /// <summary>
        /// 获取指定长度的字符串，按字节长度
        /// </summary>
        /// <param name="p_SrcString"></param>
        /// <param name="p_Length"></param>
        /// <param name="p_TailString"></param>
        /// <returns></returns>
        public static string GetSubString(string p_SrcString, int p_Length, string p_TailString)
        {
            if (string.IsNullOrEmpty(p_SrcString)) return p_SrcString;

            string text = p_SrcString;
            if (p_Length < 0)
            {
                return text;
            }
            byte[] sourceArray = Encoding.Default.GetBytes(p_SrcString);
            if (sourceArray.Length <= p_Length)
            {
                return text;
            }
            int length = p_Length;
            int[] numArray = new int[p_Length];
            byte[] destinationArray = null;
            int num2 = 0;
            for (int i = 0; i < p_Length; i++)
            {
                if (sourceArray[i] > 0x7f)
                {
                    num2++;
                    if (num2 == 3)
                    {
                        num2 = 1;
                    }
                }
                else
                {
                    num2 = 0;
                }
                numArray[i] = num2;
            }
            if ((sourceArray[p_Length - 1] > 0x7f) && (numArray[p_Length - 1] == 1))
            {
                length = p_Length + 1;
            }
            destinationArray = new byte[length];
            Array.Copy(sourceArray, destinationArray, length);
            return (Encoding.Default.GetString(destinationArray) + p_TailString);
        }

        #endregion

        #region 简单加密解密

        private static byte[] Keys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };

        /// <summary>
        /// 对字符串进行加密
        /// </summary>
        /// <param name="text">待加密的字符串</param>
        /// <returns>string</returns>
        public static string Encrypt(string text, string key)
        {
            try
            {
                key = key.PadRight(32, ' ');
                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = Encoding.UTF8.GetBytes(key);
                rijndaelProvider.IV = Keys;
                ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();

                byte[] inputData = Encoding.UTF8.GetBytes(text);
                byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                return Convert.ToBase64String(encryptedData);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 对字符串进行解密
        /// </summary>
        /// <param name="text">已加密的字符串</param>
        /// <returns></returns>
        public static string Decrypt(string text, string key)
        {
            try
            {
                key = key.PadRight(32, ' ');
                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = Encoding.UTF8.GetBytes(key);
                rijndaelProvider.IV = Keys;
                ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();

                byte[] inputData = Convert.FromBase64String(text);
                byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 比较两个值的大小
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static int Compare<T>(T value1, T value2)
        {
            try
            {
                int ret = 0;

                if (value1 == null && value2 == null) ret = 0;
                else if (value1 == null) ret = -1;
                else if (value2 == null) ret = 1;
                else if (value1.GetType().IsGenericType && value1.GetType().GetGenericTypeDefinition() == typeof(Nullable<>)
                    && value2.GetType().IsGenericType && value2.GetType().GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    //如果是Nullable<>类型，需要特殊处理
                    Type type1 = Nullable.GetUnderlyingType(value1.GetType());
                    Type type2 = Nullable.GetUnderlyingType(value2.GetType());
                    value1 = (T)Convert.ChangeType(value1, type1);
                    value2 = (T)Convert.ChangeType(value2, type2);
                    ret = ((IComparable)value1).CompareTo((IComparable)value2);
                }
                else
                {
                    ret = ((IComparable)value1).CompareTo((IComparable)value2);
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new MySoftException("比较两个值大小时发生错误：" + ex.Message, ex);
            }
        }

        #endregion
    }
}
