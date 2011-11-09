using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.PlatformService.UserService
{
    class MessageListener
    {
        public string FromIP { get; private set; }

        public int FromPort { get; private set; }

        private IMessageListener _innerListener;

        public MessageListener(string fromIP, int fromPort, IMessageListener innerListener)
        {
            this.FromIP = fromIP;
            this.FromPort = fromPort;
            _innerListener = innerListener;
        }

        /// <summary>
        /// 通知消息；
        /// </summary>
        /// <param name="message"></param>
        public void Notify(string message)
        {
            _innerListener.Publish(message);
        }

        public override bool Equals(object obj)
        {
            bool eq = base.Equals(obj);
            if (!eq)
            {
                MessageListener lstn = obj as MessageListener;
                if (lstn.FromIP.Equals(this.FromIP) && lstn.FromPort.Equals(this.FromPort))
                {
                    eq = true;
                }
            }
            return eq;
        }
    }
}
