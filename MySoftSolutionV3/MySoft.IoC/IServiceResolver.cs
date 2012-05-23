using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="interfaceType"></param>
        /// <param name="currNode"></param>
        /// <returns></returns>
        ServerNode GetServerNode(Type interfaceType, ServerNode currNode);

        /// <summary>
        /// 解析服务，可自己注入自定义服务为代理本地服务
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        object ResolveService(Type interfaceType);
    }
}
