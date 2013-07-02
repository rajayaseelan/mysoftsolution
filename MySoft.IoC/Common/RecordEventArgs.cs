using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC.Logger
{
    /// <summary>
    /// 记录事件
    /// </summary>
    [Serializable]
    public class RecordEventArgs : BaseEventArgs
    {
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ServerHostName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string ServerIPAddress { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// 实例化RecordEventArgs
        /// </summary>
        public RecordEventArgs(AppCaller caller)
        {
            this.Caller = caller;
        }
    }
}
