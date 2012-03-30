using System;
using System.Linq;
using MySoft.IoC.Messages;
using Newtonsoft.Json.Linq;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public class ServiceConfig
    {
        #region Const Members

        /// <summary>
        /// The default record hour
        /// </summary>
        public const int DEFAULT_RECORD_HOUR = 12; //12小时

        /// <summary>
        /// The default minute call number.
        /// </summary>
        public const int DEFAULT_MINUTE_CALL = 100; //100次

        /// <summary>
        /// The default client timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 5 * 60; //60秒

        /// <summary>
        /// The default server timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_TIMEOUT = 2 * 60; //60秒

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MAXPOOL = 100; //默认为100

        #endregion

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="collection"></param>
        internal static void SetParameterValue(System.Reflection.MethodInfo method, object[] parameters, ParameterCollection collection)
        {
            var index = 0;
            foreach (var p in method.GetParameters())
            {
                if (p.ParameterType.IsByRef)
                {
                    //给参数赋值
                    parameters[index] = collection[p.Name];
                }

                index++;
            }
        }

        /// <summary>
        /// 创建一个ParameterCollection
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal static ParameterCollection CreateParameters(System.Reflection.MethodInfo method, object[] parameters)
        {
            var collection = new ParameterCollection();
            int index = 0;
            foreach (var p in method.GetParameters())
            {
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
        internal static string GetCacheKey(Type serviceType, System.Reflection.MethodInfo method, ParameterCollection collection)
        {
            var opContract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
            if (opContract != null && !string.IsNullOrEmpty(opContract.CacheKey))
            {
                string cacheKey = opContract.CacheKey;
                foreach (var key in collection.Keys)
                {
                    string name = "{" + key + "}";
                    if (cacheKey.Contains(name))
                    {
                        var parameter = collection[key];
                        if (parameter != null)
                            cacheKey = cacheKey.Replace(name, parameter.ToString());
                    }
                }

                return string.Format("{0}_{1}", serviceType.FullName, cacheKey);
            }

            //返回默认的缓存key
            var jsonString = ServiceConfig.FormatJson(collection.ToString());
            var methodKey = string.Format("{0}_{1}_{2}", serviceType.FullName, method.ToString(), jsonString);
            return string.Format("CastleCache_{0}", methodKey).ToLower();
        }

        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="resMsg"></param>
        /// <param name="pis"></param>
        internal static void ParseParameter(string jsonString, ResponseMessage resMsg, System.Reflection.ParameterInfo[] pis)
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                JObject obj = JObject.Parse(jsonString);
                if (obj.Count > 0)
                {
                    foreach (var info in pis)
                    {
                        var property = obj.Properties().SingleOrDefault(p => string.Compare(p.Name, info.Name, true) == 0);
                        if (property != null)
                        {
                            //获取Json值
                            string value = property.Value.ToString(Newtonsoft.Json.Formatting.None);
                            object jsonValue = CoreHelper.ConvertValue(info.ParameterType, value);
                            resMsg.Parameters[info.Name] = jsonValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 格式化Json
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        internal static string FormatJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return jsonString;

            return jsonString.Replace(" ", "").Replace("\r\n", "");
        }
    }
}
