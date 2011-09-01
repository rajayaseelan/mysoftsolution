using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Security;
using System.Collections;
using System.Data;

namespace MySoft.IoC.Message
{
    /// <summary>
    /// 返回的数据
    /// </summary>
    [Serializable]
    public sealed class ResponseData
    {
        private bool compress = false;
        private bool encrypt = false;
        private byte[] keys;
        private byte[] buffer;
        private object value;
        private int count;

        /// <summary>
        /// 返回的结果
        /// </summary>
        public object Value
        {
            get
            {
                if (value == null)
                {
                    if (buffer == null) return null;

                    #region 处理返回的数据

                    if (compress || encrypt)
                    {
                        //处理是否解密
                        if (encrypt) buffer = XXTEA.Decrypt(buffer, keys);

                        //处理是否压缩
                        if (compress) buffer = CompressionManager.DecompressSharpZip(buffer);

                        //将byte数组反系列化成对象
                        value = SerializationManager.DeserializeBin(buffer);
                    }

                    #endregion
                }

                return value;
            }
        }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// 实例化ResultData
        /// </summary>
        /// <param name="reqBase"></param>
        /// <param name="keys"></param>
        /// <param name="value"></param>
        public ResponseData(MessageBase reqBase, byte[] keys, object value)
        {
            this.compress = reqBase.Compress;
            this.encrypt = reqBase.Encrypt;
            this.keys = keys;

            //初始化值
            Init(value);
        }

        private void Init(object val)
        {
            if (val == null) return;

            #region 处理传入的数据

            this.count = GetCount(val);

            if (compress || encrypt)
            {
                //数据系列化
                this.buffer = SerializationManager.SerializeBin(val);

                //判断是否压缩
                if (compress) this.buffer = CompressionManager.CompressSharpZip(buffer);

                //判断是否加密
                if (encrypt) this.buffer = XXTEA.Encrypt(buffer, keys);
            }
            else
            {
                this.value = val;
            }

            #endregion
        }

        private int GetCount(object val)
        {
            if (val is ICollection)
            {
                return (val as ICollection).Count;
            }
            else if (val is Array)
            {
                return (val as Array).Length;
            }
            else if (val is DataTable)
            {
                return (val as DataTable).Rows.Count;
            }
            else if (val is DataSet)
            {
                var ds = val as DataSet;
                if (ds.Tables.Count > 0)
                {
                    int count = 0;
                    foreach (DataTable table in ds.Tables)
                    {
                        count += table.Rows.Count;
                    }
                    return count;
                }
            }

            return 1;
        }
    }
}
