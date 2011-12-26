using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 类型及实例
    /// </summary>
    [Serializable]
    public class TypeInstance
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 对象实例
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// 是否本地
        /// </summary>
        public bool IsLocal { get; set; }
    }
}
