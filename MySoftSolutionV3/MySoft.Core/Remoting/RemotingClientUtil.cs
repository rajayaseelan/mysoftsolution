using System;
using System.Collections.Generic;
using MySoft.Logger;
using MySoft.Remoting.Configuration;

namespace MySoft.Remoting
{
    /// <summary>
    /// Remoting客户端工具类
    /// </summary>
    /// <typeparam name="T">一般为接口类型</typeparam>
    public class RemotingClientUtil<T> : ILogable
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly RemotingClientUtil<T> Instance = new RemotingClientUtil<T>();
        RemotingClientConfiguration _RemotingConfiguration;
        Dictionary<string, T> _RemoteObjects = new Dictionary<string, T>();

        /// <summary>
        /// Remoting Configuration
        /// </summary>
        public RemotingClientConfiguration RemotingConfiguration
        {
            get { return _RemotingConfiguration; }
            set { _RemotingConfiguration = value; }
        }

        private RemotingClientUtil()
        {
            _RemotingConfiguration = RemotingClientConfiguration.GetConfig();

            if (_RemotingConfiguration == null) return;

            Dictionary<string, RemotingHost> hosts = _RemotingConfiguration.RemotingHosts;

            //生成所有远程对象代理并加载到内存
            foreach (KeyValuePair<string, RemotingHost> kvp in hosts)
            {
                RemotingHost host = kvp.Value;
                LoadModulesByHost(host);
            }

            //检测每个客户端的可用服务器
            RemotingHostCheck.Instance.DoCheck();

            //如果检测服务器，则输出日志
            if (_RemotingConfiguration.IsCheckServer)
            {
                System.Threading.Thread thread = new System.Threading.Thread(DoWork);
                //thread.IsBackground = true;
                thread.Start();
            }
        }

        void DoWork()
        {
            while (true)
            {
                if (OnLog != null)
                {
                    try
                    {
                        lock (RemotingHostCheck.Instance.CheckLog)
                        {
                            foreach (string log in RemotingHostCheck.Instance.CheckLog)
                            {
                                OnLog(log, LogType.Information);
                            }

                            RemotingHostCheck.Instance.CheckLog.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        OnLog(ex.Message, LogType.Error);
                    }
                }

                //每隔10秒生成一次日志
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        /// <summary>
        /// 加载远程对象代理客户端模块
        /// </summary>
        /// <param name="host"></param>
        public void LoadModulesByHost(RemotingHost host)
        {
            string serverUrl = RemotingHostCheck.Instance.GetUsableServerUrl(host);

            foreach (KeyValuePair<string, string> m in host.Modules)
            {
                string objectUrl = _RemotingConfiguration.GetRemoteObjectUrl(serverUrl, m.Value);
                T instance = (T)Activator.GetObject(typeof(T), objectUrl);

                string key = string.Format("{0}${1}${2}", host.Name, serverUrl, m.Key);
                _RemoteObjects[key] = instance;
            }
        }

        /// <summary>
        /// 获取远程对象
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="remoteObjectName"></param>
        /// <returns></returns>
        public T GetRemotingObject(string hostName, string remoteObjectName)
        {
            RemotingHost host = _RemotingConfiguration.RemotingHosts[hostName];
            string serverUrl = RemotingHostCheck.Instance.GetUsableServerUrl(host);
            string key = string.Format("{0}${1}${2}", host.Name, serverUrl, remoteObjectName);

            if (!_RemoteObjects.ContainsKey(key))
            {
                string objectUrl = _RemotingConfiguration.GetRemoteObjectUrl(serverUrl, remoteObjectName);
                T instance = (T)Activator.GetObject(typeof(T), objectUrl);
                _RemoteObjects[key] = instance;

                return instance;
            }

            return _RemoteObjects[key];
        }

        /// <summary>
        /// 获取远程对象（默认为第一个RemotingClient的默认服务器）
        /// </summary>
        /// <param name="remoteObjectName"></param>
        /// <returns></returns>
        public T GetRemotingObject(string remoteObjectName)
        {
            RemotingHost host = null;

            foreach (KeyValuePair<string, RemotingHost> kvp in _RemotingConfiguration.RemotingHosts)
            {
                host = kvp.Value;
                break;
            }

            if (host != null)
            {
                return GetRemotingObject(host.Name, remoteObjectName);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 获取知名对象客户端代理实例
        /// </summary>
        /// <param name="objectUrl"></param>
        /// <returns></returns>
        public T GetWellKnownClientInstance(string objectUrl)
        {
            if (!_RemoteObjects.ContainsKey(objectUrl))
            {
                T instance = (T)Activator.GetObject(typeof(T), objectUrl);

                _RemoteObjects.Add(objectUrl, instance);

                return instance;
            }

            return _RemoteObjects[objectUrl];
        }

        /// <summary>
        /// Remoting服务器测试
        /// </summary>
        /// <param name="serverUrl">Remoting服务器地址 （比如：tcp://127.0.0.1:8888）</param>
        /// <returns>Remoting服务器时间</returns>
        public string RemotingServerTest(string serverUrl)
        {
            IRemotingTest t = RemotingClientUtil<IRemotingTest>.Instance.GetWellKnownClientInstance(serverUrl.TrimEnd('/') + "/RemotingTest");
            return t.GetDate();
        }

        /// <summary>
        /// OnLog event.
        /// </summary>
        public event LogEventHandler OnLog;
    }
}
