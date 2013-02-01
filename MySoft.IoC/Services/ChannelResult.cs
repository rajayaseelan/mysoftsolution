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
        private Thread thread;

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
        /// 设置线程
        /// </summary>
        /// <param name="thread"></param>
        public void SetThread(Thread thread)
        {
            this.thread = thread;
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        public void Cancel()
        {
            try
            {
                if (thread != null) thread.Abort();
            }
            catch
            {
            }
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
            this.thread = null;

            base.Dispose();
        }
    }
}
