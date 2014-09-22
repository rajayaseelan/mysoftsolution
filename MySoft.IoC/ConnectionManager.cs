using MySoft.IoC.Communication.Scs.Client.Tcp;
using MySoft.IoC.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// 连接管理器
    /// </summary>
    internal static class ConnectionManager
    {
        /// <summary>
        /// Timeout for connecting to a server (as milliseconds).
        /// Default value: 15 seconds (15000 ms).
        /// </summary>
        /// 
        private const int ConnectTimeout = 15000;

        private static IList<AppNode> nodes = new List<AppNode>();
        private static string templateHTML = string.Empty;
        private static string[] mailTo = new string[0];

        static ConnectionManager()
        {
            try
            {
                //处理邮件地址
                string address = ConfigurationManager.AppSettings["SendMailAddress"];
                if (!string.IsNullOrEmpty(address)) mailTo = address.Split(',', ';', '|');

                var name = "MySoft.IoC.Resources.template.htm";
                using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)))
                {
                    templateHTML = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
            }

            //定时检测连接状态
            var timer = new TimerManager(TimeSpan.FromMilliseconds(ConnectTimeout / 3), CheckConnect);

            timer.Start();
        }

        /// <summary>
        /// 检测连接
        /// </summary>
        private static void CheckConnect(object state)
        {
            //检测连接状态
            foreach (var server in nodes.ToArray())
            {
                var node = server.Node;
                var client = server.Client;

                if (CheckConnected(node))
                {
                    //改变连接状态
                    node.Connected = true;

                    //移除节点
                    lock (nodes) { nodes.Remove(server); }

                    try
                    {
                        var ex = new SocketException((int)SocketError.Success);
                        var subject = string.Format("监控项目 [{0} - ( {1} -> {2}:{3} )] 恢复可用",
                                                client.AppName, node.Key, node.IP, node.Port);

                        var _title = "故障恢复通知";
                        var _body = string.Format("[{0} - ( {1} -> {2}:{3} )] 于 {4} 恢复可用 ({5})",
                                                   client.AppName, node.Key, node.IP, node.Port,
                                                   DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss"),
                                                   ex.Message);
                        var _client = string.Format("{0} ({1})", client.AppName, client.AppVersion);
                        var _display = "normal";
                        var _timeout = GetDateTime(DateTime.Now - server.AddTime);
                        var _path = client.AppPath;
                        var _status = string.Format("{0} ({1} => {2}:{3}) 连接成功",
                                                   client.HostName, client.IPAddress, node.IP, node.Port);

                        subject = string.Format("{0} - 故障持续{1}", subject, _timeout);

                        //替换模板
                        var body = templateHTML.Replace("$title", _title).Replace("$body", _body)
                                                .Replace("$client", _client).Replace("$timeout", _timeout)
                                                .Replace("$display", _display)
                                                .Replace("$path", _path).Replace("$status", _status);

                        SendMail(node, subject, body);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 获取时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private static string GetDateTime(TimeSpan ts)
        {
            if (ts.Days > 0)
                return string.Format("{0}天{1}小时{2}分钟", ts.Days, ts.Hours, ts.Minutes);
            else if (ts.Hours > 0)
                return string.Format("{0}小时{1}分钟", ts.Hours, ts.Minutes);
            else if (ts.Minutes > 0)
                return string.Format("{0}分钟", ts.Minutes);
            else
                return string.Format("{0}秒", ts.Seconds);
        }

        /// <summary>
        /// 检测连接状态
        /// </summary>
        /// <param name="node"></param>
        static bool CheckConnected(ServerNode node)
        {
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Parse(node.IP), node.Port);
                TcpHelper.ConnectToServer(endPoint, ConnectTimeout);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="client"></param>
        /// <param name="node"></param>
        /// <param name="ex"></param>
        public static void AddNode(AppClient client, ServerNode node, Exception ex)
        {
            //改变连接状态
            node.Connected = false;

            lock (nodes)
            {
                try
                {
                    //不存在无效的节点同加入
                    if (!nodes.Select(p => p.Node).Any(p => p.IP == node.IP && p.Port == node.Port))
                    {
                        nodes.Add(new AppNode { Client = client, Node = node, AddTime = DateTime.Now });

                        var subject = string.Format("监控项目 [{0} - ( {1} -> {2}:{3} )] 不可用",
                                                    client.AppName, node.Key, node.IP, node.Port);

                        var _title = "故障通知";
                        var _body = string.Format("[{0} - ( {1} -> {2}:{3} )] 于 {4} 不可用 ({5})",
                                                   client.AppName, node.Key, node.IP, node.Port,
                                                   DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss"),
                                                   ex.Message);
                        var _client = string.Format("{0} ({1})", client.AppName, client.AppVersion);
                        var _display = "none";
                        var _timeout = string.Empty;
                        var _path = client.AppPath;
                        var _status = string.Format("{0} ({1} => {2}:{3}) 连接失败",
                                                   client.HostName, client.IPAddress, node.IP, node.Port);

                        //替换模板
                        var body = templateHTML.Replace("$title", _title).Replace("$body", _body)
                                                .Replace("$client", _client).Replace("$timeout", _timeout)
                                                .Replace("$display", _display)
                                                .Replace("$path", _path).Replace("$status", _status);

                        SendMail(node, subject, body);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        /// <summary>
        /// 异步发送邮件
        /// </summary>
        /// <param name="node"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private static void SendMail(ServerNode node, string subject, string body)
        {
            if (mailTo.Count() == 0) return;

            try
            {
                var sender = new Castle.Core.Smtp.DefaultSmtpSender("mail.51shumi.com");
                sender.AsyncSend = true;
                sender.UserName = "service@51shumi.com";
                sender.Password = "fund123.cn";
                sender.Port = 25;

                var mail = new MailMessage();
                mail.From = new MailAddress("service@51shumi.com", "杭州数米基金销售有限公司");
                foreach (var m in mailTo) mail.To.Add(m);
                mail.Subject = subject;
                mail.SubjectEncoding = Encoding.GetEncoding("gb2312");
                mail.BodyEncoding = Encoding.GetEncoding("gb2312");
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                sender.Send(mail);
            }
            catch (Exception ex)
            {
                //TODO:
            }
        }

        /// <summary>
        /// 连接服务节点信息
        /// </summary>
        public class AppNode
        {
            /// <summary>
            /// 应用名称
            /// </summary>
            public AppClient Client { get; set; }

            /// <summary>
            /// 服务节点
            /// </summary>
            public ServerNode Node { get; set; }

            /// <summary>
            /// 添加时间
            /// </summary>
            public DateTime AddTime { get; set; }
        }
    }
}
