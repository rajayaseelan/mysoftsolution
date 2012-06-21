using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft
{
    /// <summary>
    /// compress Data Type
    /// </summary>
    public enum CompressType
    {
        /// <summary>
        /// 不压缩
        /// </summary>
        None,
        /// <summary>
        /// GZip压缩
        /// </summary>
        GZip,
        /// <summary>
        /// Deflate压缩
        /// </summary>
        Deflate,
        /// <summary>
        /// 7Zip压缩
        /// </summary>
        SevenZip,
        /// <summary>
        /// SharpZip压缩
        /// </summary>
        SharpZip
    }
}
