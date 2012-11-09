using System;

namespace MySoft.Web
{
    /// <summary>
    /// 更新项
    /// </summary>
    [Serializable]
    public class UpdateItem
    {
        /// <summary>
        /// 是否更新成功
        /// </summary>
        public bool UpdateSuccess { get; set; }

        /// <summary>
        /// 动态Url
        /// </summary>
        public string DynamicUrl { get; set; }

        /// <summary>
        /// 静态Url
        /// </summary>
        public string StaticPath { get; set; }

        /// <summary>
        /// 静态路径（不包含绝对路径）
        /// </summary>
        public string StaticUrl
        {
            get
            {
                if (string.IsNullOrEmpty(StaticPath))
                    return StaticPath;
                else
                    return StaticPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "/").Replace("\\", "/").Replace("//", "/");
            }
        }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path
        {
            get
            {
                return DynamicUrl.Split('?')[0];
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        public string Query
        {
            get
            {
                var arr = DynamicUrl.Split('?');
                if (arr.Length > 1)
                    return arr[1];
                else
                    return null;
            }
        }
    }
}
