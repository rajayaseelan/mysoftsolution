using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 最高峰状态
    /// </summary>
    [Serializable]
    public class HighestStatus : SecondStatus
    {
        /// <summary>
        /// 最大请求数
        /// </summary>
        public new int RequestCount { get; set; }

        /// <summary>
        /// 最大请求发生时间
        /// </summary>
        public DateTime RequestCountCounterTime { get; set; }

        /// <summary>
        /// 最多成功请求发生时间
        /// </summary>
        public DateTime SuccessCountCounterTime { get; set; }

        /// <summary>
        /// 最多错误请求发生时间
        /// </summary>
        public DateTime ErrorCountCounterTime { get; set; }

        /// <summary>
        /// 最耗时请求发生时间
        /// </summary>
        public DateTime ElapsedTimeCounterTime { get; set; }
    }
}
