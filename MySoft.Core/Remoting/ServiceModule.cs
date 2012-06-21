using System;
using System.Runtime.Remoting;

namespace MySoft.Remoting
{
    /// <summary>
    /// 业务模块实体类
    /// </summary>
    [Serializable]
    public class ServiceModule
    {
        private WellKnownObjectMode _Mode = WellKnownObjectMode.SingleCall;

        /// <summary>
        /// 对象激活方式（SingleCall或者SingleTon）
        /// </summary>
        public WellKnownObjectMode Mode
        {
            get { return _Mode; }
            set { _Mode = value; }
        }

        private string _Name;

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _AssemblyName;

        /// <summary>
        /// 程序集名称字符串
        /// </summary>
        public string AssemblyName
        {
            get { return _AssemblyName; }
            set { _AssemblyName = value; }
        }

        private string _ClassName;

        /// <summary>
        /// 完整类名
        /// </summary>
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }
    }
}
