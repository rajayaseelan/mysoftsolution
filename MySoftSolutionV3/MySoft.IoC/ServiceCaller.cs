using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Callback;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller
    {
        private IServiceContainer container;
        private CastleServiceConfiguration config;
        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, int> callTimeouts;
        private ServerStatusService status;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="server"></param>
        /// <param name="container"></param>
        /// <param name="config"></param>
        public ServiceCaller(IScsServer server, CastleServiceConfiguration config, IServiceContainer container)
        {
            this.status = new ServerStatusService(server, config, container);
            this.config = config;
            this.container = container;
            this.callbackTypes = new Dictionary<string, Type>();
            this.callTimeouts = new Dictionary<string, int>();

            //初始化服务
            InitServiceCaller(container);

            //注册状态服务
            var hashtable = new Dictionary<Type, object>();
            hashtable[typeof(IStatusService)] = status;

            this.container.RegisterComponents(hashtable);
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
            try
            {
                //创建Caller;
                var caller = CreateCaller(client, reqMsg);

                //设置上下文
                SetOperationContext(client, caller);

                //启动计时
                var watch = Stopwatch.StartNew();

                //调用方法
                var resMsg = CallAsyncMethod(reqMsg);

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

                //如果是Json方式调用，则需要处理异常
                if (resMsg.IsError && reqMsg.InvokeMethod)
                {
                    resMsg.Error = new ApplicationException(callArgs.Error.Message);
                }

                //调用计数
                ManagedThreadPool.QueueUserWorkItem(state =>
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

                return resMsg;
            }
            catch (Exception ex)
            {
                //输出错误
                container.Write(ex);

                //处理异常
                return new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = reqMsg.ReturnType,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters,
                    Error = ex
                };
            }
            finally
            {
                //初始化上下文
                OperationContext.Current = null;
            }
        }

        /// <summary>
        /// 调用异步方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage CallAsyncMethod(RequestMessage reqMsg)
        {
            //等待超时
            var time = TimeSpan.FromSeconds(config.Timeout);
            if (callTimeouts.ContainsKey(reqMsg.ServiceName))
            {
                time = TimeSpan.FromSeconds(callTimeouts[reqMsg.ServiceName]);
            }

            //解析服务
            var service = ParseService(reqMsg);
            service = new AsyncService(container, service, time);

            //调用服务
            return service.CallService(reqMsg);
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
                Container = container,
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

            if (container.Kernel.HasComponent(serviceKey))
            {
                service = container.Resolve<IService>(serviceKey);
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
