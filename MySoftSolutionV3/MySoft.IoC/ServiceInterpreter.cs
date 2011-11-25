using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Castle.Core.Resource;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Configuration.Interpreters;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务解析器
    /// </summary>
    public class ServiceInterpreter : XmlInterpreter
    {
        /// <summary>
        /// 实例化ServiceInterpreter
        /// </summary>
        public ServiceInterpreter()
            : base()
        { }

        /// <summary>
        /// 实例化ServiceInterpreter
        /// </summary>
        /// <param name="source"></param>
        public ServiceInterpreter(IResource source)
            : base(source)
        { }

        /// <summary>
        /// 处理资源
        /// </summary>
        /// <param name="source"></param>
        /// <param name="store"></param>
        public override void ProcessResource(IResource source, IConfigurationStore store)
        {
            base.ProcessResource(source, store);

            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = Path.Combine(baseDir, "Services\\");

            if (Directory.Exists(dir))
            {
                foreach (var extraConfig in Directory.GetFiles(dir, "*.dll.config"))
                {
                    try
                    {
                        var interpreter = new XmlInterpreter(extraConfig) { Kernel = Kernel };
                        interpreter.ProcessResource(interpreter.Source, store);
                    }
                    catch (ConfigurationErrorsException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to load configuration: " + extraConfig, ex);
                    }
                }
            }
        }
    }
}
