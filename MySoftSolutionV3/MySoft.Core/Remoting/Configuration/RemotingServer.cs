using System;

namespace MySoft.Remoting.Configuration
{
    /// <summary>
    /// 远程服务器实体
    /// </summary>
    [Serializable]
    public class RemotingServer
    {
        private string _ServerName;

        /// <summary>
        /// 服务器名（用来区别其它服务器）
        /// </summary>
        public string ServerName
        {
            get { return _ServerName; }
            set { _ServerName = value; }
        }

        private string _ServerUrl;

        /// <summary>
        /// 获取远程业务对象Url（如：tcp://127.0.0.1:8888/NetValue）
        /// </summary>
        public string ServerUrl
        {
            get { return _ServerUrl; }
            set { _ServerUrl = value; }
        }
    }
}
