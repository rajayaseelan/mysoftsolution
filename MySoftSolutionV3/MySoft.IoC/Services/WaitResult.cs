using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    [Serializable]
    public sealed class WaitResult
    {
        private AutoResetEvent reset;
        /// <summary>
        /// 信号量对象
        /// </summary>
        public AutoResetEvent Reset
        {
            get { return reset; }
            set { reset = value; }
        }

        private ResponseMessage message;
        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
