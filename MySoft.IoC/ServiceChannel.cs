using System;
using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        private IScsServerClient channel;
        private ServiceCaller caller;
        private ServerStatusService status;

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
        /// <param name="messageId"></param>
        /// <param name="reqMsg"></param>
        /// <param name="action"></param>
        public void SendMessage(string messageId, RequestMessage reqMsg, Action<CallEventArgs> action)
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

            //发送结果
            if (caller.InvokeResponse(e))
            {
                //状态服务跳过
                if (reqMsg.ServiceName != typeof(IStatusService).FullName)
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
                SendMessageToChannel(e);
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        private void HandleResponse(CallerContext e, Action<CallEventArgs> action)
        {
            //调用参数
            var callArgs = new CallEventArgs(e.Caller)
            {
                ElapsedTime = e.Message.ElapsedTime,
                Count = e.Message.Count,
                Error = e.Message.Error,
                Value = e.Message.Value
            };

            //调用计数服务
            status.Counter(callArgs);

            //回调处理
            if (action != null)
            {
                //开始异步调用
                action.BeginInvoke(callArgs, ar =>
                {
                    try
                    {
                        action.EndInvoke(ar);
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        ar.AsyncWaitHandle.Close();
                    }
                }, null);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="e"></param>
        private void SendMessageToChannel(CallerContext e)
        {
            try
            {
                var message = new ScsResultMessage(e.Message, e.MessageId);

                //发送消息
                e.Channel.SendMessage(message);
            }
            catch (SocketException ex) { }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
            }
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
                    e.Channel.SendMessage(message);
                }
                catch
                {
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

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.channel = null;
            this.caller = null;
            this.status = null;
        }

        #endregion
    }
}
