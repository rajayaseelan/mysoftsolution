using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

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
            //创建Caller;
            var caller = CreateCaller(client, reqMsg);

            //获取上下文
            var context = GetOperationContext(client, caller);

            try
            {
                //解析服务
                var service = ParseService(reqMsg, context);

                var asyncArgs = new AsyncCallerArgs { MessageId = messageId, Context = context, ReqMsg = reqMsg };
                var asyncCaller = new AsyncCaller(service);

                //异步调用
                asyncCaller.BeginDoTask(asyncArgs, AsyncCallback, new ArrayList { asyncArgs, asyncCaller });
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
        /// 异步回调
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var arr = ar.AsyncState as ArrayList;
            var asyncArgs = arr[0] as AsyncCallerArgs;
            var asyncCaller = arr[1] as AsyncCaller;

            var messageId = asyncArgs.MessageId;
            var client = asyncArgs.Context.ServerClient;
            var caller = asyncArgs.Context.Caller;
            var reqMsg = asyncArgs.ReqMsg;

            //响应结果，清理资源
            var resMsg = asyncCaller.EndDoTask(ar);

            if (resMsg == null) return;

            //调用参数
            var callArgs = new CallEventArgs
            {
                Caller = caller,
                ElapsedTime = resMsg.ElapsedTime,
                Count = resMsg.Count,
                Error = resMsg.Error,
                Value = resMsg.Value
            };

            //响应计数
            NotifyEventArgs(callArgs);

            //转换成秒判断
            if (TimeSpan.FromMilliseconds(resMsg.ElapsedTime).TotalSeconds > status.Config.Timeout)
            {
                //写超时日志
                WriteTimeoutLog(asyncArgs.Context, reqMsg, resMsg);
            }

            //如果是Json方式调用，则需要处理异常
            if (resMsg.IsError && reqMsg.InvokeMethod)
            {
                resMsg.Error = new ApplicationException(callArgs.Error.Message);
            }

            //发送消息
            SendMessage(client, reqMsg, resMsg, messageId);
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
        /// <param name="t_reqMsg"></param>
        /// <param name="t_resMsg"></param>
        private void WriteTimeoutLog(OperationContext context, RequestMessage t_reqMsg, ResponseMessage t_resMsg)
        {
            //调用计数
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var arr = state as ArrayList;
                    var reqMsg = arr[0] as RequestMessage;
                    var resMsg = arr[1] as ResponseMessage;

                    string body = string.Format("Remote client【{0}】call service ({1},{2}) timeout.\r\nParameters => {3}\r\nMessage => {4}",
                        reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), resMsg.Message);

                    //获取异常
                    var exception = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

                    //写异常日志
                    status.Container.WriteError(exception);
                }
                catch (Exception ex)
                {
                    //TODO
                }
            }, new ArrayList { t_reqMsg, t_resMsg });
        }

        /// <summary>
        /// 响应计数事件
        /// </summary>
        /// <param name="callArgs"></param>
        private void NotifyEventArgs(CallEventArgs callArgs)
        {
            //调用计数
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var arr = state as ArrayList;
                    var statusService = arr[0] as ServerStatusService;
                    var eventArgs = arr[1] as CallEventArgs;

                    //调用计数服务
                    statusService.Counter(eventArgs);

                    //响应消息
                    MessageCenter.Instance.Notify(eventArgs);
                }
                catch (Exception ex)
                {
                    //TODO
                }
            }, new ArrayList { status, callArgs });
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

    /// <summary>
    /// 回调参数
    /// </summary>
    public class AsyncCallerArgs
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 上下文对象
        /// </summary>
        public OperationContext Context { get; set; }

        /// <summary>
        /// 请求对象
        /// </summary>
        public RequestMessage ReqMsg { get; set; }
    }
}
