using System;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel
    {
        public event EventHandler<CallEventArgs> Callback;

        private ILog logger;
        private ServiceCaller caller;
        private ServerStatusService status;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="logger"></param>
        public ServiceChannel(ServiceCaller caller, ServerStatusService status, ILog logger)
        {
            this.caller = caller;
            this.status = status;
            this.logger = logger;
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void SendResponse(IScsServerClient channel, CallerContext e)
        {
            try
            {
                //获取响应信息
                caller.InvokeResponse(channel, e);
            }
            catch (Exception ex)
            {
                //获取异常响应信息
                e.Message = IoCHelper.GetResponse(e.Request, ex);
            }

            //处理响应信息
            HandleResponse(e);

            //发送消息
            SendMessage(channel, e);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="e"></param>
        private void HandleResponse(CallerContext e)
        {
            if (e.Message == null) return;

            try
            {
                //状态服务不统计
                if (e.Caller.ServiceName != typeof(IStatusService).FullName)
                {
                    //调用参数
                    var callArgs = new CallEventArgs(e.Caller)
                    {
                        ElapsedTime = e.Message.ElapsedTime,
                        Count = Math.Max(e.Message.Count, e.Count),
                        Error = e.Message.Error,
                        Value = e.Message.Value
                    };

                    //开始异步调用
                    var action = new Action<CallEventArgs>(AsyncCounter);

                    //调用结束关闭句柄
                    action.BeginInvoke(callArgs, null, null);
                }
            }
            catch (Exception ex) { }
            finally
            {
                //设置消息异常
                SetMessageError(e);
            }
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="callArgs"></param>
        private void AsyncCounter(CallEventArgs callArgs)
        {
            try
            {
                //调用计数服务
                status.Counter(callArgs);

                //开始调用
                if (Callback != null) Callback(this, callArgs);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 设置消息异常
        /// </summary>
        /// <param name="e"></param>
        private void SetMessageError(CallerContext e)
        {
            if (e.Message == null) return;

            //如果是Json方式调用，则需要处理异常
            if (e.Request.InvokeMethod && e.Message.IsError)
            {
                //获取最底层异常信息
                var error = ErrorHelper.GetInnerException(e.Message.Error);

                e.Message.Error = new ApplicationException(error.Message);
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
    }
}
