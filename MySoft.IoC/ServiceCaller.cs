using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Threading;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller
    {
        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, int> callTimeouts;
        private IWorkItemsGroup smart;
        private ServerStatusService status;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="group"></param>
        /// <param name="status"></param>
        public ServiceCaller(IWorkItemsGroup group, ServerStatusService status)
        {
            this.status = status;
            this.smart = group;
            this.callbackTypes = new Dictionary<string, Type>();
            this.callTimeouts = new Dictionary<string, int>();

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
                if (contract != null)
                {
                    if (contract.CallbackType != null)
                        callbackTypes[type.FullName] = contract.CallbackType;

                    if (contract.Timeout > 0)
                        callTimeouts[type.FullName] = contract.Timeout;
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage CallMethod(IScsServerClient client, RequestMessage reqMsg)
        {
            //创建Caller;
            var caller = CreateCaller(client, reqMsg);

            //设置上下文
            SetOperationContext(client, caller);

            try
            {
                //处理状态服务
                if (reqMsg.ServiceName == typeof(IStatusService).FullName)
                {
                    var s = ParseService(reqMsg);

                    //调用服务
                    return s.CallService(reqMsg);
                }
                else
                {
                    //创建服务
                    var service = CreateService(reqMsg);

                    //启动计时
                    var watch = Stopwatch.StartNew();

                    //调用服务
                    var resMsg = service.CallService(reqMsg);

                    //停止计时
                    watch.Stop();

                    //调用参数
                    var callArgs = new CallEventArgs
                    {
                        Caller = caller,
                        ElapsedTime = watch.ElapsedMilliseconds,
                        Count = resMsg.Count,
                        Error = resMsg.Error,
                        Value = resMsg.Value
                    };

                    //响应计数
                    NotifyEventArgs(callArgs);

                    //如果是Json方式调用，则需要处理异常
                    if (resMsg.IsError && reqMsg.InvokeMethod)
                    {
                        resMsg.Error = new ApplicationException(callArgs.Error.Message);
                    }

                    return resMsg;
                }
            }
            finally
            {
                //初始化上下文
                OperationContext.Current = null;
            }
        }

        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private IService CreateService(RequestMessage reqMsg)
        {
            //等待超时
            var timeSpan = TimeSpan.FromSeconds(status.Config.Timeout);
            if (callTimeouts.ContainsKey(reqMsg.ServiceName))
            {
                timeSpan = TimeSpan.FromSeconds(callTimeouts[reqMsg.ServiceName]);
            }

            //解析服务
            var service = ParseService(reqMsg);

            return new AsyncService(smart, status.Container, service, timeSpan);
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
        /// 设置上下文
        /// </summary>
        /// <param name="client"></param>
        /// <param name="caller"></param>
        private void SetOperationContext(IScsServerClient client, AppCaller caller)
        {
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(caller.ServiceName))
            {
                callbackType = callbackTypes[caller.ServiceName];
            }

            OperationContext.Current = new OperationContext(client, callbackType)
            {
                Container = status.Container,
                Caller = caller
            };
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private IService ParseService(RequestMessage reqMsg)
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
                throw IoCHelper.GetException(OperationContext.Current, reqMsg, body);
            }

            return service;
        }
    }
}
