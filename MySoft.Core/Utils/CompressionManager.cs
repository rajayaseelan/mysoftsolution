using System.IO;
using System.IO.Compression;
using SevenZip.Compression.LZMA;
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

            MemoryStream outStream = new MemoryStream();
            DeflaterOutputStream compressStream = new DeflaterOutputStream(outStream, new Deflater(Deflater.BEST_COMPRESSION));
            CompressStream(new MemoryStream(buffer), compressStream);
            compressStream.Close();
            outStream.Close();

            return outStream.ToArray();
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

            MemoryStream inStream = new MemoryStream(buffer);
            InflaterInputStream unCompressStream = new InflaterInputStream(inStream);
            MemoryStream outStream = new MemoryStream();
            DecompressStream(unCompressStream, outStream);
            inStream.Close();
            unCompressStream.Close();
            outStream.Close();

            return outStream.ToArray();
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

            MemoryStream outStream = new MemoryStream();
            GZipStream compressStream = new GZipStream(outStream, CompressionMode.Compress, true);
            CompressStream(new MemoryStream(buffer), compressStream);
            compressStream.Close();
            outStream.Close();

            return outStream.ToArray();
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

            MemoryStream inStream = new MemoryStream(buffer);
            GZipStream unCompressStream = new GZipStream(inStream, CompressionMode.Decompress, true);
            MemoryStream outStream = new MemoryStream();
            DecompressStream(unCompressStream, outStream);
            inStream.Close();
            unCompressStream.Close();
            outStream.Close();

            return outStream.ToArray();
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

            MemoryStream outStream = new MemoryStream();
            DeflateStream compressStream = new DeflateStream(outStream, CompressionMode.Compress, true);
            CompressStream(new MemoryStream(buffer), compressStream);
            compressStream.Close();
            outStream.Close();

            return outStream.ToArray();
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

            MemoryStream inStream = new MemoryStream(buffer);
            DeflateStream unCompressStream = new DeflateStream(inStream, CompressionMode.Decompress, true);
            MemoryStream outStream = new MemoryStream();
            DecompressStream(unCompressStream, outStream);
            inStream.Close();
            unCompressStream.Close();
            outStream.Close();

            return outStream.ToArray();
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

            return SevenZipHelper.Compress(buffer);
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

            return SevenZipHelper.Decompress(buffer);
        }

        #endregion

        /// <summary>
        /// —πÀı¡˜
        /// </summary>
        /// <param name="originalStream"></param>
        /// <param name="compressStream"></param>
        private static void CompressStream(Stream originalStream, Stream compressStream)
        {
            BinaryWriter writer = new BinaryWriter(compressStream);
            BinaryReader reader = new BinaryReader(originalStream);
            while (true)
            {
                byte[] buffer = reader.ReadBytes(1024);
                if (buffer == null || buffer.Length < 1)
                    break;
                writer.Write(buffer);
            }
        }

        /// <summary>
        /// Ω‚—πÀı¡˜
        /// </summary>
        /// <param name="compressStream"></param>
        /// <param name="originalStream"></param>
        private static void DecompressStream(Stream compressStream, Stream originalStream)
        {
            BinaryReader reader = new BinaryReader(compressStream);
            BinaryWriter writer = new BinaryWriter(originalStream);
            while (true)
            {
                byte[] buffer = reader.ReadBytes(1024);
                if (buffer == null || buffer.Length < 1)
                    break;
                writer.Write(buffer);
            }
        }
    }
}
