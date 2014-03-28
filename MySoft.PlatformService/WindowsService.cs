using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using MySoft.IoC;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Configuration;
using MySoft.IoC.Logger;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.PlatformService
{
    /// <summary>
    /// Windows服务
    /// </summary>
    public partial class WindowsService : ServiceBase
    {
        private CastleService service;
        private IServiceRecorder recorder;

        public WindowsService()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(WindowsService_UnhandledException);
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(WindowsService_UnhandledException);

            try
            {
                Init();
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("ServiceRun", ex.ToString());

                throw;
            }

            //初始化组件
            InitializeComponent();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            var config = CastleServiceConfiguration.GetConfig();
            this.service = new CastleService(config);

            this.service.OnError += error => SimpleLog.Instance.WriteLog(error);
            this.service.Completed += server_Completed;

            try
            {
                var typeName = ConfigurationManager.AppSettings["ServiceRecorderType"];
                var recordTimeout = ConfigurationManager.AppSettings["ServiceRecorderTimeout"];

                if (!string.IsNullOrEmpty(typeName))
                {
                    var type = Type.GetType(typeName);
                    var timeout = -1L;

                    if (!string.IsNullOrEmpty(recordTimeout))
                    {
                        timeout = Convert.ToInt64(recordTimeout);
                    }

                    if (type != null && typeof(IServiceRecorder).IsAssignableFrom(type))
                    {
                        this.recorder = Activator.CreateInstance(type, config, timeout) as IServiceRecorder;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 处理线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WindowsService_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            SimpleLog.Instance.WriteLog(exception);
        }

        protected override void OnStart(string[] args)
        {
            //启动服务
            service.Start();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            service.Stop();

            base.OnStop();
        }

        protected override void OnShutdown()
        {
            service.Stop();

            base.OnShutdown();
        }

        /// <summary>
        /// 服务调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void server_Completed(object sender, CallEventArgs e)
        {
            //调用服务，记入数据库
            if (recorder != null)
            {
                try
                {
                    var endPoint = service.Server.EndPoint as ScsTcpEndPoint;
                    var res = new RecordEventArgs(e.Caller)
                    {
                        MessageId = e.MessageId,
                        Error = e.Error,
                        Count = e.Count,
                        ElapsedTime = e.ElapsedTime,
                        ServerHostName = DnsHelper.GetHostName(),
                        ServerIPAddress = DnsHelper.GetIPAddress(),
                        ServerPort = endPoint.TcpPort
                    };

                    recorder.Call(sender, res);
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLog(ex);
                }
            }

            if (e.IsError)
            {
                var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
                var body = string.Format("Remote client【{0}】call service ({1},{2}) error.\r\nParameters => {3}",
                            message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters);

                var error = IoCHelper.GetException(e.Caller, body, e.Error);

                //写异常日志
                SimpleLog.Instance.WriteLog(error);
            }
        }
    }
}
