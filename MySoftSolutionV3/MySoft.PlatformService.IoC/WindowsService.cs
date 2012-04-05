using System;
using System.Configuration;
using System.Threading;
using MySoft.Installer;
using MySoft.IoC;
using MySoft.IoC.Configuration;
using MySoft.Logger;
using MySoft.IoC.HttpServer;
using MySoft.Net.Http;
using System.Net.Sockets;

namespace MySoft.PlatformService.IoC
{
    /// <summary>
    /// Windows服务
    /// </summary>
    public class WindowsService : IServiceRun
    {
        private readonly object syncobj = new object();
        private CastleServiceConfiguration config;
        private StartMode startMode = StartMode.Service;
        private CastleService server;
        private HTTPServer httpServer;
        private string[] mailTo;

        /// <summary>
        /// 实例化Windows服务
        /// </summary>
        public WindowsService()
        {
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(WindowsService_UnhandledException);
        }

        /// <summary>
        /// 处理线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WindowsService_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            SimpleLog.Instance.WriteLogWithSendMail(exception, mailTo);
        }

        #region IServiceRun 成员

        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Init()
        {
            this.config = CastleServiceConfiguration.GetConfig();
            this.server = new CastleService(config);

            this.server.OnLog += new LogEventHandler(server_OnLog);
            this.server.OnError += new ErrorLogEventHandler(server_OnError);

            //处理邮件地址
            string address = ConfigurationManager.AppSettings["SendMailAddress"];
            if (!string.IsNullOrEmpty(address)) mailTo = address.Split(',', ';', '|');
        }

        /// <summary>
        /// 设置运行类型
        /// </summary>
        public StartMode StartMode
        {
            get
            {
                return this.startMode;
            }
            set
            {
                this.startMode = value;
            }
        }

        public void Start()
        {
            if (startMode == StartMode.Console)
            {
                Console.WriteLine("[{0}] => Service ready started...", DateTime.Now);

                StartService();

                Console.WriteLine("[{0}] => Server host: {1}", DateTime.Now, server.ServerUrl);
                Console.WriteLine("[{0}] => Press enter to exit and stop service...", DateTime.Now);
            }
            else
            {
                StartService();
            }
        }

        private void StartService()
        {
            server.Start();

            if (config.HttpEnabled)
            {
                var caller = new HttpServiceCaller(config, server.Container);
                var factory = new HttpRequestHandlerFactory(caller);
                httpServer = new HTTPServer(factory, config.HttpPort);

                if (startMode == StartMode.Console)
                {
                    httpServer.OnServerStart += () => { Console.WriteLine("[{0}] => Http server started. http://{1}:{2}", DateTime.Now, DnsHelper.GetIPAddress(), config.HttpPort); };
                    httpServer.OnServerStop += () => { Console.WriteLine("[{0}] => Http server stoped.", DateTime.Now); };
                }

                httpServer.OnServerException += ex => server_OnError(ex);
                httpServer.Start();
            }
        }

        public void Stop()
        {
            if (startMode == StartMode.Console)
            {
                Console.WriteLine("[{0}] => Service ready stopped...", DateTime.Now);
            }

            if (httpServer != null) httpServer.Stop();
            server.Stop();
        }

        #endregion

        void server_OnLog(string log, LogType type)
        {
            if (startMode == StartMode.Console)
            {
                string message = string.Format("[{0}] => ({1}) {2}", DateTime.Now, type, log);
                lock (syncobj)
                {
                    //保存当前颜色
                    var color = Console.ForegroundColor;

                    if (type == LogType.Error)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (type == LogType.Warning)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);

                    //恢复当前颜色
                    Console.ForegroundColor = color;
                }
            }
        }

        void server_OnError(Exception error)
        {
            if (startMode == StartMode.Console)
            {
                string message = string.Format("[{0}] => {1}", DateTime.Now, error.Message);
                if (error.InnerException != null)
                {
                    message += string.Format("\r\n错误信息 => {0}", ErrorHelper.GetInnerException(error.InnerException));
                }
                lock (syncobj)
                {
                    //保存当前颜色
                    var color = Console.ForegroundColor;

                    if (error is WarningException)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);

                    //恢复当前颜色
                    Console.ForegroundColor = color;
                }
            }
            else
            {
                //如果Socket异常，则不发送邮件
                if (error is SocketException)
                {
                    SimpleLog.Instance.WriteLog(error);
                    return;
                }

                SimpleLog.Instance.WriteLogWithSendMail(error, mailTo);
            }
        }
    }
}
