using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存更新项
    /// </summary>
    [Serializable]
    internal class CacheUpdateItem
    {
        /// <summary>
        /// 缓存Key
        /// </summary>
        public CacheKey Key { get; set; }

        /// <summary>
        /// 缓存超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// 执行委托
        /// </summary>
        public Func<object, ResponseItem> Func { get; set; }

        /// <summary>
        /// 状态对象
        /// </summary>
        public object State { get; set; }
    }
}
