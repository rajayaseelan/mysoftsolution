
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
        public const double DEFAULT_TIMEOUT_NUMBER = 300; //300秒

        /// <summary>
        /// The default cachetime number.
        /// </summary>
        public const double DEFAULT_CACHETIME_NUMBER = 60; //60秒

        /// <summary>
        /// The default logtime number.
        /// </summary>
        public const double DEFAULT_LOGTIME_NUMBER = 5; //5秒

        /// <summary>
        /// The default record number.
        /// </summary>
        public const int DEFAULT_RECORD_NUMBER = 3600; //3600次

        /// <summary>
        /// The default maxpool number.
        /// </summary>
        public const int DEFAULT_CLIENTPOOL_NUMBER = 50;

        #endregion
    }
}
