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

            #region 压缩解压缩

            //bytes = CompressionManager.CompressSharpZip(bytes);
            //bytes = CompressionManager.DecompressSharpZip(bytes);

            //bytes = CompressionManager.Compress7Zip(bytes);
            //bytes = CompressionManager.Decompress7Zip(bytes);

            //bytes = CompressionManager.CompressDeflate(bytes);
            //bytes = CompressionManager.DecompressDeflate(bytes);

            //bytes = CompressionManager.CompressGZip(bytes);
            //bytes = CompressionManager.DecompressGZip(bytes);

            #endregion
        }

        /// <summary>
        /// 序列化流
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override byte[] SerializeMessage(IScsMessage message)
        {
            var bytes = base.SerializeMessage(message);
            if (compress)
            {
                bytes = CompressionManager.CompressGZip(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// 反序列化流
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            if (compress)
            {
                bytes = CompressionManager.DecompressGZip(bytes);
            }

            return base.DeserializeMessage(bytes);
        }
    }
}
