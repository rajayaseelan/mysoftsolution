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
        private readonly ILog logger;
        private readonly ServiceCaller caller;
        private readonly ServerStatusService status;
        private readonly Semaphore semaphore;
        private readonly Action<CallEventArgs> callback;

        /// <summary>
        /// 实例化ServiceSemaphore
        /// </summary>
        /// <param name="maxCaller"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="logger"></param>
        /// <param name="callback"></param>
        public ServiceSemaphore(int maxCaller, ServiceCaller caller, ServerStatusService status, ILog logger, Action<CallEventArgs> callback)
        {
            this.caller = caller;
            this.status = status;
            this.logger = logger;
            this.callback = callback;

            this.semaphore = new Semaphore(maxCaller, maxCaller);
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
            using (var e = new CallerContext
                                    {
                                        MessageId = messageId,
                                        Request = reqMsg,
                                        Caller = CreateCaller(appPath, reqMsg)
                                    })
            {
                //等待资源
                semaphore.WaitOne(-1, false);

                try
                {
                    //连接状态
                    if (channel.CommunicationState == CommunicationStates.Connected)
                    {
                        //实例化服务通道
                        var client = new ServiceChannel(channel, caller, status);

                        //发送消息
                        client.Send(e, callback);
                    }
                }
                catch (Exception ex)
                {
                    //写异常日志
                    logger.WriteError(ex);
                }
                finally
                {
                    //释放资源
                    semaphore.Release();
                }
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
