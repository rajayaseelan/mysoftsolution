/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com QQ:547386448
 *  Updated 2011-04-9
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft.Net.Sockets
{
    /// <summary>
    /// 数据包格式化类
    /// (此类功能是讲.NET数据转换成通讯数据包）
    /// </summary>
    public class BufferFormat
    {
        private List<byte> buffList;
        private bool finish;

        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="buffType">包类型</param>
        public BufferFormat(int buffType)
        {
            buffList = new List<byte>();
            buffList.AddRange(GetSocketBytes(buffType));
            finish = false;
        }

        #region 布尔值
        /// <summary>
        /// 添加一个布尔值
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(bool data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }

        #endregion

        #region 整数
        /// <summary>
        /// 添加一个1字节的整数
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(byte data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Add(data);
        }

        /// <summary>
        /// 添加一个2字节的整数
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(Int16 data)
        {
            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个4字节的整数
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(Int32 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个8字节的整数
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(UInt64 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }

        #endregion

        #region 浮点数

        /// <summary>
        /// 添加一个4字节的浮点
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(float data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个8字节的浮点
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(double data)
        {
            buffList.AddRange(GetSocketBytes(data));
        }

        #endregion

        #region 数据包

        /// <summary>
        /// 添加一个BYTE[]数据包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void AddItem(Byte[] data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            byte[] ldata = GetSocketBytes(data.Length);
            buffList.AddRange(ldata);
            buffList.AddRange(data);

        }

        #endregion

        #region 字符串
        /// <summary>
        /// 添加一个字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void AddItem(String data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            Byte[] bytes = Encoding.Unicode.GetBytes(data);
            buffList.AddRange(GetSocketBytes(bytes.Length));
            buffList.AddRange(bytes);

        }

        #endregion

        #region 时间
        /// <summary>
        /// 添加一个一个DATATIME
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void AddItem(DateTime data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            AddItem(data.ToString());
        }

        #endregion

        #region 对象
        /// <summary>
        /// 将一个对象转换为二进制数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void AddItem(object obj)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            byte[] data = SerializeObject(obj);
            buffList.AddRange(GetSocketBytes(data.Length));
            buffList.AddRange(data);
        }

        #endregion

        /// <summary>
        /// 完毕
        /// </summary>
        /// <returns></returns>
        public byte[] Finish()
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            int l = buffList.Count + 4;
            byte[] data = GetSocketBytes(l);
            for (int i = data.Length - 1; i >= 0; i--)
            {
                buffList.Insert(0, data[i]);
            }

            finish = true;
            return buffList.ToArray();
        }

        #region 系列化数据

        public static byte[] FormatFCA(DataPacket o)
        {
            if (o == null || o.PacketObject == null) return new byte[0];

            List<byte> bufflist = new List<byte>();

            //将包ID加入到数据包中
            bufflist.AddRange(o.PacketID.ToByteArray());

            //系列化后的数据包
            bufflist.AddRange(SerializeObject(o.PacketObject));

            //插入数据包大小
            int l = bufflist.Count + 4;
            byte[] data = GetSocketBytes(l);
            for (int i = data.Length - 1; i >= 0; i--)
            {
                bufflist.Insert(0, data[i]);
            }

            return bufflist.ToArray();
        }

        #endregion

        #region V整数

        /// <summary>
        /// 将一个32位整形转换成一个BYTE[]4字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(Int32 data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个64位整形转换成一个BYTE[]8字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(UInt64 data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个 1位CHAR转换成1位的BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(Char data)
        {
            Byte[] bytes = new Byte[] { (Byte)data };
            return bytes;
        }

        /// <summary>
        /// 将一个 16位整数转换成2位的BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(Int16 data)
        {
            return BitConverter.GetBytes(data);
        }

        #endregion

        #region V布尔值

        /// <summary>
        /// 将一个布尔值转换成一个BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(bool data)
        {
            return BitConverter.GetBytes(data);
        }

        #endregion

        #region V浮点数

        /// <summary>
        /// 将一个32位浮点数转换成一个BYTE[]4字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(float data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个64位浮点数转换成一个BYTE[]8字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(double data)
        {
            return BitConverter.GetBytes(data);
        }

        #endregion


        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        public static byte[] SerializeObject(object pObj)
        {
            return SerializationManager.SerializeBin(pObj);
        }
    }
}
