using System;
using System.Collections.Generic;
using MySoft.Remoting.Configuration;

namespace MySoft.Remoting
{
    /// <summary>
    /// 检测每个客户端的可用服务器
    /// </summary>
    public class RemotingHostCheck : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly RemotingHostCheck Instance = new RemotingHostCheck();

        System.Timers.Timer timer = null;
        RemotingClientConfiguration cfg = null;
        IList<string> _CheckLog = new List<string>();

        /// <summary>
        /// 服务器检测日志
        /// </summary>
        public IList<string> CheckLog
        {
            get { return _CheckLog; }
            set { _CheckLog = value; }
        }

        /// <summary>
        /// 开始检测
        /// </summary>
        public void DoCheck()
        {
            cfg = RemotingClientConfiguration.GetConfig();

            if (cfg.IsCheckServer)
            {
                timer = new System.Timers.Timer(cfg.Interval);
                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                timer.Start();
            }
        }

        //检查每个RemotingHost的默认可用服务器
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            timer.Stop();

            Dictionary<string, RemotingHost> hosts = cfg.RemotingHosts;

            foreach (KeyValuePair<string, RemotingHost> kvp in hosts)
            {
                if (!kvp.Value.IsChecking)
                {
                    CheckRemotingHost(kvp.Value);
                }
            }

            timer.Start();
        }

        /// <summary>
        /// 获取可用的服务器地址
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public string GetUsableServerUrl(RemotingHost host)
        {
            if (AppDomain.CurrentDomain.GetData(host.Name) == null)
            {
                string url = host.Servers[host.DefaultServer].ServerUrl;
                AppDomain.CurrentDomain.SetData(host.Name, url);
            }

            return AppDomain.CurrentDomain.GetData(host.Name).ToString();
        }

        void CheckRemotingHost(RemotingHost host)
        {
            string objectUrl = string.Empty;
            RemotingServer defaultServer = host.Servers[host.DefaultServer];
            string usableServerUrl = this.GetUsableServerUrl(host);
            bool flag = false;  //是否需要重设当前可用服务器标志

            host.IsChecking = true;

            //首先检查当前可用服务器
            try
            {
                objectUrl = cfg.GetRemoteObjectUrl(usableServerUrl, "RemotingTest");
                IRemotingTest t = (IRemotingTest)Activator.GetObject(typeof(RemotingTest), objectUrl);
                t.GetDate();
                WriteLog(host.Name, usableServerUrl, true, "ok");
            }
            catch (Exception ex)
            {
                flag = true;    //需要重设当前可用服务器标志
                WriteLog(host.Name, usableServerUrl, false, ex.Message);
            }

            //若当前可用服务器不是默认服务器，则再检查默认服务器，若其可用，则还原
            if (defaultServer.ServerUrl != usableServerUrl)
            {
                try
                {
                    objectUrl = cfg.GetRemoteObjectUrl(defaultServer.ServerUrl, "RemotingTest");
                    IRemotingTest t = (IRemotingTest)Activator.GetObject(typeof(RemotingTest), objectUrl);
                    t.GetDate();

                    AppDomain.CurrentDomain.SetData(host.Name, defaultServer.ServerUrl);
                    WriteLog(host.Name, defaultServer.ServerUrl, true, "ok");
                }
                catch (Exception ex)
                {
                    WriteLog(host.Name, defaultServer.ServerUrl, false, ex.Message);
                }
            }

            string serverUrl = string.Empty;

            //遍历其他服务器，检查其状态
            foreach (KeyValuePair<string, RemotingServer> kvp in host.Servers)
            {
                serverUrl = kvp.Value.ServerUrl;

                if (serverUrl == usableServerUrl) continue;
                if (serverUrl == defaultServer.ServerUrl) continue;

                objectUrl = cfg.GetRemoteObjectUrl(serverUrl, "RemotingTest");

                try
                {
                    IRemotingTest t = (IRemotingTest)Activator.GetObject(typeof(RemotingTest), objectUrl);
                    t.GetDate();

                    if (flag)
                    {
                        AppDomain.CurrentDomain.SetData(host.Name, serverUrl);  //重设当前可用服务器
                        flag = false;
                        WriteLog(host.Name, serverUrl, true, "ok");
                    }

                    WriteLog(host.Name, serverUrl, false, "ok");
                }
                catch (Exception ex)
                {
                    WriteLog(host.Name, serverUrl, false, ex.Message);
                }
            }

            host.IsChecking = false;
        }

        #region IDisposable 成员

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        #endregion

        //写日志
        private void WriteLog(string hostName, string serverUrl, bool isCurrentServer, string msg)
        {
            //string key = string.Format("{0}${1}", hostName, serverUrl);
            string log = string.Format("RemotingHost：{0}，服务器：{1}，是否当前服务器：{2}，状态：{3}，记录时间：{4}", hostName, serverUrl, isCurrentServer.ToString(), msg, DateTime.Now.ToString());

            lock (_CheckLog)
            {
                _CheckLog.Add(log);
            }
        }
    }
}
