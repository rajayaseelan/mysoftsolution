using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Configuration;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务节点
    /// </summary>
    public sealed class ServiceNode : RemoteNode
    {
        /// <summary>
        /// 返回一个远程节点
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static ServiceNode Parse(string ip, int port)
        {
            return new ServiceNode { Key = string.Format("{0}:{1}", ip, port), IP = ip, Port = port };
        }

        /// <summary>
        /// 返回一个远程节点
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ServiceNode Parse(string value)
        {
            var strs = value.Split(':');
            return Parse(strs[0], Convert.ToInt32(strs[1]));
        }
    }
}
