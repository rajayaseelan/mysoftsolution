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
        /// The default cache count
        /// </summary>
        public const int DEFAULT_CACHE_COUNT = 100; //100条记录

        /// <summary>
        /// The default cache timeout
        /// </summary>
        public const int DEFAULT_CACHE_TIMEOUT = 5 * 60; //5分钟

        /// <summary>
        /// The default record hour
        /// </summary>
        public const int DEFAULT_RECORD_HOUR = 6; //6小时

        /// <summary>
        /// The default record timeout number. 
        /// </summary>
        public const int DEFAULT_RECORD_TIMEOUT = 5; //5秒

        /// <summary>
        /// 默认等待5分钟
        /// </summary>
        public const int DEFAULT_WAIT_TIMEOUT = 5 * 60; //5分钟

        /// <summary>
        /// The default client  call timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_CALL_TIMEOUT = 2 * 60; //2*60秒

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
