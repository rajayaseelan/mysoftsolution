using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Communication.Protocols.BinarySerialization;
using MySoft.IoC.Services;
using MySoft.Security;
using System.Runtime.Serialization;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 自定义系列化协议
    /// </summary>
    public class CustomWireProtocol : BinarySerializationProtocol
    {
        private bool compress;

        /// <summary>
        /// 实例化CustomWireProtocol
        /// </summary>
        /// <param name="compress"></param>
        public CustomWireProtocol(bool compress)
        {
            this.compress = compress;
        }

        /// <summary>
        /// 序列化流
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override byte[] SerializeMessage(IScsMessage message)
        {
            var buffer = base.SerializeMessage(message);
            if (compress)
            {
                buffer = CompressionManager.CompressSharpZip(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// 反序列化流
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected override IScsMessage DeserializeMessage(byte[] buffer)
        {
            if (compress)
            {
                buffer = CompressionManager.DecompressSharpZip(buffer);
            }

            return base.DeserializeMessage(buffer);
        }
    }
}
