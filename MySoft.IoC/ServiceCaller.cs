using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

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

            //创建Caller;
            var caller = CreateCaller(client, reqMsg);

            //如果是状态服务，直接响应
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
            {
                SyncCallMethod(client, caller, reqMsg, messageId);
            }
            else
            {
                AsyncCallMethod(client, caller, reqMsg, messageId);
            }
        }

        /// <summary>
        /// 同步调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="caller"></param>
        /// <param name="reqMsg"></param>
        /// <param name="messageId"></param>
        private void SyncCallMethod(IScsServerClient client, AppCaller caller, RequestMessage reqMsg, string messageId)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            var context = GetOperationContext(client, caller);

            try
            {
                try
                {
                    //获取上下文
                    OperationContext.Current = context;

                    //解析服务
                    var service = ParseService(reqMsg, context);

                    resMsg = service.CallService(reqMsg);
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
                    OperationContext.Current = null;
                }

                //发送消息
                SendMessage(client, reqMsg, resMsg, messageId);
            }
            finally
            {
                context = null;
                client = null;
                reqMsg = null;
                resMsg = null;
            }
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="caller"></param>
        /// <param name="reqMsg"></param>
        /// <param name="messageId"></param>
        private void AsyncCallMethod(IScsServerClient client, AppCaller caller, RequestMessage reqMsg, string messageId)
        {
            try
            {
                var cacheKey = string.Format("Caller_{0}_{1}_{2}", caller.ServiceName, caller.MethodName, caller.Parameters);
                var resMsg = CacheHelper.Get<ResponseMessage>(cacheKey);

                if (resMsg == null)
                {
                    //调用方法
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        //获取上下文
                        var context = GetOperationContext(client, caller);

                        try
                        {
                            //获取响应
                            resMsg = GetResponseMessage(context, reqMsg);

                            //转换成毫秒判断
                            if (resMsg.ElapsedTime > TimeSpan.FromSeconds(status.Config.Timeout).TotalMilliseconds)
                            {
                                //写超时日志
                                WriteTimeout(context, reqMsg, resMsg);

                                //超过超时时间的数据，插入本地缓存（非错误数据）
                                if (!resMsg.IsError)
                                {
                                    CacheHelper.Insert(cacheKey, resMsg, status.Config.Timeout);
                                }
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
                        finally
                        {
                            context = null;
                            client = null;
                            reqMsg = null;
                            resMsg = null;
                        }
                    });
                }
                else
                {
                    //将传入的Id赋值给返回Id
                    resMsg.TransactionId = reqMsg.TransactionId;

                    //发送消息
                    SendMessage(client, reqMsg, resMsg, messageId);
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
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseMessage(OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            OperationContext.Current = context;

            try
            {
                // Register wait for single object
                if (reqMsg.Timeout <= 0) reqMsg.Timeout = ServiceConfig.DEFAULT_CALL_TIMEOUT;  //最小为30秒

                var mre = new ManualResetEvent(false);
                var timeout = (int)TimeSpan.FromSeconds(reqMsg.Timeout).TotalMilliseconds / 2;
                ThreadPool.RegisterWaitForSingleObject(mre, TimerCallback, Thread.CurrentThread, timeout, true);

                try
                {
                    //解析服务
                    var service = ParseService(reqMsg, context);

                    //响应结果，清理资源
                    resMsg = service.CallService(reqMsg);
                }
                catch (ThreadAbortException e)
                {
                    Thread.ResetAbort();

                    string body = string.Format("Remote client【{0}】call service ({1},{2}) timeout {4} ms, the request is aborted.\r\nParameters => {3}",
                                    reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), timeout);

                    //获取异常
                    var error = IoCHelper.GetException(context, reqMsg, new ThreadException(body, e));

                    status.Container.WriteError(error);

                    //处理异常
                    resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(body));
                }
                finally
                {
                    mre.Set();
                }
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
                    var ts = SimpleThreadState(thread.ThreadState);

                    if (ts == ThreadState.WaitSleepJoin || ts == ThreadState.Running)
                    {
                        thread.Abort();
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

            //异步处理
            ThreadPool.QueueUserWorkItem(obj =>
            {
                try
                {
                    //调用计数服务
                    status.Counter(callArgs);

                    //响应消息
                    MessageCenter.Instance.Notify(callArgs);
                }
                finally
                {
                    callArgs = null;
                }
            });
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
                catch
                {
                    //写异常日志
                    status.Container.WriteError(ex);
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
            //调用计数
            string body = string.Format("Remote client【{0}】call service ({1},{2}) timeout.\r\nParameters => {3}\r\nMessage => {4}",
                reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), resMsg.Message);

            //获取异常
            var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

            //写异常日志
            status.Container.WriteError(error);
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
