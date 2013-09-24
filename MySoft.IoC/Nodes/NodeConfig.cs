using System;
using System.Collections;
using System.Xml.Serialization;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 节点配置
    /// </summary>
    [Serializable]
    [XmlRoot("nodeConfig")]
    public class NodeConfig
    {
        /// <summary>
        /// 程序集名称
        /// </summary>
        [XmlAttribute("assemblyName")]
        public string AssemblyName { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        [XmlAttribute("namespace")]
        public string Namespace { get; set; }

        /// <summary>
        /// 节点Key
        /// </summary>
        [XmlAttribute("key")]
        public string Key { get; set; }
    }

    /// <summary>
    /// NodeConfig集合
    /// </summary>
    [Serializable]
    public class NodeConfigCollection : CollectionBase
    {
        /// <summary>
        /// Adds a new NodeConfig to the collection.
        /// </summary>
        /// <param name="r">A NodeConfig instance.</param>
        public virtual void Add(NodeConfig r)
        {
            this.InnerList.Add(r);
        }

        /// <summary>
        /// Gets or sets a NodeConfig at a specified ordinal index.
        /// </summary>
        public NodeConfig this[int index]
        {
            get
            {
                return (NodeConfig)this.InnerList[index];
            }
            set
            {
                this.InnerList[index] = value;
            }
        }
    }
}
