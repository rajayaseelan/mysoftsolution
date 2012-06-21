using System;

namespace MySoft.RESTful
{
    /// <summary>
    /// 发布的REST方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PublishMethodAttribute : Attribute
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
        /// 是否认证
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// 实例化PublishMethod
        /// </summary>
        public PublishMethodAttribute() { }

        /// <summary>
        /// 实例化PublishMethod
        /// </summary>
        /// <param name="name"></param>
        public PublishMethodAttribute(string name)
        {
            this.Name = name;
        }
    }
}
