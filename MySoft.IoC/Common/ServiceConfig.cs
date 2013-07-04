
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
        public const int DEFAULT_RECORD_HOUR = 24; //24小时

        /// <summary>
        /// The server default max caller
        /// </summary>
        public const int DEFAULT_SERVER_MAXCALLER = 30; //默认并发数30

        /// <summary>
        /// The default client call timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 30; //30秒

        /// <summary>
        /// The default server call timeout number. 
        /// </summary>
        public const int DEFAULT_SERVER_TIMEOUT = 15; //15秒

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENT_MAXPOOL = 100; //默认为100

        /// <summary>
        /// The current framework version.
        /// </summary>
        public const string CURRENT_FRAMEWORK_VERSION = "v4.0"; //当前版本

        #endregion
    }
}