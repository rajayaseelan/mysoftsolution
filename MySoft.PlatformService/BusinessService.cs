using System;
using System.ServiceProcess;
using System.Threading;
using MySoft.Installer;
using MySoft.Logger;

namespace MySoft.PlatformService
{
    public partial class BusinessService : ServiceBase
    {
        private readonly IServiceRun service;

        public BusinessService()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(BusinessService_UnhandledException);
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(BusinessService_UnhandledException);

            //获取Win服务项
            this.service = InstallerServer.Instance.GetWinService();
            this.service.Init();

            InitializeComponent();
        }

        static void BusinessService_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            SimpleLog.Instance.WriteLogForDir("ServiceRun", exception);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (service == null)
                {
                    throw new Exception("IServiceRun服务加载失败，服务未能正常启动！");
                }

                string serviceName = service.GetType().FullName;
                SimpleLog.Instance.WriteLogForDir("ServiceRun", string.Format("正在启动服务{0}......", serviceName));
                service.Start(StartMode.Service, null);
                SimpleLog.Instance.WriteLogForDir("ServiceRun", string.Format("服务{0}启动成功！", serviceName));
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("ServiceRun", ex);
                throw ex;
            }

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            try
            {
                if (service == null)
                {
                    throw new Exception("IServiceRun服务加载失败，服务未能正常停止！");
                }

                string serviceName = service.GetType().FullName;
                SimpleLog.Instance.WriteLogForDir("ServiceRun", string.Format("正在停止服务{0}......", serviceName));
                service.Stop();
                SimpleLog.Instance.WriteLogForDir("ServiceRun", string.Format("服务{0}停止成功！", serviceName));
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("ServiceRun", ex);
                throw ex;
            }

            base.OnStop();
        }

        protected override void OnShutdown()
        {
            this.OnStop(); //调用停止命令
            base.OnShutdown();
        }
    }
}
