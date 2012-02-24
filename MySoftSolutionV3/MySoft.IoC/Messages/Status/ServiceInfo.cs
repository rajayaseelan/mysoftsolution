using System;
using System.Collections.Generic;

namespace MySoft.IoC.Messages
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
        /// 服务全称
        /// </summary>
        public string FullName { get; set; }

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
        /// 服务全称
        /// </summary>
        public string FullName { get; set; }

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
        public string TypeName { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string TypeFullName { get; set; }

        /// <summary>
        /// 是否引用类型
        /// </summary>
        public bool IsByRef { get; set; }

        /// <summary>
        /// 是否输出类型
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// 是否枚举
        /// </summary>
        public bool IsEnum { get; set; }

        /// <summary>
        /// 是否简单类型
        /// </summary>
        public bool IsPrimitive { get; set; }

        /// <summary>
        /// 枚举值
        /// </summary>
        public IList<EnumInfo> EnumValue { get; set; }

        /// <summary>
        /// 子参数信息
        /// </summary>
        public IList<ParameterInfo> SubParameters { get; set; }

        public ParameterInfo()
        {
            this.SubParameters = new List<ParameterInfo>();
            this.EnumValue = new List<EnumInfo>();
        }
    }

    /// <summary>
    /// 枚举信息
    /// </summary>
    [Serializable]
    public class EnumInfo
    {
        /// <summary>
        /// 枚举名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 枚举值
        /// </summary>
        public int Value { get; set; }
    }
}
