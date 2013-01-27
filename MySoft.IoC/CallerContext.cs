using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    internal interface IDataContext
    {
        /// <summary>
        /// Caller信息
        /// </summary>
        AppCaller Caller { get; set; }

        /// <summary>
        /// 请求信息
        /// </summary>
        RequestMessage Request { get; set; }

        /// <summary>
        /// 缓冲数据
        /// </summary>
        byte[] Buffer { get; set; }
    }

    /// <summary>
    /// 调用上下文
    /// </summary>
    internal class CallerContext : IDataContext, IDisposable
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Caller信息
        /// </summary>
        public AppCaller Caller { get; set; }

        /// <summary>
        /// 请求信息
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// 响应信息
        /// </summary>
        public ResponseMessage Message { get; set; }

        /// <summary>
        /// 缓冲数据
        /// </summary>
        public byte[] Buffer { get; set; }

        #region IDisposable 成员

        public void Dispose()
        {
            this.Caller = null;
            this.Request = null;
            this.Message = null;
            this.Buffer = null;
        }

        #endregion
    }
}
