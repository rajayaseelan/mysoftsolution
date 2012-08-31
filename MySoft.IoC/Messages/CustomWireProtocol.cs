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
        /// 实例化CustomBinarySerializationProtocol
        /// </summary>
        /// <param name="compress"></param>
        public CustomWireProtocol(bool compress)
        {
            this.compress = compress;
        }

        protected override byte[] SerializeMessage(IScsMessage message)
        {
            var bytes = base.SerializeMessage(message);
            if (compress) bytes = DeflateCompress(bytes);

            return bytes;
        }

        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
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
