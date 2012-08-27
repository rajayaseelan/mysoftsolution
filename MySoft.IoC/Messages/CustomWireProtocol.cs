using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols.BinarySerialization;
using MySoft.IoC.Services;
using MySoft.Security;

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
            if (compress) bytes = DeflateCompress(bytes);
            if (encrypt) bytes = XXTEA.Encrypt(bytes, keys);

            return bytes;
        }

        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            if (encrypt) bytes = XXTEA.Decrypt(bytes, keys);
            if (compress) bytes = DeflateDecompress(bytes);

            return base.DeserializeMessage(bytes);
        }

        private byte[] DeflateCompress(byte[] bytes)
        {
            var ms = new MemoryStream();
            using (var compressStream = new DeflateStream(ms, CompressionMode.Compress, true))
            {
                compressStream.Write(bytes, 0, bytes.Length);
                compressStream.Close();
            }

            return ms.ToArray();
        }

        private byte[] DeflateDecompress(byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            byte[] newByteArray = new byte[0];
            using (var compressStream = new DeflateStream(ms, CompressionMode.Decompress, false))
            {
                newByteArray = RetrieveBytesFromStream(compressStream);
                compressStream.Close();
            }

            return newByteArray;
        }

        private byte[] RetrieveBytesFromStream(Stream stream)
        {
            List<byte> lst = new List<byte>();
            byte[] data = new byte[1024];
            int totalCount = 0;
            while (true)
            {
                int bytesRead = stream.Read(data, 0, data.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                byte[] buffers = new byte[bytesRead];
                Array.Copy(data, buffers, bytesRead);
                lst.AddRange(buffers);
                totalCount += bytesRead;
            }

            return lst.ToArray();
        }
    }
}
