using System;

namespace MySoft.RESTful
{
    /// <summary>
    /// 发布的REST方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PublishMethod : Attribute
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 方法描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 调用方式
        /// </summary>
        public SubmitType Method { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 是否认证
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// 实例化PublishMethod
        /// </summary>
        public PublishMethod()
        {
            this.Enabled = true;
            this.Authorized = true;
            this.Method = SubmitType.GET;
        }

        /// <summary>
        /// 实例化PublishMethod
        /// </summary>
        /// <param name="name"></param>
        public PublishMethod(string name)
            : this()
        {
            this.Name = name;
        }
    }
}
