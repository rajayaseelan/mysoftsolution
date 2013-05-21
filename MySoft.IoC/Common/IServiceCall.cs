using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 执行服务请求接口
    /// </summary>
    public interface IServiceCall
    {
        /// <summary>
        /// 开始执行命令
        /// </summary>
        /// <param name="reqMsg"></param>
        void BeginCall(CallMessage reqMsg);

        /// <summary>
        /// 结束执行命令
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedTime"></param>
        void EndCall(CallMessage reqMsg, ReturnMessage resMsg, long elapsedTime);
    }
}
