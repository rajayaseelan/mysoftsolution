using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MySoft.IoC.Status
{
    /// <summary>
    /// 服务信息
    /// </summary>
    [Serializable]
    public class ServiceInfo
    {
        private string assembly;
        /// <summary>
        /// 程序集
        /// </summary>
        public string Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }

        private string name;
        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private MethodInfo[] methods;
        /// <summary>
        /// 服务方法
        /// </summary>
        public MethodInfo[] Methods
        {
            get { return methods; }
            set { methods = value; }
        }
    }
}
