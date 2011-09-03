using System;
using System.ServiceModel;
using MySoft.Installer;
using MySoft.RESTful;

namespace MySoft.PlatformService.RESTful
{
    class Program
    {
        static void Main()
        {
            IServiceRun run = new WindowsService();
            run.Init();

            run.Start();

            Console.ReadLine();
        }
    }

    /// <summary>
    /// Windows服务
    /// </summary>
    public class WindowsService : IServiceRun
    {
        #region IServiceRun 成员

        private ServiceHost service;

        /// <summary>
        /// 初始化服务
        /// </summary>
        public void Init()
        {
            service = new ServiceHost(typeof(DefaultRESTfulService), new Uri("http://127.0.0.1:58888/"));
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if (service.State == CommunicationState.Closed)
            {
                service.Open();
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (service.State == CommunicationState.Opened)
            {
                service.Close();
            }
        }

        private StartMode startMode;
        /// <summary>
        /// 启动模式
        /// </summary>
        public StartMode StartMode
        {
            get
            {
                return startMode;
            }
            set
            {
                startMode = value;
            }
        }

        #endregion
    }
}
