using System;
using System.Collections.Generic;

namespace MySoft.Remoting.Configuration
{
    /// <summary>
    /// RemotingHost实体类
    /// </summary>
    [Serializable]
    public class RemotingHost
    {
        private string _Name;

        /// <summary>
        /// RemotingHost Name
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _DefaultServer;

        /// <summary>
        /// 默认服务器
        /// </summary>
        public string DefaultServer
        {
            get { return _DefaultServer; }
            set { _DefaultServer = value; }
        }

        private Dictionary<string, RemotingServer> _Servers;

        /// <summary>
        /// Remoting服务器集合
        /// </summary>
        public Dictionary<string, RemotingServer> Servers
        {
            get { return _Servers; }
            set { _Servers = value; }
        }

        Dictionary<string, string> _Modules;

        /// <summary>
        /// 远程对象业务模块集合
        /// </summary>
        public Dictionary<string, string> Modules
        {
            get { return _Modules; }
            set { _Modules = value; }
        }

        Boolean _IsChecking;

        /// <summary>
        /// 该服务器是否正在被检测
        /// </summary>
        public Boolean IsChecking
        {
            get { return _IsChecking; }
            set { _IsChecking = value; }
        }
    }
}
