
namespace MySoft.IoC
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public class ServiceConfig
    {
        #region Const Members

        /// <summary>
        /// The default timeout number. 
        /// </summary>
        public const int DEFAULT_TIMEOUT_NUMBER = 60; //60秒

        /// <summary>
        /// The default record number.
        /// </summary>
        public const int DEFAULT_RECORD_NUMBER = 3600; //3600次

        /// <summary>
        /// 默认为5分钟
        /// </summary>
        public const int DEFAULT_CLIENT_TIMEOUT = 60 * 5;

        /// <summary>
        /// The default pool number.
        /// </summary>
        public const int DEFAULT_CLIENTPOOL_MAXNUMBER = 10;

        #endregion
    }
}
