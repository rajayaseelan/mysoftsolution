using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// 最高峰状态
    /// </summary>
    [Serializable]
    public class HighestStatus : SecondStatus
    {
        private DateTime dataFlowCounterTime;
        /// <summary>
        /// 最高流量发生时间
        /// </summary>
        public DateTime DataFlowCounterTime
        {
            get
            {
                return dataFlowCounterTime;
            }
            set
            {
                dataFlowCounterTime = value;
            }
        }

        private DateTime requestCountCounterTime;
        /// <summary>
        /// 最大请求发生时间
        /// </summary>
        public DateTime RequestCountCounterTime
        {
            get
            {
                return requestCountCounterTime;
            }
            set
            {
                requestCountCounterTime = value;
            }
        }

        private DateTime successCountCounterTime;
        /// <summary>
        /// 最多成功请求发生时间
        /// </summary>
        public DateTime SuccessCountCounterTime
        {
            get
            {
                return successCountCounterTime;
            }
            set
            {
                successCountCounterTime = value;
            }
        }

        private DateTime errorCountCounterTime;
        /// <summary>
        /// 最多错误请求发生时间
        /// </summary>
        public DateTime ErrorCountCounterTime
        {
            get
            {
                return errorCountCounterTime;
            }
            set
            {
                errorCountCounterTime = value;
            }
        }

        private DateTime elapsedTimeCounterTime;
        /// <summary>
        /// 最耗时请求发生时间
        /// </summary>
        public DateTime ElapsedTimeCounterTime
        {
            get
            {
                return elapsedTimeCounterTime;
            }
            set
            {
                elapsedTimeCounterTime = value;
            }
        }
    }
}
