using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller
    {
        private IDictionary<string, Type> callbackTypes;
        private ServerStatusService status;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="status"></param>
        public ServiceCaller(ServerStatusService status)
        {
            this.status = status;
            this.callbackTypes = new Dictionary<string, Type>();

            //初始化服务
            InitServiceCaller(status.Container);

            //注册状态服务
            var hashtable = new Dictionary<Type, object>();
            hashtable[typeof(IStatusService)] = status;

            //注册组件
            status.Container.RegisterComponents(hashtable);
        }

        private void InitServiceCaller(IServiceContainer container)
        {
            callbackTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);

            var types = container.GetServiceTypes<ServiceContractAttribute>();
            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public void CallMethod(IScsServerClient client, RequestMessage reqMsg, string messageId)
        {
            //如果是断开状态，直接返回
            if (client.CommunicationState == CommunicationStates.Disconnected)
                return;

            try
            {
                var arr = new ArrayList { client, reqMsg, messageId };

                //状态服务
                if (reqMsg.InvokeMethod || reqMsg.ServiceName == typeof(IStatusService).FullName)
                {
                    ThreadPool.QueueUserWorkItem(AsyncCallback, arr);
                }
                else
                {
                    ManagedThreadPool.QueueUserWorkItem(AsyncCallback, arr);
                }
            }
            catch (Exception ex)
            {
                //将异常信息写出
                status.Container.WriteError(ex);

                //处理异常
                var resMsg = IoCHelper.GetResponse(reqMsg, ex);

                //发送消息
                SendMessage(client, reqMsg, resMsg, messageId);
            }
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="state"></param>
        private void AsyncCallback(object state)
        {
            var arr = state as ArrayList;
            var client = arr[0] as IScsServerClient;
            var reqMsg = arr[1] as RequestMessage;
            var messageId = Convert.ToString(arr[2]);

            //定义响应的消息
            ResponseMessage resMsg = null;

            //创建Caller;
            var caller = CreateCaller(client, reqMsg);

            //获取上下文
            var context = GetOperationContext(client, caller);

            try
            {
                //Console.WriteLine("{0} => {1}:{2}", DateTime.Now, reqMsg.ServiceName, reqMsg.MethodName);

                //获取响应
                resMsg = GetResponseMessage(context, reqMsg);

                if (resMsg == null) return;

                //转换成毫秒判断
                if (resMsg.ElapsedTime > TimeSpan.FromSeconds(status.Config.Timeout).TotalMilliseconds)
                {
                    //写超时日志
                    WriteTimeout(context, reqMsg, resMsg);
                }

                //响应及写超时信息
                CounterCaller(context, resMsg);

                //如果是Json方式调用，则需要处理异常
                if (resMsg.IsError && reqMsg.InvokeMethod)
                {
                    resMsg.Error = new ApplicationException(resMsg.Error.Message);
                }

                //发送消息
                SendMessage(client, reqMsg, resMsg, messageId);
            }
            catch (Exception ex)
            {
                //TODO
            }
            finally
            {
                caller = null;
                context = null;
                reqMsg = null;
                resMsg = null;
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseMessage(OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            // Register wait for single object
            if (reqMsg.Timeout <= 0) reqMsg.Timeout = ServiceConfig.DEFAULT_CALL_TIMEOUT;  //最小为30秒
            var timeout = (int)TimeSpan.FromSeconds(reqMsg.Timeout).TotalMilliseconds / 2;
            var manualReset = new ManualResetEvent(false);
            ThreadPool.RegisterWaitForSingleObject(manualReset, TimerCallback, Thread.CurrentThread, timeout, true);

            try
            {
                OperationContext.Current = context;

                //解析服务
                var service = ParseService(reqMsg, context);

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (ThreadAbortException ex)
            {
                //线程重置
                Thread.ResetAbort();

                var body = string.Format("Remote client【{0}】call service ({1},{2}) timeout {4} ms, the request is aborted..\r\nParameters => {3}",
                    reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), timeout);

                //获取异常
                var error = IoCHelper.GetException(context, reqMsg, new ThreadException(body, ex));

                status.Container.WriteError(error);

                var title = string.Format("Call remote service ({0},{1}) timeout {2} ms, the request is aborted.",
                            reqMsg.ServiceName, reqMsg.MethodName, timeout);

                //处理异常
                error = new ThreadException(title, new TimeoutException(body));

                resMsg = IoCHelper.GetResponse(reqMsg, error);
            }
            catch (Exception ex)
            {
                //将异常信息写出
                status.Container.WriteError(ex);

                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                manualReset.Set();

                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// Call method timer callback
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timedOut"></param>
        private void TimerCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                try
                {
                    var thread = state as Thread;

                    //判断是否为激活状态
                    if (thread.IsAlive)
                    {
                        var ts = SimpleThreadState(thread.ThreadState);

                        if (ts == ThreadState.WaitSleepJoin || ts == ThreadState.Running)
                        {
                            thread.Abort();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private ThreadState SimpleThreadState(ThreadState ts)
        {
            return ts & (ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin |
                         ThreadState.Stopped);
        }

        /// <summary>
        /// Counter caller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resMsg"></param>
        private void CounterCaller(OperationContext context, ResponseMessage resMsg)
        {
            //调用参数
            var callArgs = new CallEventArgs
            {
                Caller = context.Caller,
                ElapsedTime = resMsg.ElapsedTime,
                Count = resMsg.Count,
                Error = resMsg.Error,
                Value = resMsg.Value
            };

            try
            {
                //调用计数服务
                status.Counter(callArgs);

                //响应消息
                MessageCenter.Instance.Notify(callArgs);
            }
            catch (Exception ex)
            {
                //TODO
            }
            finally
            {
                callArgs = null;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="messageId"></param>
        private void SendMessage(IScsServerClient client, RequestMessage reqMsg, ResponseMessage resMsg, string messageId)
        {
            try
            {
                var sendMsg = new ScsResultMessage(resMsg, messageId);

                //发送消息
                client.SendMessage(sendMsg);
            }
            catch (Exception ex)
            {
                //写异常日志
                status.Container.WriteError(ex);

                try
                {
                    resMsg = IoCHelper.GetResponse(reqMsg, ex);

                    var sendMsg = new ScsResultMessage(resMsg, messageId);

                    //发送消息
                    client.SendMessage(sendMsg);
                }
                catch (Exception e)
                {
                    //写异常日志
                    status.Container.WriteError(e);
                }
            }
        }

        /// <summary>
        /// 写超时日志
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        private void WriteTimeout(OperationContext context, RequestMessage reqMsg, ResponseMessage resMsg)
        {
            try
            {
                //调用计数
                string body = string.Format("Remote client【{0}】call service ({1},{2}) timeout.\r\nParameters => {3}\r\nMessage => {4}",
                    reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), resMsg.Message);

                //获取异常
                var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

                //写异常日志
                status.Container.WriteError(error);
            }
            catch (Exception ex)
            {
                //TODO
            }
        }

        /// <summary>
        /// 获取AppCaller
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private AppCaller CreateCaller(IScsServerClient client, RequestMessage reqMsg)
        {
            //获取AppPath
            var appPath = (client.State == null) ? null : (client.State as AppClient).AppPath;

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

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <param name="client"></param>
        /// <param name="caller"></param>
        private OperationContext GetOperationContext(IScsServerClient client, AppCaller caller)
        {
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(caller.ServiceName))
            {
                callbackType = callbackTypes[caller.ServiceName];
            }

            return new OperationContext(client, callbackType)
            {
                Container = status.Container,
                Caller = caller
            };
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IService ParseService(RequestMessage reqMsg, OperationContext context)
        {
            IService service = null;
            string serviceKey = "Service_" + reqMsg.ServiceName;

            if (status.Container.Kernel.HasComponent(serviceKey))
            {
                service = status.Container.Resolve<IService>(serviceKey);
            }

            if (service == null)
            {
                string body = string.Format("The server【{1}({2})】not find matching service ({0})."
                    , reqMsg.ServiceName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                //获取异常
                throw IoCHelper.GetException(context, reqMsg, new WarningException(body));
            }

            return service;
        }
    }
}
