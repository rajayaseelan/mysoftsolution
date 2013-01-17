using System;
using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        //等待超时时间
        private const int WAIT_TIMEOUT = 10;

        private Action<CallEventArgs> callback;
        private ServiceCaller caller;
        private ServerStatusService status;
        private Semaphore _semaphore;
        private int maxCaller;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="maxCaller"></param>
        public ServiceChannel(Action<CallEventArgs> callback, ServiceCaller caller, ServerStatusService status, int maxCaller)
        {
            this.callback = callback;
            this.caller = caller;
            this.status = status;
            this.maxCaller = maxCaller;
            this._semaphore = new Semaphore(maxCaller, maxCaller);
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void Send(IScsServerClient channel, CallerContext e)
        {
            if (!_semaphore.WaitOne(TimeSpan.FromSeconds(WAIT_TIMEOUT), false))
            {
                //获取异常响应
                e.Message = IoCHelper.GetResponse(e.Request, new WarningException("The server than the largest concurrent [" + maxCaller + "]."));

                //发送消息
                SendMessage(channel, e);

                return;
            }

            try
            {
                if (caller.InvokeResponse(channel, e))
                {
                    //状态服务跳过
                    if (e.Request.ServiceName != typeof(IStatusService).FullName)
                    {
                        //处理响应信息
                        HandleResponse(e);
                    }

                    //如果是Json方式调用，则需要处理异常
                    if (e.Request.InvokeMethod && e.Message.IsError)
                    {
                        e.Message.Error = new ApplicationException(e.Message.Error.Message);
                    }

                    //发送消息
                    SendMessage(channel, e);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="e"></param>
        private void HandleResponse(CallerContext e)
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
                try
                {
                    //开始调用
                    callback(callArgs);
                }
                catch (Exception ex)
                {
                }
            }

            //调用计数服务
            status.Counter(callArgs);
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
                var message = new ScsResultMessage(e.Message, e.MessageId);

                //发送消息
                channel.SendMessage(message);
            }
            catch (CommunicationException ex) { }
            catch (ObjectDisposedException ex) { }
            catch (SocketException ex) { }
            catch (Exception ex)
            {
                //获取异常响应
                var body = string.Format("Sending messages error: {0}, service: ({1}, {2})",
                                        ErrorHelper.GetInnerException(ex).Message, e.Caller.ServiceName, e.Caller.MethodName);

                try
                {
                    var error = IoCHelper.GetException(e.Caller, body);

                    var resMsg = IoCHelper.GetResponse(e.Request, error);
                    var message = new ScsResultMessage(resMsg, e.MessageId);

                    //发送消息
                    channel.SendMessage(message);
                }
                catch (Exception inner) { }
                finally
                {
                    throw IoCHelper.GetException(e.Caller, body, ex);
                }
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                this._semaphore.Close();
            }
            catch (Exception ex) { }
            finally
            {
                this.callback = null;
                this.caller = null;
                this.status = null;
                this._semaphore = null;
            }
        }

        #endregion
    }
}
