using System;
using System.Collections;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MT = MySoft.Threading;

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
        private MT.Semaphore semaphore;
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
            this.semaphore = new MT.Semaphore(maxCaller);
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void SendResponse(IScsServerClient channel, CallerContext e)
        {
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                using (var waitResult = new WaitResult(e.Request))
                {
                    //异步处理
                    ThreadPool.QueueUserWorkItem(WaitCallback, new ArrayList { channel, e, waitResult });

                    //等待超时响应
                    if (!waitResult.WaitOne(TimeSpan.FromSeconds(timeout)))
                    {
                        //获取异常响应
                        e.Message = GetTimeoutResponse(e.Request);
                    }
                    else
                    {
                        //正常响应信息
                        e.Message = waitResult.Message;
                    }
                }

                //处理上下文
                HandleCallerContext(channel, e);
            }
            finally
            {
                //释放一个控制器
                semaphore.AddOne();
            }
        }

        /// <summary>
        /// 等待回调
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            try
            {
                var arr = state as ArrayList;
                var channel = arr[0] as IScsServerClient;
                var context = arr[1] as IDataContext;
                var waitResult = arr[2] as WaitResult;

                //调用器为null表示已经退出
                if (context.Caller == null) return;

                //如果通道状态为未连接，也退出
                if (channel.CommunicationState == CommunicationStates.Connected)
                {
                    //调用响应信息
                    var resMsg = caller.InvokeResponse(channel, context);

                    waitResult.Set(resMsg);
                }
                else
                {
                    //设置空响应
                    waitResult.Set(null);
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
        private void HandleCallerContext(IScsServerClient channel, CallerContext e)
        {
            if (e.Message != null)
            {
                //不是从缓存读取，则响应与状态服务跳过
                if (e.Request.ServiceName != typeof(IStatusService).FullName)
                {
                    //处理响应信息
                    HandleResponse(e.Caller, e.Message);
                }

                //如果是Json方式调用，则需要处理异常
                if (e.Request.InvokeMethod && e.Message.IsError)
                {
                    //获取最底层异常信息
                    var error = ErrorHelper.GetInnerException(e.Message.Error);

                    e.Message.Error = new ApplicationException(error.Message);
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
        private void HandleResponse(AppCaller caller, ResponseMessage resMsg)
        {
            //调用参数
            var callArgs = new CallEventArgs(caller)
            {
                ElapsedTime = resMsg.ElapsedTime,
                Count = resMsg.Count,
                Error = resMsg.Error,
                Value = resMsg.Value
            };

            try
            {
                //开始调用
                if (callback != null)
                {
                    callback(callArgs);
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
