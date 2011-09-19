using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// 参数信息
    /// </summary>
    [Serializable]
    public class ApiParameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }
    }
}
