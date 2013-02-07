using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 服务器状态信息
    /// </summary>
    [Serializable]
    public class ServerStatus
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 统计小时数
        /// </summary>
        public int TotalHours { get; set; }

        /// <summary>
        /// 汇总状态信息
        /// </summary>
        public SummaryStatus Summary { get; set; }

        /// <summary>
        /// 最高状态信息
        /// </summary>
        public HighestStatus Highest { get; set; }

        /// <summary>
        /// 最新状态信息
        /// </summary>
        public TimeStatus Latest { get; set; }
    }
}
