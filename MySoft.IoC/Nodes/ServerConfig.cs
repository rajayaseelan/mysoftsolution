using System;
using System.Collections;
using System.Xml.Serialization;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 服务配置
    /// </summary>
    [Serializable]
    [XmlRoot("serverConfig")]
    public class ServerConfig
    {
        /// <summary>
        /// 配置节点
        /// </summary>
        [XmlArray("nodeConfigs")]
        [XmlArrayItem("nodeConfig", typeof(NodeConfig))]
        public NodeConfigCollection Configs { get; set; }

        /// <summary>
        /// 服务节点
        /// </summary>
        [XmlArray("serverNodes")]
        [XmlArrayItem("serverNode", typeof(ServerNode))]
        public ServerNodeCollection Nodes { get; set; }

        /// <summary>
        /// 实例化ServerConfig
        /// </summary>
        public ServerConfig()
        {
            this.Configs = new NodeConfigCollection();
            this.Nodes = new ServerNodeCollection();
        }
    }

    /// <summary>
    /// ServerNode集合
    /// </summary>
    [Serializable]
    public class ServerNodeCollection : CollectionBase
    {
        /// <summary>
        /// Adds a new ServerNode to the collection.
        /// </summary>
        /// <param name="r">A ServerNode instance.</param>
        public virtual void Add(ServerNode r)
        {
            this.InnerList.Add(r);
        }

        /// <summary>
        /// Gets or sets a ServerNode at a specified ordinal index.
        /// </summary>
        public ServerNode this[int index]
        {
            get
            {
                return (ServerNode)this.InnerList[index];
            }
            set
            {
                this.InnerList[index] = value;
            }
        }
    }
}
