using System;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 调用事件参数
    /// </summary>
    [Serializable]
    public class CallEventArgs : EventArgs
    {
        /// <summary>
        /// 调用参数信息
        /// </summary>
        public AppCaller Caller { get; set; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime CallTime { get; set; }

        /// <summary>
        /// 耗时时间
        /// </summary>
        public long ElapsedTime { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 数据数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get { return this.Error != null; }
        }

        /// <summary>
        /// 是否业务异常
        /// </summary>
        public bool IsBusinessError
        {
            get
            {
                return this.Error is BusinessException;
            }
        }

        /// <summary>
        /// 实例化CallEventArgs
        /// </summary>
        public CallEventArgs()
        {
            this.Caller = new AppCaller();
        }
    }
}
