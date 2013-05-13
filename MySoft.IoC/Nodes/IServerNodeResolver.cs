using System.Collections.Generic;

namespace MySoft.IoC.Nodes
{
    /// <summary>
    /// 服务节点选择器接口
    /// </summary>
    public interface IServerNodeResolver
    {
        /// <summary>
        /// 获取服务器节点
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        IList<ServerNode> GetServerNodes(string nodeKey, string serviceName);
    }
}
