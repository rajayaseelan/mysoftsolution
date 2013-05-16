using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using MySoft.Installer;
using MySoft.Installer.Configuration;
using MySoft.Logger;

namespace MySoft.PlatformService
{
    /// <summary>
    /// 安装服务类
    /// </summary>
    public class InstallerServer
    {
        public static readonly InstallerServer Instance = new InstallerServer();

        private IServiceRun service;

        /// <summary>
        /// 获取WinService
        /// </summary>
        /// <returns></returns>
        public IServiceRun GetWinService()
        {
            return service;
        }

        /// <summary>
        /// 获取Win配置项
        /// </summary>
        /// <returns></returns>
        public InstallerConfiguration GetWinConfig()
        {
            return InstallerConfiguration.GetConfig();
        }

        /// <summary>
        /// 实例化InstallerServer
        /// </summary>
        private InstallerServer()
        {
            var config = InstallerConfiguration.GetConfig();

            if (config == null) return;

            if (service == null)
            {
                #region 动态加载服务

                try
                {
                    var type = Type.GetType(config.ServiceType);
                    if (type == null) throw new Exception(string.Format("加载服务{0}失败！", config.ServiceType));

                    this.service = Activator.CreateInstance(type) as IServiceRun;
                }
                catch (Exception ex)
                {
                    var inner = ErrorHelper.GetInnerException(ex);

                    Console.WriteLine(inner.ToString());

                    //写错误日志
                    SimpleLog.Instance.WriteLogForDir("ServiceRun", inner);
                }

                #endregion
            }
        }

        /// <summary>
        /// 列出服务
        /// </summary>
        public void ListService(string contains, string status)
        {
            if (string.IsNullOrEmpty(contains) && string.IsNullOrEmpty(status))
            {
                contains = "(Paltform Service)";
            }

            //判断第二个参数，看是否为状态
            if (!string.IsNullOrEmpty(contains) && contains.Substring(0, 1) == "-")
            {
                status = contains;
                contains = null;
            }

            IList<ServiceInformation> list = new List<ServiceInformation>();
            if (!string.IsNullOrEmpty(status))
            {
                try
                {
                    var prefix = status.Substring(0, 1);
                    status = status.Substring(1);
                    if (prefix != "-" || string.IsNullOrEmpty(status))
                    {
                        Console.WriteLine("输入的参数无效！");
                        return;
                    }

                    Console.WriteLine("正在读取服务信息......");
                    ServiceControllerStatus serviceStatus = (ServiceControllerStatus)Enum.Parse(typeof(ServiceControllerStatus), status, true);
                    list = InstallerUtils.GetServiceList(contains, serviceStatus);
                }
                catch
                {
                    Console.WriteLine("输入的状态无效！");
                    return;
                }
            }
            else
            {
                Console.WriteLine("正在读取服务信息......");
                list = InstallerUtils.GetServiceList(contains);
            }

            if (list.Count == 0)
            {
                Console.WriteLine("未能读取到相关的服务信息......");
            }
            else
            {
                foreach (var controller in list)
                {
                    try
                    {
                        Console.WriteLine("------------------------------------------------------------------------");
                        Console.WriteLine("服务名：{0} ({1})", controller.ServiceName, controller.Status);
                        Console.WriteLine("显示名：{0}", controller.DisplayName);
                        Console.WriteLine("描  述：{0}", controller.Description);
                        Console.WriteLine("路  径：{0}", controller.ServicePath);
                    }
                    finally
                    {
                        controller.Dispose();
                    }
                }
                Console.WriteLine("------------------------------------------------------------------------");
            }
        }

        /// <summary>
        /// 从控制台运行
        /// </summary>
        /// <param name="startMode"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool StartConsole(StartMode startMode, object state)
        {
            var config = InstallerConfiguration.GetConfig();

            if (config == null)
            {
                Console.WriteLine("无效的服务配置项！");
                return false;
            }

            ServiceController controller = InstallerUtils.LookupService(config.ServiceName);

            if (controller != null)
            {
                try
                {
                    if (controller.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("服务已经启动,不能从控制台启动,请先停止服务后再执行该命令！");
                        return false;
                    }
                }
                finally
                {
                    controller.Dispose();
                }
            }

            if (service != null)
            {
                service.Init();
                service.Start(startMode, state);

                Console.WriteLine("控制台已经启动......");
                return true;
            }
            else
            {
                Console.WriteLine("无效的服务启动项！");
                return false;
            }
        }

        /// <summary>
        /// 从控制台停止
        /// </summary>
        public void StopConsole()
        {
            if (service != null)
            {
                service.Stop();

                Console.WriteLine("控制台已经退出......");
            }
        }

