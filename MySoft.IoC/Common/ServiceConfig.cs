using System;
using System.Linq;

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
        public const int DEFAULT_RECORD_HOUR = 6; //6小时

        /// <summary>
        /// The default client  call timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_CALL_TIMEOUT = 2 * 60; //2*60秒

        /// <summary>
        /// The default record timeout number. 
        /// </summary>
        public const int DEFAULT_RECORD_TIMEOUT = 2; //2秒

        /// <summary>
        /// The default record count number.
        /// </summary>
        public const int DEFAULT_RECORD_COUNT = 100; //100条

        /// <summary>
        /// The default sync time number.
        /// </summary>
        public const int DEFAULT_SYNC_CACHE_TIME = 5; //5秒

        /// <summary>
        /// The default cache time number.
        /// </summary>
        public const int DEFAULT_DATA_CACHE_TIME = 30 * 60; //30*60秒，默认缓存30分钟

        /// <summary>
        /// The default server call timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_CALL_TIMEOUT = 60; //60秒

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MINPOOL = 10; //默认为10

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MAXPOOL = 100; //默认为100，一般情况下100足矣

        #endregion
    }
}
