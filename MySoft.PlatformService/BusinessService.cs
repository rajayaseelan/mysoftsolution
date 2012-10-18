using System;
using System.ServiceProcess;
using MySoft.Installer;
using MySoft.Logger;

namespace MySoft.PlatformService
{
    public partial class BusinessService : ServiceBase
    {
        private readonly IServiceRun service;
        public BusinessService(IServiceRun service)
        {
            this.service = service;
            InitializeComponent();
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
                service.Start(StartMode.Service);
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
