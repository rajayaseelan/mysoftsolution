
namespace MySoft.IoC
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public static class ServiceConfig
    {
        #region Const Members

        /// <summary>
        /// The default record hour
        /// </summary>
        public const int DEFAULT_RECORD_HOUR = 24; //24小时

        /// <summary>
        /// The server default max caller
        /// </summary>
        public const int DEFAULT_SERVER_MAXCALLER = 200; //默认并发数200

        /// <summary>
        /// The client default max caller
        /// </summary>
        public const int DEFAULT_CLIENT_MAXCALLER = 50; //默认并发数50

        /// <summary>
        /// The default client call timeout number. 
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 60; //60秒

        /// <summary>
        /// The current framework version.
        /// </summary>
        public const string CURRENT_FRAMEWORK_VERSION = "v4.2"; //当前版本

        #endregion
    }
}