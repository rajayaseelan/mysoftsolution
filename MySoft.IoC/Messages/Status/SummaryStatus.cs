using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 汇总状态信息
    /// </summary>
    [Serializable]
    public class SummaryStatus : SecondStatus
    {
        private int runningSeconds;
        /// <summary>
        /// 运行总时间
        /// </summary>
        public int RunningSeconds
        {
            get
            {
                return runningSeconds;
            }
            set
            {
                runningSeconds = value;
            }
        }

        private int requestCount;
        /// <summary>
        /// 请求数
        /// </summary>
        public new int RequestCount
        {
            get
            {
                return requestCount;
            }
            set
            {
                requestCount = value;
            }
        }

        /// <summary>
        /// 平均请求数（每秒）
        /// </summary>
        public double AverageRequestCount
        {
            get
            {
                if (runningSeconds > 0)
                    return Math.Round((this.RequestCount * 1.0) / (runningSeconds * 1.0), 4);
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
                if (runningSeconds > 0)
                    return Math.Round((this.SuccessCount * 1.0) / (runningSeconds * 1.0), 4);
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
                if (runningSeconds > 0)
                    return Math.Round((this.ErrorCount * 1.0) / (runningSeconds * 1.0), 4);
                else
                    return 0;
            }
        }
    }
}
