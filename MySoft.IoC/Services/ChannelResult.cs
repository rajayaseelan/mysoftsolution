using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Communication.Scs.Communication;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 通道等待响应
    /// </summary>
    internal class ChannelResult : WaitResult
    {
        private IScsServerClient channel;

        /// <summary>
        /// 通道
        /// </summary>
        public IScsServerClient Channel
        {
            get { return channel; }
        }

        private IDataContext context;

        /// <summary>
        /// 上下文
        /// </summary>
        public IDataContext Context
        {
            get { return context; }
        }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool Completed
        {
            get
            {
                if (channel == null) return true;
                if (channel.CommunicationState != CommunicationStates.Connected) return true;

                return false;
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

            base.Dispose();
        }
    }
}
