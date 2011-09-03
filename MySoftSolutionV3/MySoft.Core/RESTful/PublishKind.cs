using System;

namespace MySoft.RESTful
{
    /// <summary>
    /// 发布REST分类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class PublishKind : Attribute
    {
        /// <summary>
        /// 类别名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类别描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 实例化PublishKind
        /// </summary>
        public PublishKind()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// 实例化PublishKind
        /// </summary>
        /// <param name="name"></param>
        public PublishKind(string name)
            : this()
        {
            this.Name = name;
        }
    }
}
