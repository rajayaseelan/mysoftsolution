using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存对象
    /// </summary>
    [Serializable]
    public sealed class CacheObject
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 参数集合
        /// </summary>
        public ParameterCollection Parameters { get; set; }
    }
}
