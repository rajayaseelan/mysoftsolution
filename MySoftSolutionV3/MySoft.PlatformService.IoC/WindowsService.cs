using System;
using System.Configuration;
using System.Threading;
using MySoft.Installer;
using MySoft.IoC;
using MySoft.IoC.Configuration;
using MySoft.Logger;

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
        private string[] mailTo;

        /// <summary>
        /// 实例化Windows服务
        /// </summary>
        public WindowsService()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(WindowsService_UnhandledException);
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
                server_OnLog("Service ready started...", LogType.Normal);

                StartService();

                server_OnLog(string.Format("Server host: {0}", server.ServerUrl), LogType.Normal);
                server_OnLog("Press enter to exit and stop service...", LogType.Normal);
            }
            else
            {
                StartService();
            }
        }

        private void StartService()
        {
            server.Start();
        }

        public void Stop()
        {
            if (startMode == StartMode.Console)
            {
                server_OnLog("Service ready stopped...", LogType.Normal);
            }

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
                    else if (type == LogType.Information)
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
                //如果是以下异常，则不发送邮件
                SimpleLog.Instance.WriteLogWithSendMail(error, mailTo);
            }
        }
    }
}
