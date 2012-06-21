using System;
using System.IO;
using System.IO.Compression;
using SharpZip.Zip.Compression;
using SharpZip.Zip.Compression.Streams;

namespace MySoft
{
    /// <summary>
    /// Compression Manager
    /// </summary>
    public abstract class CompressionManager
    {
        #region SharpZip

        /// <summary>
        /// SharpZip—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] CompressSharpZip(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            using (MemoryStream inStream = new MemoryStream(buffer))
            {
                MemoryStream outStream = new MemoryStream();
                Deflater mDeflater = new Deflater(Deflater.BEST_COMPRESSION);
                DeflaterOutputStream compressStream = new DeflaterOutputStream(outStream, mDeflater);
                int mSize;
                byte[] mWriteData = new Byte[4096];
                while ((mSize = inStream.Read(mWriteData, 0, 4096)) > 0)
                {
                    compressStream.Write(mWriteData, 0, mSize);
                }
                compressStream.Finish();
                inStream.Close();
                return outStream.ToArray();
            }
        }

        /// <summary>
        /// SharpZipΩ‚—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] DecompressSharpZip(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            using (MemoryStream inStream = new MemoryStream(buffer))
            {
                InflaterInputStream unCompressStream = new InflaterInputStream(inStream);
                MemoryStream outStream = new MemoryStream();
                int mSize;
                Byte[] mWriteData = new Byte[4096];
                while ((mSize = unCompressStream.Read(mWriteData, 0, mWriteData.Length)) > 0)
                {
                    outStream.Write(mWriteData, 0, mSize);
                }
                unCompressStream.Close();
                return outStream.ToArray();
            }
        }

        #endregion

        #region GZip

        /// <summary>
        /// GZip—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] CompressGZip(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            MemoryStream ms = new MemoryStream();
            using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
            {
                gzip.Write(buffer, 0, buffer.Length);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// GZipΩ‚—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] DecompressGZip(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            using (MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length))
            {
                MemoryStream msOut = new MemoryStream();
                byte[] writeData = new byte[4096];
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    int n;
                    while ((n = gzip.Read(writeData, 0, writeData.Length)) > 0)
                    {
                        msOut.Write(writeData, 0, n);
                    }
                }
                return msOut.ToArray();
            }
        }

        #endregion

        #region Deflate

        /// <summary>
        /// Deflate—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] CompressDeflate(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            MemoryStream ms = new MemoryStream();
            using (DeflateStream gzip = new DeflateStream(ms, CompressionMode.Compress))
            {
                gzip.Write(buffer, 0, buffer.Length);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// DeflateΩ‚—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] DecompressDeflate(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            using (MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length))
            {
                MemoryStream msOut = new MemoryStream();
                byte[] writeData = new byte[4096];
                using (DeflateStream gzip = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    int n;
                    while ((n = gzip.Read(writeData, 0, writeData.Length)) > 0)
                    {
                        msOut.Write(writeData, 0, n);
                    }
                }
                return msOut.ToArray();
            }
        }

        #endregion

        #region 7Zip

        /// <summary>
        /// 7Zip—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Compress7Zip(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            return SevenZip.Compression.LZMA.SevenZipHelper.Compress(buffer);
        }

        /// <summary>
        /// 7ZipΩ‚—πÀı
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Decompress7Zip(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return buffer;
            }

            return SevenZip.Compression.LZMA.SevenZipHelper.Decompress(buffer);
        }

        #endregion
    }
}
