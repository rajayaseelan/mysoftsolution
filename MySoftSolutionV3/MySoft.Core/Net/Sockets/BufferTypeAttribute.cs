/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2010-12-26 
 */
using System;

namespace MySoft.Net.Sockets
{
    /// <summary>
    /// 数据包格式化类
    /// （凡是打了此标记的类才能够被 BufferFormat.FormatFCA 处理)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BufferTypeAttribute : Attribute
    {
        /// <summary>
        /// 数据包命令类型
        /// </summary>
        public int BufferCmdType { get; set; }

        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="bufferCmdType">数据包命令类型</param>
        public BufferTypeAttribute(int bufferCmdType)
        {
            this.BufferCmdType = bufferCmdType;
        }
    }
}
