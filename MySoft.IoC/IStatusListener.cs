using System.Collections.Generic;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 状态信息监听
    /// </summary>
    public interface IStatusListener
    {
        /// <summary>
        /// 推送客户端连接信息
        /// </summary>
        /// <param name="connectInfo"></param>
        void Push(ConnectInfo connectInfo);

        /// <summary>
        /// 推送改变客户端信息
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="appClient"></param>
        void Change(string ipAddress, int port, AppClient appClient);

        /// <summary>
        /// 推送实时错误信息
        /// </summary>
        /// <param name="callError"></param>
        void Push(CallError callError);

        /// <summary>
        /// 调用超时事件信息
        /// </summary>
        /// <param name="callTimeout"></param>
        void Push(CallTimeout callTimeout);

        /// <summary>
        /// 推送服务状态信息（包括SummaryStatus，HighestStatus，TimeStatus）
        /// </summary>
        /// <returns></returns>
        void Push(ServerStatus serverStatus);
    }
}
