using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Communication.Scs.Communication.Protocols.BinarySerialization;
using MySoft.Communication.Scs.Communication.Messages;
using MySoft.Security;
using MySoft.IoC.Services;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 自定义系列化协议
    /// </summary>
    public class CustomWireProtocol : BinarySerializationProtocol
    {
        private bool compress;
        private bool encrypt;
        private byte[] keys;

        /// <summary>
        /// 实例化CustomBinarySerializationProtocol
        /// </summary>
        /// <param name="compress"></param>
        /// <param name="encrypt"></param>
        public CustomWireProtocol(bool compress, bool encrypt)
        {
            this.compress = compress;
            this.encrypt = encrypt;

            if (encrypt)
            {
                //获取加密的字符串
                var encryptString = BigInteger.GenerateRandom(128).ToString();
                keys = MD5.Hash(Encoding.UTF8.GetBytes(encryptString));
            }
        }

        protected override byte[] SerializeMessage(IScsMessage message)
        {
            var bytes = base.SerializeMessage(message);
            if (compress) bytes = CompressionManager.CompressGZip(bytes);
            if (encrypt) bytes = XXTEA.Encrypt(bytes, keys);

            return bytes;
        }

        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            if (encrypt) bytes = XXTEA.Decrypt(bytes, keys);
            if (compress) bytes = CompressionManager.DecompressGZip(bytes);

            return base.DeserializeMessage(bytes);
        }
    }
}
