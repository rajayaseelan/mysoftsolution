using System.Threading;
using MySoft.IoC.Communication.Scs.Server;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 通道等待响应
    /// </summary>
    internal class ChannelResult : WaitResult
    {
        private IScsServerClient channel;
        private IDataContext context;

        /// <summary>
        /// 通道
        /// </summary>
        public IScsServerClient Channel
        {
            get { return channel; }
        }

        /// <summary>
        /// 上下文
        /// </summary>
        public IDataContext Context
        {
            get { return context; }
        }

        /// <summary>
        /// 实例化ChannelResult
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="context"></param>
        public ChannelResult(IScsServerClient channel, IDataContext e)
            : base(e.Request)
        {
            this.channel = channel;
            this.context = e;
        }

        public override void Dispose()
        {
            this.channel = null;
            this.context = null;

            base.Dispose();
        }
    }
}
