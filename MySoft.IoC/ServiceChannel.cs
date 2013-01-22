using System;
using System.Net.Sockets;
using System.Threading;
using Amib.Threading;
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
        private Action<CallEventArgs> callback;
        private ServiceCaller caller;
        private ServerStatusService status;
        private SmartThreadPool stp;
        private IWorkItemsGroup group;
        private int timeout;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="maxCaller"></param>
        /// <param name="timeout"></param>
        public ServiceChannel(Action<CallEventArgs> callback, ServiceCaller caller, ServerStatusService status, int maxCaller, int timeout)
        {
            this.callback = callback;
            this.caller = caller;
            this.status = status;
            this.stp = new SmartThreadPool(new STPStartInfo
            {
                IdleTimeout = timeout * 1000,
                DisposeOfStateObjects = true,
                CallToPostExecute = CallToPostExecute.Always,
                ThreadPriority = ThreadPriority.Highest,
                UseCallerCallContext = false,
                UseCallerHttpContext = false,
                MaxWorkerThreads = maxCaller
            });

            //创建一个工作组
            this.group = stp.CreateWorkItemsGroup(3);
            this.group.Start();

            this.timeout = timeout;
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void SendResponse(IScsServerClient channel, CallerContext e)
        {
            //创建一个工作项
            var item = group.QueueWorkItem((_channel, _context) =>
            {
                //调用响应信息
                return caller.InvokeResponse(_channel, _context.Caller, _context.Request);

            }, channel, e);

            try
            {
                //获取响应结果
                e.Message = item.GetResult(TimeSpan.FromSeconds(timeout), true);
            }
            catch (WorkItemTimeoutException ex) //超时异常
            {
                //结束进程
                item.Cancel(true);

                //获取异常响应
                e.Message = GetTimeoutResponse(e.Request, TimeSpan.FromSeconds(timeout), ex.Message);
            }
            catch (WorkItemResultException ex) //获取结果异常
            {
                //获取异常响应
                e.Message = IoCHelper.GetResponse(e.Request, ex.InnerException);
            }
            catch (Exception ex) //其它异常
            {
                //获取异常响应
                e.Message = IoCHelper.GetResponse(e.Request, ex);
            }

            //处理上下文
            HandleCallerContext(channel, e);
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="timeout"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg, TimeSpan timeout, string message)
        {
            //获取异常响应信息
            var body = string.Format("Async call service ({0}, {1})  timeout ({2}) ms. {3}",
                        reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds, message);

            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(body));

            //设置耗时时间
            resMsg.ElapsedTime = (long)timeout.TotalMilliseconds;

            return resMsg;
        }

        /// <summary>
        /// 响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        private void HandleCallerContext(IScsServerClient channel, CallerContext e)
        {
            if (e.Message == null) return;

            //状态服务跳过
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
                var message = new ScsResultMessage(e.Message, e.MessageId);

                //发送消息
                channel.SendMessage(message);
            }
            catch (CommunicationException ex) { }
            catch (ObjectDisposedException ex) { }
            catch (InvalidOperationException ex) { }
            catch (SocketException ex) { }
            catch (Exception ex)
            {
                //获取异常响应
                var body = string.Format("Sending messages error: {0}, service: ({1}, {2})"
                                        , ex.ToString() //ErrorHelper.GetInnerException(ex).Message
                                        , e.Caller.ServiceName, e.Caller.MethodName);

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
                this.group.WaitForIdle();
                this.stp.Shutdown();
                this.stp.Dispose();
            }
            catch (Exception ex) { }
            finally
            {
                this.stp = null;
                this.callback = null;
                this.caller = null;
                this.status = null;
            }
        }

        #endregion
    }
}
