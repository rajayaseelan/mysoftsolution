using MySoft.IoC.Communication.Scs.Communication.Protocols;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 自定义工厂类
    /// </summary>
    public class CustomWireProtocolFactory : IScsWireProtocolFactory
    {
        private bool compress;

        public CustomWireProtocolFactory(bool compress)
        {
            this.compress = compress;
        }

        #region IScsWireProtocolFactory 成员

        /// <summary>
        /// 创建WireProtocol
        /// </summary>
        /// <returns></returns>
        public IScsWireProtocol CreateWireProtocol()
        {
            return new CustomWireProtocol(compress);
        }

        #endregion
    }
}
