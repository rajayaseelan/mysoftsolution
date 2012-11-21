using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 服务节点委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public delegate IList<ServerNode> ServerNodeEventHandler(object sender, NodeEventArgs e);

    /// <summary>
    /// Node节点
    /// </summary>
    [Serializable]
    public class NodeEventArgs : EventArgs
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeKey { get; private set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 实例化NodeEventArgs
        /// </summary>
        /// <param name="nodeKey"></param>
        public NodeEventArgs(string nodeKey)
        {
            this.NodeKey = nodeKey;
        }
    }
}
