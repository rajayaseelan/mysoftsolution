using System;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        public event Action<CallEventArgs> Callback;

        private ILog logger;
        private ServiceCaller caller;
        private ServerStatusService status;
        private int timeout;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="config"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="logger"></param>
        public ServiceChannel(CastleServiceConfiguration config, ServiceCaller caller, ServerStatusService status, ILog logger)
        {
            this.caller = caller;
            this.status = status;
            this.logger = logger;
            this.timeout = config.Timeout;
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void SendResponse(IScsServerClient channel, CallerContext e)
        {
#if DEBUG
            var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
            var body = string.Format("Remote client【{0}】begin call service ({1},{2}).\r\nParameters => {3}",
                                        message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters);

            logger.WriteLog(body, LogType.Normal);
#endif

            using (var channelResult = new ChannelResult(channel, e))
            {
                //开始异步调用
                ThreadPool.QueueUserWorkItem(WaitCallback, channelResult);

                //等待超时响应
                if (!channelResult.WaitOne(TimeSpan.FromSeconds(timeout)))
                {
                    //获取异常响应
                    e.Message = GetTimeoutResponse(e.Request, "Work item timeout.");
                }
                else
                {
                    //正常响应信息
                    e.Message = channelResult.Message;
                }
            }

            //处理上下文
            HandleCallerContext(channel, e);
        }

        /// <summary>
        /// 响应信息
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            var channelResult = state as ChannelResult;

            try
            {
                //调用响应信息
                var channel = channelResult.Channel;
                var context = channelResult.Context;

                if (channel != null && channel.CommunicationState == CommunicationStates.Connected)
                {
                    //返回响应
                    var resMsg = caller.InvokeResponse(channel, context);

                    channelResult.Set(resMsg);
                }
                else
                {
                    channelResult.Set(null);
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg, string message)
        {
            //获取异常响应信息
            var body = string.Format("Async call service ({0}, {1})  timeout ({2}) ms. {3}",
                        reqMsg.ServiceName, reqMsg.MethodName, timeout * 1000, message);

            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(body));

            //设置耗时时间
            resMsg.ElapsedTime = timeout * 1000;

            return resMsg;
        }

        /// <summary>
        /// 响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        private void HandleCallerContext(IScsServerClient channel, CallerContext e)
        {
            //不是从缓存读取，则响应与状态服务跳过
            if (e.Message != null)
            {
                //处理响应信息
                HandleResponse(e.Caller, e.Message, e.Count);

                //如果是Json方式调用，则需要处理异常
                if (e.Request.InvokeMethod && e.Message.IsError)
                {
                    //获取最底层异常信息
                    var error = ErrorHelper.GetInnerException(e.Message.Error);

                    e.Message.Error = new ApplicationException(error.Message);
                }
            }
            else
            {
                var resMsg = IoCHelper.GetResponse(e.Request, null);

                //处理响应信息
                HandleResponse(e.Caller, resMsg, e.Count);
            }

            //发送消息
            SendMessage(channel, e);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="resMsg"></param>
        /// <param name="count"></param>
        private void HandleResponse(AppCaller caller, ResponseMessage resMsg, int count)
        {
            if (caller.ServiceName == typeof(IStatusService).FullName)
            {
                return;
            }

            try
            {
                //调用参数
                var callArgs = new CallEventArgs(caller)
                {
                    ElapsedTime = resMsg.ElapsedTime,
                    Count = Math.Max(resMsg.Count, count),
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };

                //调用计数服务
                status.Counter(callArgs);

                //开始调用
                if (Callback != null) Callback(callArgs);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        private void SendMessage(IScsServerClient channel, CallerContext e)
        {
            if (channel.CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                //如果没有返回消息，则退出
                if (e.Buffer == null && e.Message == null)
                {
                    return;
                }

                IScsMessage message = null;

                if (e.Buffer != null)
                    message = new ScsRawDataMessage(e.Buffer, e.MessageId);
                else
                    message = new ScsResultMessage(e.Message, e.MessageId);

                //发送消息
                channel.SendMessage(message);
            }
            catch (Exception ex)
            {
                //获取异常响应
                var title = string.Format("Sending message ({0}, {1}) error.", e.Caller.ServiceName, e.Caller.MethodName);
                var error = IoCHelper.GetException(e.Caller, title, ex);

                throw error;
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.caller = null;
            this.status = null;
        }

        #endregion
    }
}
