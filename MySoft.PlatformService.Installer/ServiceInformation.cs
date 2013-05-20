using System.ServiceProcess;
using Microsoft.Win32;

namespace MySoft.PlatformService.Installer
{
    /// <summary>
    /// 服务信息
    /// </summary>
    public class ServiceInformation : ServiceController
    {
        public ServiceInformation(string serviceName)
            : base(serviceName)
        { }

        /// <summary>
        /// 服务路径
        /// </summary>
        public string ServicePath
        {
            get
            {
                RegistryKey _Key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Services\" + base.ServiceName);
                if (_Key != null)
                {
                    object _ObjPath = _Key.GetValue("ImagePath");
                    if (_ObjPath != null) return _ObjPath.ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get
            {
                RegistryKey _Key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Services\" + base.ServiceName);
                if (_Key != null)
                {
                    object _ObjPath = _Key.GetValue("Description");
                    if (_ObjPath != null) return _ObjPath.ToString();
                }

                return string.Empty;
            }
        }
    }
}
