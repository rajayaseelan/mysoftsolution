using System;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务并发处理
    /// </summary>
    internal class ServiceSemaphore
    {
        private ILog logger;
        private ServiceCaller caller;
        private ServerStatusService status;
        private Action<CallEventArgs> action;

        /// <summary>
        /// 实例化ServiceSemaphore
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="logger"></param>
        /// <param name="action"></param>
        public ServiceSemaphore(ServiceCaller caller, ServerStatusService status, ILog logger, Action<CallEventArgs> action)
        {
            this.caller = caller;
            this.status = status;
            this.logger = logger;
            this.action = action;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        public void Send(IScsServerClient channel, string messageId, RequestMessage reqMsg)
        {
            //获取AppPath
            var appPath = (channel.UserToken == null) ? null : (channel.UserToken as AppClient).AppPath;

            //实例化上下文
            var e = new CallerContext
                                            {
                                                MessageId = messageId,
                                                Channel = channel,
                                                Request = reqMsg,
                                                Caller = CreateCaller(appPath, reqMsg)
                                            };

            //启动线程处理
            ThreadPool.QueueUserWorkItem(WaitCallback, e);
        }

        /// <summary>
        /// 线程回调处理
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            if (state == null) return;

            var e = state as CallerContext;

            try
            {
                if (e.Channel.CommunicationState != CommunicationStates.Connected) return;

                //实例化服务通道
                using (var client = new ServiceChannel(caller, status))
                {
                    try
                    {
                        //发送消息
                        client.SendMessage(e, action);
                    }
                    catch (Exception ex)
                    {
                        //写异常日志
                        logger.WriteError(ex);
                    }
                }
            }
            finally
            {
                //清理资源
                e.Dispose();
            }
        }

        /// <summary>
        /// 获取AppCaller
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private AppCaller CreateCaller(string appPath, RequestMessage reqMsg)
        {
            //服务参数信息
            var caller = new AppCaller
            {
                AppPath = appPath,
                AppName = reqMsg.AppName,
                IPAddress = reqMsg.IPAddress,
                HostName = reqMsg.HostName,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName,
                Parameters = reqMsg.Parameters.ToString(),
                CallTime = DateTime.Now
            };

            return caller;
        }
    }
}
