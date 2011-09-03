using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Communication.Scs.Communication.Protocols;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 自定义工厂类
    /// </summary>
    public class CustomWireProtocolFactory : IScsWireProtocolFactory
    {
        private bool compress;
        private bool encrypt;

        public CustomWireProtocolFactory(bool compress, bool encrypt)
        {
            this.compress = compress;
            this.encrypt = encrypt;
        }

        #region IScsWireProtocolFactory 成员

        /// <summary>
        /// 创建WireProtocol
        /// </summary>
        /// <returns></returns>
        public IScsWireProtocol CreateWireProtocol()
        {
            return new CustomWireProtocol(compress, encrypt);
        }

        #endregion
    }
}
