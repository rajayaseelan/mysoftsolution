using System;
using System.Configuration;
using System.Threading;
using MySoft.Installer;
using MySoft.IoC;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.PlatformService.IoC
{
    /// <summary>
    /// Windows服务
    /// </summary>
    public class WindowsService : IServiceRun
    {
        private int timeout = -1;
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
            var config = CastleServiceConfiguration.GetConfig();
            this.server = new CastleService(config);

            this.server.OnLog += new LogEventHandler(server_OnLog);
            this.server.OnError += new ErrorLogEventHandler(server_OnError);
            this.server.OnCalling += new EventHandler<CallEventArgs>(server_OnCalling);

            //处理邮件地址
            string address = ConfigurationManager.AppSettings["SendMailAddress"];
            if (!string.IsNullOrEmpty(address)) mailTo = address.Split(',', ';', '|');
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="startMode"></param>
        /// <param name="state"></param>
        public void Start(StartMode startMode, object state)
        {
            this.startMode = startMode;
            if (state != null)
            {
                int value = -1;
                if (int.TryParse(state.ToString(), out value))
                {
                    if (value > 0)
                    {
                        this.timeout = value;
                        server_OnLog(string.Format("Display caller more than timeout ({0}) ms...", timeout), LogType.Normal);
                    }
                }
                this.timeout = Convert.ToInt32(state);
            }

            if (startMode != StartMode.Service)
            {
                server_OnLog("Server ready started...", LogType.Normal);

                StartService();

                server_OnLog(string.Format("Tcp server host -> {0}", server.ServerUrl), LogType.Normal);
                server_OnLog(string.Format("Server publish ({0}) services.", server.ServiceCount), LogType.Normal);
                server_OnLog("Press enter to exit and stop server...", LogType.Normal);
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
            if (startMode != StartMode.Service)
            {
                server_OnLog("Server ready stopped...", LogType.Normal);
            }

            server.Stop();
        }

        #endregion

        /// <summary>
        /// 服务调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void server_OnCalling(object sender, CallEventArgs e)
        {
            if (e.IsTimeout)
            {
                var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
                var body = string.Format("Remote client【{0}】call service ({1},{2}) timeout ({4}) ms.\r\nParameters => {3}",
                            message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters, e.ElapsedTime);

                var error = IoCHelper.GetException(e.Caller, new System.TimeoutException(body));

                //写异常日志
                server_OnError(error);
            }
            else if (e.IsError)
            {
                var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
                var body = string.Format("Remote client【{0}】call service ({1},{2}) error.\r\nParameters => {3}",
                            message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters);

                var error = IoCHelper.GetException(e.Caller, body, e.Error);

                //写异常日志
                server_OnError(error);
            }
            else
            {
                if (startMode == StartMode.Debug)
                {
                    var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
                    var body = string.Format("Remote client【{0}】call service ({1},{2}), result ({4}) rows, elapsed time ({5}) ms.\r\nParameters => {3}",
                                message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters, e.Count, e.ElapsedTime);

                    server_OnLog(body, LogType.Information);
                }
            }
        }

        void server_OnLog(string log, LogType type)
        {
            if (startMode != StartMode.Service)
            {
                string message = string.Format("[{0}] => ({1}) {2}", DateTime.Now, type, log);

                if (type == LogType.Error)
                {
                    IoCHelper.WriteLine(ConsoleColor.Red, message);
                }
                else if (type == LogType.Warning)
                {
                    IoCHelper.WriteLine(ConsoleColor.Yellow, message);
                }
                else if (type == LogType.Information)
                {
                    IoCHelper.WriteLine(ConsoleColor.Green, message);
                }
                else
                {
                    IoCHelper.WriteLine(message);
                }
            }
        }

        void server_OnError(Exception error)
        {
            if (startMode != StartMode.Service)
            {
                string message = string.Format("[{0}] => {1}", DateTime.Now, error.Message);
                if (error.InnerException != null)
                {
                    message += string.Format("\r\n错误信息 => {0}", ErrorHelper.GetInnerException(error.InnerException));
                }

                if (error is WarningException)
                {
                    IoCHelper.WriteLine(ConsoleColor.Yellow, message);
                }
                else
                {
                    IoCHelper.WriteLine(ConsoleColor.Red, message);
                }
            }
            else
            {
                SimpleLog.Instance.WriteLogWithSendMail(error, mailTo);
            }
        }
    }
}
