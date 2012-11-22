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
        /// <summary>
        /// 客户端信息
        /// </summary>
        public AppClient Client { get; private set; }

        /// <summary>
        /// 客户端信息
        /// </summary>
        /// <param name="client"></param>
        public ScsClientMessage(AppClient client)
        {
            this.Client = client;
        }
    }
}
