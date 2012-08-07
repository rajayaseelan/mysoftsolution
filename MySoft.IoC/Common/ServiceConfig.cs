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
        public const int DEFAULT_RECORD_HOUR = 3; //3小时

        /// <summary>
        /// The default max call number.
        /// </summary>
        public const int DEFAULT_MAX_CALL = 1000; //1000次

        /// <summary>
        /// The default client timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 2 * 60; //2*60秒

        /// <summary>
        /// The default client timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_TIMEOUT = 5; //5秒

        /// <summary>
        /// The default min timeout number. 
        /// </summary>
        public const int DEFAULT_CALL_TIMEOUT = 30; //30秒

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
