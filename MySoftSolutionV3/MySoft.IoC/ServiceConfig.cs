
using MySoft.IoC.Messages;
namespace MySoft.IoC
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public class ServiceConfig
    {
        #region Const Members

        /// <summary>
        /// The default record number.
        /// </summary>
        public const int DEFAULT_RECORD_NUMBER = 3600; //3600次

        /// <summary>
        /// The default minute call number.
        /// </summary>
        public const int DEFAULT_MINUTE_CALL_NUMBER = 1000; //1000次

        /// <summary>
        /// The default client timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 60; //60秒

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENTPOOL_MAXNUMBER = 100; //默认为100

        #endregion

        /// <summary>
        /// 获取缓存Key值
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="opContract"></param>
        /// <returns></returns>
        internal static string GetCacheKey(RequestMessage reqMsg, OperationContractAttribute opContract)
        {
            if (opContract != null && !string.IsNullOrEmpty(opContract.CacheKey))
            {
                string cacheKey = opContract.CacheKey;
                foreach (var key in reqMsg.Parameters.Keys)
                {
                    string name = "{" + key + "}";
                    if (cacheKey.Contains(name))
                    {
                        var parameter = reqMsg.Parameters[key];
                        if (parameter != null)
                            cacheKey = cacheKey.Replace(name, parameter.ToString());
                    }
                }

                return string.Format("{0}_{1}", reqMsg.ServiceName, cacheKey);
            }

            return string.Format("CastleCache_{0}_{1}_{2}", reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters);
        }
    }
}
