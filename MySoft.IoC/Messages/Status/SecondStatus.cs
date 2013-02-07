using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 每秒服务器状态信息
    /// </summary>
    [Serializable]
    public abstract class SecondStatus
    {
        /// <summary>
        /// 请求数
        /// </summary>
        public int RequestCount
        {
            get
            {
                return SuccessCount + ErrorCount;
            }
        }

        /// <summary>
        /// 成功计数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 错误数
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public long ElapsedTime { get; set; }

        /// <summary>
        /// 平均耗时（每次请求）
        /// </summary>
        public double AverageElapsedTime
        {
            get
            {
                if (this.RequestCount > 0)
                    return Math.Round((ElapsedTime * 1.0) / (RequestCount * 1.0), 4);
                else
                    return 0;
            }
        }
    }
}
