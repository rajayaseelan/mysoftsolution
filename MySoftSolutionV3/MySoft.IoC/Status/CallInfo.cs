using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 调用异常信息
    /// </summary>
    [Serializable]
    public class CallError
    {
        /// <summary>
        /// 调用信息
        /// </summary>
        public AppCaller Caller { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime CallTime { get; set; }

        /// <summary>
        /// 是否业务异常
        /// </summary>
        public bool IsBusinessError { get; set; }

        public CallError()
        {
            this.Caller = new AppCaller();
        }
    }

    /// <summary>
    /// 调用超时
    /// </summary>
    [Serializable]
    public class CallTimeout
    {
        /// <summary>
        /// 调用信息
        /// </summary>
        public AppCaller Caller { get; set; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime CallTime { get; set; }

        /// <summary>
        /// 数据数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public long ElapsedTime { get; set; }

        public CallTimeout()
        {
            this.Caller = new AppCaller();
        }
    }
}
