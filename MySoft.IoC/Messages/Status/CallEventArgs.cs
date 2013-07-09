using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 基类
    /// </summary>
    [Serializable]
    public abstract class BaseEventArgs : EventArgs
    {
        /// <summary>
        /// 调用参数信息
        /// </summary>
        public AppCaller Caller { get; internal set; }

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
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get { return this.Error != null; }
        }
    }

    /// <summary>
    /// 调用事件参数
    /// </summary>
    [Serializable]
    public class CallEventArgs : BaseEventArgs
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 实例化CallEventArgs
        /// </summary>
        public CallEventArgs(AppCaller appCaller)
        {
            this.Caller = appCaller;
        }
    }
}
