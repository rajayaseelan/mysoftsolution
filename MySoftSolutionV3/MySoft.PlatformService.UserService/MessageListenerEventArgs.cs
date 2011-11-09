using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.PlatformService.UserService
{
    class MessageListenerEventArgs : EventArgs
    {
        public MessageListener Listener { get; private set; }

        public MessageListenerEventArgs(MessageListener listener)
        {
            this.Listener = listener;
        }
    }
}
