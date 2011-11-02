using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 调用事件参数
    /// </summary>
    [Serializable]
    public class CallEventArgs : EventArgs
    {
        /// <summary>
        /// 调用消息
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
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 记录数
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception CallError { get; set; }

        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get { return this.CallError != null; }
        }

        public CallEventArgs()
        {
            this.Caller = new AppCaller();
        }
    }

    /// <summary>
    /// 调用消息
    /// </summary>
    [Serializable]
    public class AppCaller : AppClient
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string SubServiceName { get; set; }
    }
}
