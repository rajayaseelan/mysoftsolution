using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using MySoft.Installer.Configuration;

namespace MySoft.PlatformService
{
    [RunInstaller(true)]
    public partial class BusinessInstaller : System.Configuration.Install.Installer
    {
        public BusinessInstaller(InstallerConfiguration config)
        {
            BeforeInstall += new InstallEventHandler((obj, state) => { Initialize(config); });
            BeforeUninstall += new InstallEventHandler((obj, state) => { Initialize(config); });

            InitializeComponent();
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void Initialize(InstallerConfiguration config)
        {
            if (config == null) return;
            string _ServiceName = config.ServiceName;
            string _DisplayName = config.DisplayName;
            string _Description = config.Description;
            string _UserName = config.UserName;
            string _Password = config.Password;

            if (string.IsNullOrEmpty(_DisplayName))
            {
                _DisplayName = _ServiceName;
            }

            ServiceProcessInstaller spi = new ServiceProcessInstaller();

            //指定服务帐号类型
            if (String.IsNullOrEmpty(_UserName) || String.IsNullOrEmpty(_Password))
            {
                spi.Account = ServiceAccount.LocalSystem;
            }
            else
            {
                spi.Account = ServiceAccount.User;
                spi.Username = _UserName;
                spi.Password = _Password;
            }

            ServiceInstaller si = new ServiceInstaller();
            si.ServiceName = _ServiceName;
            si.DisplayName = _DisplayName + " (Paltform Service)";
            si.Description = _Description + " (平台服务中心)";
            si.StartType = ServiceStartMode.Automatic;

            // adding				
            this.Installers.Add(spi);
            this.Installers.Add(si);
        }
    }
}
