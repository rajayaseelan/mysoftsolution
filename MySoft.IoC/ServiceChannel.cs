using System;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        private Action<CallEventArgs> callback;
        private IServiceContainer container;
        private ServiceCaller caller;
        private ServerStatusService status;
        private Semaphore semaphore;
        private int timeout;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="container"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="maxCaller"></param>
        /// <param name="timeout"></param>
        public ServiceChannel(Action<CallEventArgs> callback, IServiceContainer container, ServiceCaller caller
                                , ServerStatusService status, int maxCaller, int timeout)
        {
            this.callback = callback;
            this.container = container;
            this.caller = caller;
            this.status = status;
            this.timeout = timeout;
            this.semaphore = new Semaphore(maxCaller);
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void SendResponse(IScsServerClient channel, CallerContext e)
        {
            //开始计时
            var watch = Stopwatch.StartNew();

            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                using (var channelResult = new ChannelResult(channel, e))
                {
                    //开始异步调用
                    ManagedThreadPool.QueueUserWorkItem(WaitCallback, channelResult);

                    //等待超时响应
                    if (!channelResult.WaitOne(TimeSpan.FromSeconds(timeout)))
                    {
                        //获取异常响应
                        e.Message = GetTimeoutResponse(e.Request);
                    }
                    else
                    {
                        //正常响应信息
                        e.Message = channelResult.Message;
                    }
                }

                //处理上下文
                HandleCallerContext(channel, e, watch.ElapsedMilliseconds);
            }
            finally
            {
                //停止记时
                if (watch.IsRunning)
                {
                    watch.Stop();
                }

                //释放一个控制器
                semaphore.AddOne();
            }
        }

        /// <summary>
        /// 响应信息
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            if (state == null) return;

            try
            {
                var channelResult = state as ChannelResult;

                if (channelResult.Completed)
                {
                    channelResult.Set(null);
                }
                else
                {
                    //调用响应信息
                    var channel = channelResult.Channel;
                    var context = channelResult.Context;

                    var resMsg = caller.InvokeResponse(channel, context);

                    channelResult.Set(resMsg);
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
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg)
        {
            //获取异常响应信息
            var body = string.Format("Async call service ({0}, {1})  timeout ({2}) ms.",
                        reqMsg.ServiceName, reqMsg.MethodName, timeout * 1000);

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
        /// <param name="elapsedTime"></param>
        private void HandleCallerContext(IScsServerClient channel, CallerContext e, long elapsedTime)
        {
            if (e.Message != null)
            {
                //不是从缓存读取，则响应与状态服务跳过
                if (e.Request.ServiceName != typeof(IStatusService).FullName)
                {
                    //处理响应信息
                    HandleResponse(e.Caller, e.Message, elapsedTime);
                }

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
                //生成响应信息
                var resMsg = IoCHelper.GetResponse(e.Request, null);

                //不是从缓存读取，则响应与状态服务跳过
                if (e.Request.ServiceName != typeof(IStatusService).FullName)
                {
                    //处理响应信息
                    HandleResponse(e.Caller, resMsg, elapsedTime);
                }
            }

            //发送消息
            SendMessage(channel, e);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedTime"></param>
        private void HandleResponse(AppCaller caller, ResponseMessage resMsg, long elapsedTime)
        {
            //调用参数
            var callArgs = new CallEventArgs(caller)
            {
                ElapsedTime = elapsedTime,
                Count = resMsg.Count,
                Error = resMsg.Error,
                Value = resMsg.Value
            };

            try
            {
                //开始调用
                if (callback != null)
                {
                    //异步调用
                    callback.BeginInvoke(callArgs, ar =>
                    {
                        try
                        {
                            //完成委托
                            callback.EndInvoke(ar);
                        }
                        catch (Exception ex) { }
                        finally
                        {
                            ar.AsyncWaitHandle.Close();
                        }
                    }, callArgs);
                }
            }
            catch (Exception ex) { }
            finally
            {
                //调用计数服务
                status.Counter(callArgs);
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
                if (e.Buffer == null && e.Message == null) return;

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

                container.WriteError(error);
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.callback = null;
            this.caller = null;
            this.status = null;
        }

        #endregion
    }
}
