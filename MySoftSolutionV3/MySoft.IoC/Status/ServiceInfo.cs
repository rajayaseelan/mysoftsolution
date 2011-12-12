using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 服务情况
    /// </summary>
    [Serializable]
    public class ServiceInfo
    {
        /// <summary>
        /// 程序集
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 方法信息
        /// </summary>
        public IList<MethodInfo> Methods { get; set; }

        public ServiceInfo()
        {
            this.Methods = new List<MethodInfo>();
        }
    }

    /// <summary>
    /// 方法信息
    /// </summary>
    [Serializable]
    public class MethodInfo
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public IList<ParameterInfo> Parameters { get; set; }

        public MethodInfo()
        {
            this.Parameters = new List<ParameterInfo>();
        }
    }

    /// <summary>
    /// 参数信息
    /// </summary>
    [Serializable]
    public class ParameterInfo
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string Type { get; set; }
    }
}
