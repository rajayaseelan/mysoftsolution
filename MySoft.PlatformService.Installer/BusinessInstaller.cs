using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using MySoft.Installer.Configuration;

namespace MySoft.PlatformService.Installer
{
    [RunInstaller(true)]
    public partial class BusinessInstaller : System.Configuration.Install.Installer
    {
        private static bool _Initialized = false;

        public BusinessInstaller()
        {
            BeforeInstall += BusinessInstaller_BeforeInstall;
            BeforeUninstall += BusinessInstaller_BeforeUninstall;

            InitializeComponent();
        }

        void BusinessInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            if (!_Initialized)
            {
                var config = InstallerConfiguration.GetConfig();

                Initialize(config);
            }
        }

        void BusinessInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            if (!_Initialized)
            {
                var config = InstallerConfiguration.GetConfig();

                Initialize(config);
            }
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void Initialize(InstallerConfiguration config)
        {
            _Initialized = true;

            if (config == null) return;

            string _ServiceName = config.ServiceName;
            string _DisplayName = config.DisplayName;
            string _Description = config.Description;
            string _Type = config.Type;
            string _UserName = config.UserName;
            string _Password = config.Password;
            bool _AutoRun = config.AutoRun;

            if (string.IsNullOrEmpty(_DisplayName))
            {
                _DisplayName = _ServiceName;
            }

            ServiceProcessInstaller spi = new ServiceProcessInstaller();

            switch (_Type.ToLower())
            {
                case "network":
                    spi.Account = ServiceAccount.NetworkService;
                    break;
                case "local":
                    spi.Account = ServiceAccount.LocalService;
                    break;
                case "system":
                    spi.Account = ServiceAccount.LocalSystem;
                    break;
                case "user":
                    if (String.IsNullOrEmpty(_UserName) || String.IsNullOrEmpty(_Password))
                    {
                        spi.Account = ServiceAccount.NetworkService;
                    }
                    else
                    {
                        //指定服务帐号类型
                        spi.Account = ServiceAccount.User;
                        spi.Username = _UserName;
                        spi.Password = _Password;
                    }
                    break;
                default:
                    spi.Account = ServiceAccount.NetworkService;
                    break;
            }

            ServiceInstaller si = new ServiceInstaller();
            si.ServiceName = _ServiceName;
            si.DisplayName = _DisplayName + " (Paltform Service)";
            si.Description = _Description + " (平台服务中心)";

            if (_AutoRun)
                si.StartType = ServiceStartMode.Automatic;
            else
                si.StartType = ServiceStartMode.Manual;

            //Adding installer				
            this.Installers.Add(spi);
            this.Installers.Add(si);
        }
    }
}