using System;

namespace MySoft.Web
{
    /// <summary>
    /// 为None时配合cookieTime为0使用
    /// </summary>
    [Serializable]
    public enum CookieExpiresType
    {
        /// <summary>
        /// 默认
        /// </summary>
        None,
        /// <summary>
        /// 年
        /// </summary>
        Year,
        /// <summary>
        /// 月
        /// </summary>
        Month,
        /// <summary>
        /// 天
        /// </summary>
        Day,
        /// <summary>
        /// 小时
        /// </summary>
        Hour,
        /// <summary>
        /// 分钟
        /// </summary>
        Minute,
        /// <summary>
        /// 秒
        /// </summary>
        Second
    }
}
