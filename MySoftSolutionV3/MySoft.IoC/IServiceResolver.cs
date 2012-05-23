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
        /// <typeparam name="T"></typeparam>
        /// <param name="currNode"></param>
        /// <returns></returns>
        ServerNode GetServerNode<T>(ServerNode currNode);

        /// <summary>
        /// 解析服务，可自己注入自定义服务为代理本地服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        T ResolveService<T>(IContainer container);
    }
}
