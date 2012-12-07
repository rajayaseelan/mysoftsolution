using System;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel
    {
        private readonly IScsServerClient channel;
        private readonly ServiceCaller caller;
        private readonly ServerStatusService status;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        public ServiceChannel(IScsServerClient channel, ServiceCaller caller, ServerStatusService status)
        {
            this.channel = channel;
            this.caller = caller;
            this.status = status;
        }

        /// <summary>
        /// 响应消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        public void Send(CallerContext e, Action<CallEventArgs> action)
        {
            //发送结果
            if (caller.InvokeResponse(channel, e))
            {
                //状态服务跳过
                if (e.Request.ServiceName != typeof(IStatusService).FullName)
                {
                    //处理响应信息
                    HandleResponse(e, action);
                }

                //如果是Json方式调用，则需要处理异常
                if (e.Request.InvokeMethod && e.Message.IsError)
                {
                    e.Message.Error = new ApplicationException(e.Message.Error.Message);
                }

                //发送消息
                SendMessage(e);
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="callback"></param>
        private void HandleResponse(CallerContext e, Action<CallEventArgs> callback)
        {
            try
            {
                //调用参数
                var callArgs = new CallEventArgs(e.Caller)
                {
                    ElapsedTime = e.Message.ElapsedTime,
                    Count = e.Message.Count,
                    Error = e.Message.Error,
                    Value = e.Message.Value
                };

                //回调处理
                if (callback != null)
                {
                    //开始异步调用
                    callback.BeginInvoke(callArgs, ar =>
                    {
                        try
                        {
                            var action = ar.AsyncState as Action<CallEventArgs>;
                            action.EndInvoke(ar);
                        }
                        catch (Exception ex) { }
                        finally
                        {
                            ar.AsyncWaitHandle.Close();
                        }
                    }, callback);
                }

                //调用计数服务
                status.Counter(callArgs);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="e"></param>
        private void SendMessage(CallerContext e)
        {
            try
            {
                var message = new ScsResultMessage(e.Message, e.MessageId);

                //发送消息
                channel.SendMessage(message);
            }
            catch (CommunicationException ex) { }
            catch (ObjectDisposedException ex) { }
            catch (SocketException ex) { }
            catch (Exception ex)
            {
                try
                {
                    //获取异常响应
                    var body = string.Format("Sending messages error: {0}, service: ({1}, {2})",
                                            ErrorHelper.GetInnerException(ex).Message, e.Caller.ServiceName, e.Caller.MethodName);

                    var error = IoCHelper.GetException(e.Caller, body);

                    var resMsg = IoCHelper.GetResponse(e.Request, error);
                    var message = new ScsResultMessage(resMsg, e.MessageId);

                    //发送消息
                    channel.SendMessage(message);
                }
                catch
                {
                }

                throw;
            }
        }
    }
}
