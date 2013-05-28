using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 调用者信息
    /// </summary>
    public class AppCaller
    {
        /// <summary>
        /// 强类型服务
        /// </summary>
        public Type Service { get; set; }

        /// <summary>
        /// 强类型方法
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 强类型参数
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// 应用参数信息
        /// </summary>
        public AppData AppData { get; set; }
    }

    /// <summary>
    /// 应用调用数据
    /// </summary>
    public class AppData
    {
        /// <summary>
        /// 调用分类名称
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// 调用方法名称
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 调用参数信息
        /// </summary>
        public string Parameters { get; set; }
    }
}
