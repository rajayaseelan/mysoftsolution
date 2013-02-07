
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
        /// The default max caller
        /// </summary>
        public const int DEFAULT_CLIENT_MAXCALLER = 10; //默认为10

        /// <summary>
        /// The default max caller
        /// </summary>
        public const int DEFAULT_SERVER_MAXCALLER = 100; //默认为100

        /// <summary>
        /// The default client  call timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 5 * 60; //5*60秒

        /// <summary>
        /// The default server  call timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_TIMEOUT = 60; //60秒

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MINPOOL = 30; //默认为30

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MAXPOOL = 100; //默认为100

        #endregion
    }
}