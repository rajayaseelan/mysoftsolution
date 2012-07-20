using System;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    [Serializable]
    public class ScsClientMessage : ScsMessage
    {
        private AppClient client;
        /// <summary>
        /// 客户端信息
        /// </summary>
        public AppClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// 客户端信息
        /// </summary>
        /// <param name="client"></param>
        public ScsClientMessage(AppClient client)
        {
            this.client = client;
        }
    }
}
