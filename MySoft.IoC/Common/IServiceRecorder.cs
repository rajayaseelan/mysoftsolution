
namespace MySoft.IoC.Logger
{
    /// <summary>
    /// 服务日志记录
    /// </summary>
    public interface IServiceRecorder
    {
        /// <summary>
        /// 记录服务请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Call(object sender, RecordEventArgs e);
    }
}
