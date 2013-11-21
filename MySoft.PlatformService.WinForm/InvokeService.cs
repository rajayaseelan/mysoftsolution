using MySoft.IoC.Messages;

namespace MySoft.PlatformService.WinForm
{
    /// <summary>
    /// 调用服务信息
    /// </summary>
    public class InvokeService
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public ServiceInfo Service { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public MethodInfo Method { get; set; }
    }
}
