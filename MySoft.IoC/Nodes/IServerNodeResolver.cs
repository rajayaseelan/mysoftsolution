using System.Collections.Generic;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 服务节点选择器接口
    /// </summary>
    public interface IServerNodeResolver
    {
        /// <summary>
        /// 获取所有节点
        /// </summary>
        /// <returns></returns>
        IList<ServerNode> GetAllServerNode();

        /// <summary>
        /// 获取服务器节点
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        IList<ServerNode> GetServerNodes(string assemblyName, string @namespace);
    }
}