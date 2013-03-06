using System;

namespace MySoft
{
    /// <summary>
    /// 缓存更新项
    /// </summary>
    [Serializable]
    internal class CacheUpdateItem<T>
    {
        /// <summary>
        /// 缓存类型
        /// </summary>
        public LocalCacheType Type { get; set; }

        /// <summary>
        /// 缓存Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 缓存超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// 执行委托
        /// </summary>
        public Func<object, T> Func { get; set; }

        /// <summary>
        /// 验证函数
        /// </summary>
        public Predicate<T> Pred { get; set; }

        /// <summary>
        /// 状态对象
        /// </summary>
        public object State { get; set; }
    }
}
