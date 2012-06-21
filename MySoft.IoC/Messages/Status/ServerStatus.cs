using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 服务器状态信息
    /// </summary>
    [Serializable]
    public class ServerStatus
    {
        private DateTime startDate;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
            }
        }

        private int totalHours;
        /// <summary>
        /// 统计小时数
        /// </summary>
        public int TotalHours
        {
            get
            {
                return totalHours;
            }
            set
            {
                totalHours = value;
            }
        }

        private SummaryStatus summary;
        /// <summary>
        /// 汇总状态信息
        /// </summary>
        public SummaryStatus Summary
        {
            get
            {
                return summary;
            }
            set
            {
                summary = value;
            }
        }

        private HighestStatus highest;
        /// <summary>
        /// 最高状态信息
        /// </summary>
        public HighestStatus Highest
        {
            get
            {
                return highest;
            }
            set
            {
                highest = value;
            }
        }

        private TimeStatus latest;
        /// <summary>
        /// 最新状态信息
        /// </summary>
        public TimeStatus Latest
        {
            get
            {
                return latest;
            }
            set
            {
                latest = value;
            }
        }
    }
}
