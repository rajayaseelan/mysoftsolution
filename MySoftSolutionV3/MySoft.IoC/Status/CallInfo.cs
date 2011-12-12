using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 连接信息
    /// </summary>
    [Serializable]
    public class ConnectInfo
    {
        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime ConnectTime { get; set; }

        /// <summary>
        /// 本地终结点
        /// </summary>
        public EndPoint LocalEndPoint { get; set; }

        /// <summary>
        /// 远程终结点
        /// </summary>
        public EndPoint RemoteEndPoint { get; set; }

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
        /// 异常类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime CallTime { get; set; }

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
