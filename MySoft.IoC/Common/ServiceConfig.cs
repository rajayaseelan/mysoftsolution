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
        public const int DEFAULT_RECORD_HOUR = 12; //12小时

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
        public const int DEFAULT_CLIENT_MINPOOL = 50; //默认为50

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MAXPOOL = 500; //默认为500，一般情况下500足矣

        #endregion
    }
}
