
using MySoft.IoC.Messages;
using System;
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
        public const int DEFAULT_MINUTE_CALL = 1000; //1000次

        /// <summary>
        /// The default client timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 60; //60秒

        /// <summary>
        /// The default server timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_TIMEOUT = 30; //30秒

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
        /// 创建一个Hashtable
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
            var methodKey = string.Format("{0}_{1}_{2}", serviceType.FullName, method.ToString(), collection.ToString());
            return string.Format("CastleCache_{0}", methodKey).ToLower();
        }
    }
}
