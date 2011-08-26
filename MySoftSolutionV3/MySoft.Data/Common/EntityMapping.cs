using System;
using System.Xml.Serialization;

namespace MySoft.Data.Mapping
{
    /// <summary>
    /// 表映射设置
    /// </summary>
    [Serializable]
    [XmlRoot("tableSetting")]
    public class TableSetting
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        [XmlAttribute("namespace")]
        public string Namespace { get; set; }

        /// <summary>
        /// 表前缀
        /// </summary>
        [XmlAttribute("prefix")]
        public string Prefix { get; set; }

        /// <summary>
        /// 表后缀
        /// </summary>
        [XmlAttribute("suffix")]
        public string Suffix { get; set; }

        /// <summary>
        /// 表映射
        /// </summary>
        [XmlElement("tableMapping")]
        public TableMapping[] Mappings { get; set; }

        /// <summary>
        /// 初始化TableSetting
        /// </summary>
        public TableSetting()
        {
            this.Mappings = new TableMapping[0];
        }
    }

    /// <summary>
    /// 表映射节点
    /// </summary>
    [Serializable]
    [XmlRoot("tableMapping")]
    public class TableMapping
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// 类名称
        /// </summary>
        [XmlAttribute("className")]
        public string ClassName { get; set; }

        /// <summary>
        /// 使用前缀
        /// </summary>
        [XmlAttribute("usePrefix")]
        public bool UsePrefix { get; set; }

        /// <summary>
        /// 使用后缀
        /// </summary>
        [XmlAttribute("useSuffix")]
        public bool UseSuffix { get; set; }

        /// <summary>
        /// 映射的表名
        /// </summary>
        [XmlAttribute("mappingName")]
        public string MappingName { get; set; }

        /// <summary>
        /// 字段映射
        /// </summary>
        [XmlElement("fieldMapping")]
        public FieldMapping[] Mappings { get; set; }

        /// <summary>
        /// 初始化TableMapping
        /// </summary>
        public TableMapping()
        {
            this.Timeout = 60;
            this.UsePrefix = true;
            this.UseSuffix = true;
            this.Mappings = new FieldMapping[0];
        }
    }

    /// <summary>
    /// 字段映射节点
    /// </summary>
    [Serializable]
    [XmlRoot("fieldMapping")]
    public class FieldMapping
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        [XmlAttribute("propertyName")]
        public string PropertyName { get; set; }

        /// <summary>
        /// 映射的字段名
        /// </summary>
        [XmlAttribute("mappingName")]
        public string MappingName { get; set; }
    }
}
