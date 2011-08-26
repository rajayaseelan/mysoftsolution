using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
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
                lock (this)
                {
                    runningSeconds = value;
                }
            }
        }

        /// <summary>
        /// 平均数据流量（每秒）
        /// </summary>
        public new double AverageDataFlow
        {
            get
            {
                if (runningSeconds > 0)
                    return Math.Round((base.DataFlow * 1.0) / (runningSeconds * 1.0), 4);
                else
                    return 0;
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
                    return Math.Round((base.RequestCount * 1.0) / (runningSeconds * 1.0), 4);
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
                    return Math.Round((base.SuccessCount * 1.0) / (runningSeconds * 1.0), 4);
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
                    return Math.Round((base.ErrorCount * 1.0) / (runningSeconds * 1.0), 4);
                else
                    return 0;
            }
        }
    }
}
