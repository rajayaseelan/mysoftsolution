
namespace MySoft.IoC
{
    /// <summary>
    /// 服务选择器接口
    /// </summary>
    public interface IServiceResolver
    {
        /// <summary>
        /// 获取服务器节点
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="currNode"></param>
        /// <returns></returns>
        ServerNode GetServerNode(string serviceName, ServerNode currNode);
    }
}
