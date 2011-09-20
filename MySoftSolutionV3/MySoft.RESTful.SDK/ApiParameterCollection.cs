using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// 参数集合
    /// </summary>
    [Serializable]
    public class ApiParameterCollection : List<ApiParameter>
    {
        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, object value)
        {
            if (!this.Exists(p => p.Name == name))
                base.Add(new ApiParameter { Name = name, Value = value });
        }
    }
}
