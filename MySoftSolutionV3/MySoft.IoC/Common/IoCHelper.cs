using System;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using MySoft.Security;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC
{
    internal static class IoCHelper
    {
        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="collection"></param>
        /// <param name="parameters"></param>
        public static void SetRefParameterValues(System.Reflection.MethodInfo method, ParameterCollection collection, object[] parameters)
        {
            var index = 0;
            foreach (var p in method.GetParameters())
            {
                //给参数赋值
                if (p.ParameterType.IsByRef)
                    parameters[index] = collection[p.Name];

                index++;
            }
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="reqMsg"></param>
        public static object[] CreateParameterValues(System.Reflection.MethodInfo method, ParameterCollection collection)
        {
            var index = 0;
            var pis = method.GetParameters();
            var parameters = new object[pis.Length];
            foreach (var p in pis)
            {
                //给参数赋值
                if (collection[p.Name] == null)
                    parameters[index] = CoreHelper.GetTypeDefaultValue(p.ParameterType);
                else
                    parameters[index] = collection[p.Name];

                index++;
            }

            return parameters;
        }

        /// <summary>
        /// 设置ParameterCollection值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="collection"></param>
        /// <param name="parameters"></param>
        public static void SetRefParameters(System.Reflection.MethodInfo method, ParameterCollection collection, object[] parameters)
        {
            int index = 0;
            foreach (var p in method.GetParameters())
            {
                if (p.ParameterType.IsByRef)
                    collection[p.Name] = parameters[index];

                index++;
            }
        }

        /// <summary>
        /// 创建一个ParameterCollection
        /// </summary>
        /// <param name="method"></param>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static ParameterCollection CreateParameters(System.Reflection.MethodInfo method, string jsonString)
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                JObject obj = JObject.Parse(jsonString);
                if (obj.Count > 0)
                {
                    var pis = method.GetParameters();
                    object[] parameters = new object[pis.Length];
                    var index = 0;
                    foreach (var p in pis)
                    {
                        var property = obj.Properties().SingleOrDefault(o => string.Compare(o.Name, p.Name, true) == 0);
                        if (property != null)
                        {
                            //获取Json值
                            string value = property.Value.ToString(Newtonsoft.Json.Formatting.None);
                            object jsonValue = CoreHelper.ConvertJsonValue(p.ParameterType, value);
                            parameters[index] = jsonValue;
                        }

                        index++;
                    }

                    //创建参数集合
                    return CreateParameters(method, parameters);
                }
            }

            //如果json检测不通过，返回空的参数集合
            return new ParameterCollection();
        }

        /// <summary>
        /// 创建一个ParameterCollection
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static ParameterCollection CreateParameters(System.Reflection.MethodInfo method, object[] parameters)
        {
            var collection = new ParameterCollection();
            int index = 0;
            foreach (var p in method.GetParameters())
            {
                if (parameters[index] == null)
                    collection[p.Name] = CoreHelper.GetTypeDefaultValue(p.ParameterType);
                else
                    collection[p.Name] = parameters[index];

                index++;
            }

            return collection;
        }

        /// <summary>
        /// 获取缓存Key值
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="method"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string GetCacheKey(Type serviceType, System.Reflection.MethodInfo method, ParameterCollection collection)
        {
            var opContract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
            if (opContract != null && !string.IsNullOrEmpty(opContract.CacheKey))
            {
                string cacheKey = opContract.CacheKey;
                var index = 0;
                foreach (var key in collection.Keys)
                {
                    string name = "{" + key + "}";
                    if (cacheKey.Contains(name))
                    {
                        var parameter = collection[key];
                        if (parameter != null)
                            cacheKey = cacheKey.Replace(name, "|" + parameter.ToString() + "|");
                        else
                            cacheKey = cacheKey.Replace(name, "|");
                    }

                    index++;
                }

                return string.Format("{0}_{1}", serviceType.FullName, cacheKey);
            }
            else
            {
                //返回默认的缓存key
                var cacheKey = string.Format("{0}${1}${2}", serviceType.FullName, method.ToString(), collection.ToString());
                return string.Format("CastleCache_{0}", GetMD5String(cacheKey));
            }
        }

        /// <summary>
        /// 获取MD5值
        /// </summary>
        /// <param name="thisKey"></param>
        /// <returns></returns>
        public static string GetMD5String(string thisKey)
        {
            if (string.IsNullOrEmpty(thisKey))
            {
                return thisKey;
            }

            var formatKey = thisKey.Replace(" ", "").Replace("\r\n", "");
            return MD5.HexHash(Encoding.Default.GetBytes(formatKey));
        }
    }
}
