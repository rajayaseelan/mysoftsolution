
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
        public const int DEFAULT_SERVER_MAXCALLER = 30; //默认为30

        /// <summary>
        /// The default client  call timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 2 * 60; //2*60秒

        /// <summary>
        /// The default server  call timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_TIMEOUT = 60; //60秒

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MINPOOL = 50; //默认为50

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MAXPOOL = 200; //默认为200

        #endregion
    }
}