using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 服务配置
    /// </summary>
    [Serializable]
    [XmlRoot("serverconfig")]
    public class ServerConfig
    {
        /// <summary>
        /// 默认节点
        /// </summary>
        [XmlAttribute("default")]
        public string DefaultKey { get; set; }

        /// <summary>
        /// 服务节点
        /// </summary>
        [XmlElement("serverNode", Type = typeof(ServerNode))]
        public ServerNode[] Nodes { get; set; }

        /// <summary>
        /// 实例化ServerConfig
        /// </summary>
        public ServerConfig()
        {
            this.Nodes = new ServerNode[0];
        }
    }
}
