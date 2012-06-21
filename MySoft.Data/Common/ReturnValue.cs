using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// 返回数据
    /// </summary>
    [Serializable]
    public sealed class ReturnValue
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 记录数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 是否有异常
        /// </summary>
        public bool IsError
        {
            get
            {
                return this.Error != null;
            }
        }
    }
}