        /// <summary>
        /// 运行服务服务
        /// </summary>
        public void StartService(string serviceName)
        {
            var config = InstallerConfiguration.GetConfig();

            if (string.IsNullOrEmpty(serviceName))
            {
                if (config == null)
                {
                    Console.WriteLine("无效的服务配置项！");
                    return;
                }

                serviceName = config.ServiceName;
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                Console.WriteLine("无效的服务名称！");
                return;
            }

            ServiceController controller = InstallerUtils.LookupService(serviceName);

            if (controller != null)
            {
                try
                {
                    if (controller.Status == ServiceControllerStatus.Stopped)
                    {
                        Console.WriteLine("正在启动服务{0}......", serviceName);
                        try
                        {
                            controller.Start();
                            controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                            controller.Refresh();

                            if (controller.Status == ServiceControllerStatus.Running)
                                Console.WriteLine("启动服务{0}成功！", serviceName);
                            else
                                Console.WriteLine("启动服务{0}失败！", serviceName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("服务{0}已经启动！", serviceName);
                    }
                }
                finally
                {
                    controller.Dispose();
                }
            }
            else
            {
                Console.WriteLine("不存在服务{0}！", serviceName);
            }
        }

        /// <summary>
        /// 停止服务服务
        /// </summary>
        public void StopService(string serviceName)
        {
            var config = InstallerConfiguration.GetConfig();

            if (string.IsNullOrEmpty(serviceName))
            {
                if (config == null)
                {
                    Console.WriteLine("无效的服务配置项！");
                    return;
                }

                serviceName = config.ServiceName;
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                Console.WriteLine("无效的服务名称！");
                return;
            }

            ServiceController controller = InstallerUtils.LookupService(serviceName);

            if (controller != null)
            {
                try
                {
                    if (controller.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("正在停止服务{0}.....", serviceName);
                        try
                        {
                            controller.Stop();
                            controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                            controller.Refresh();

                            if (controller.Status == ServiceControllerStatus.Stopped)
                                Console.WriteLine("停止服务{0}成功！", serviceName);
                            else
                                Console.WriteLine("停止服务{0}失败！", serviceName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("服务{0}已经停止！", serviceName);
                    }
                }
                finally
                {
                    controller.Dispose();
                }
            }
            else
            {
                Console.WriteLine("不存在服务{0}！", serviceName);
            }
        }

        /// <summary>
        /// 安装服务器为window服务
        /// </summary>
        public void InstallService()
        {
            var config = InstallerConfiguration.GetConfig();

            if (config == null)
            {
                Console.WriteLine("无效的服务配置项！");
                return;
            }

            ServiceController controller = InstallerUtils.LookupService(config.ServiceName);

            if (controller == null)
            {
                try
                {
                    using (TransactedInstaller installer = GetTransactedInstaller())
                    {
                        IDictionary savedState = new Hashtable();

                        installer.Install(savedState);
                        installer.Commit(savedState);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("服务{0}已经存在,请先卸载后再执行此命令！", config.ServiceName);
            }
        }

        /// <summary>
        /// 卸载window服务
        /// </summary>
        public void UninstallService()
        {
            var config = InstallerConfiguration.GetConfig();

            if (config == null)
            {
                Console.WriteLine("无效的服务配置项！");
                return;
            }

            ServiceController controller = InstallerUtils.LookupService(config.ServiceName);

            if (controller != null)
            {
                try
                {
                    using (TransactedInstaller installer = GetTransactedInstaller())
                    {
                        installer.Uninstall(null);
                        installer.Commit(null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    controller.Dispose();
                }
            }
            else
            {
                Console.WriteLine("服务{0}尚未安装,输入'/?'查看帮助！", config.ServiceName);
            }
        }

        /// <summary>
        /// 获取当前的安装信息
        /// </summary>
        /// <returns></returns>
        private TransactedInstaller GetTransactedInstaller()
        {
            TransactedInstaller installer = new TransactedInstaller();
            installer.BeforeInstall += new InstallEventHandler((obj, state) => { Console.WriteLine("服务正在安装......"); });
            installer.AfterInstall += new InstallEventHandler((obj, state) => { Console.WriteLine("服务安装完成！"); });
            installer.BeforeUninstall += new InstallEventHandler((obj, state) => { Console.WriteLine("服务正在卸载......"); });
            installer.AfterUninstall += new InstallEventHandler((obj, state) => { Console.WriteLine("服务卸载完成！"); });

            BusinessInstaller businessInstaller = new BusinessInstaller();
            installer.Installers.Add(businessInstaller);
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installer.log");
            string path = string.Format("/assemblypath={0}", System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] cmd = { path };
            InstallContext context = new InstallContext(logFile, cmd);
            installer.Context = context;

            return installer;
        }
    }
}