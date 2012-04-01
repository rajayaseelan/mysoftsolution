using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 连接信息
    /// </summary>
    [Serializable]
    public class ConnectInfo : AppClient
    {
        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime ConnectTime { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 服务端IP
        /// </summary>
        public string ServerIPAddress { get; set; }

        /// <summary>
        /// 服务端Port
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected { get; set; }
    }

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
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// html错误信息
        /// </summary>
        public string HtmlError { get; set; }

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
        /// 总耗时
        /// </summary>
        public long ElapsedTime { get; set; }

        /// <summary>
        /// 数据数
        /// </summary>
        public int Count { get; set; }

        public CallTimeout()
        {
            this.Caller = new AppCaller();
        }
    }
}
