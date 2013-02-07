using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 汇总状态信息
    /// </summary>
    [Serializable]
    public class SummaryStatus : SecondStatus
    {
        /// <summary>
        /// 运行总时间
        /// </summary>
        public int RunningSeconds { get; set; }

        /// <summary>
        /// 请求数
        /// </summary>
        public new int RequestCount { get; set; }

        /// <summary>
        /// 平均请求数（每秒）
        /// </summary>
        public double AverageRequestCount
        {
            get
            {
                if (RunningSeconds > 0)
                    return Math.Round((RequestCount * 1.0) / (RunningSeconds * 1.0), 4);
                else
                    return 0;
            }
        }

        /// <summary>
        /// 平均成功数（每秒）
        /// </summary>
        public double AverageSuccessCount
        {
            get
            {
                if (RunningSeconds > 0)
                    return Math.Round((SuccessCount * 1.0) / (RunningSeconds * 1.0), 4);
                else
                    return 0;
            }
        }

        /// <summary>
        /// 平均错误数（每秒）
        /// </summary>
        public double AverageErrorCount
        {
            get
            {
                if (RunningSeconds > 0)
                    return Math.Round((ErrorCount * 1.0) / (RunningSeconds * 1.0), 4);
                else
                    return 0;
            }
        }
    }
}
