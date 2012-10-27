using System;
using System.Collections.Generic;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    internal class ServiceCaller : IDisposable
    {
        public event EventHandler<CallEventArgs> Handler;

        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, AsyncCaller> asyncCallers;
        private ServerStatusService status;
        private IServiceContainer container;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="status"></param>
        /// <param name="config"></param>
        /// <param name="container"></param>
        public ServiceCaller(ServerStatusService status, CastleServiceConfiguration config, IServiceContainer container)
        {
            this.status = status;
            this.container = container;
            this.callbackTypes = new Dictionary<string, Type>();
            this.asyncCallers = new Dictionary<string, AsyncCaller>();

            //注册状态服务
            var hashtable = new Dictionary<Type, object>();
            hashtable[typeof(IStatusService)] = status;

            //注册组件
            container.RegisterComponents(hashtable);

            //初始化服务
            InitServiceCaller(container, config);
        }

        private void InitServiceCaller(IServiceContainer container, CastleServiceConfiguration config)
        {
            callbackTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);

            var types = container.GetServiceTypes<ServiceContractAttribute>();
            var timeout = TimeSpan.FromSeconds(config.Timeout);

            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    callbackTypes[type.FullName] = contract.CallbackType;
                }

                IService service = null;
                string serviceKey = "Service_" + type.FullName;

                if (container.Kernel.HasComponent(serviceKey))
                {
                    service = container.Resolve<IService>(serviceKey);

                    //实例化AsyncCaller
                    if (config.EnableCache)
                        asyncCallers[type.FullName] = new AsyncCaller(service, timeout, null, true);
                    else
                        asyncCallers[type.FullName] = new AsyncCaller(service, timeout, true);
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reqMsg"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public void CallMethod(IScsServerClient channel, RequestMessage reqMsg, string messageId)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;
            AppCaller caller = null;
            OperationContext context = null;

            try
            {
                //创建Caller
                caller = CreateCaller(channel, reqMsg);

                //解析服务
                var asyncCaller = GetAsyncCaller(caller);

                //获取上下文
                context = GetOperationContext(channel, caller);

                //异步调用服务
                resMsg = asyncCaller.Run(context, reqMsg);
            }
            catch (Exception ex)
            {
                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }

            try
            {
                //判断返回的消息
                if (resMsg != null)
                {
                    //处理响应信息
                    resMsg = HandleResponse(caller, reqMsg, resMsg);

                    //发送消息
                    SendMessage(channel, reqMsg, resMsg, messageId);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                reqMsg = null;
                resMsg = null;
                caller = null;
                context = null;
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private ResponseMessage HandleResponse(AppCaller caller, RequestMessage reqMsg, ResponseMessage resMsg)
        {
            //响应及写超时信息
            CounterCaller(caller, resMsg);

            //如果是Json方式调用，则需要处理异常
            if (resMsg.IsError && reqMsg.InvokeMethod)
            {
                resMsg.Error = new ApplicationException(resMsg.Error.Message);
            }

            return resMsg;
        }

        /// <summary>
        /// Counter caller
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="resMsg"></param>
        private void CounterCaller(AppCaller caller, ResponseMessage resMsg)
        {
            //调用参数
            var callArgs = new CallEventArgs
            {
                Caller = caller,
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

                //输出信息
                if (Handler != null)
                {
                    try
                    {
                        Handler(container, callArgs);
                    }
                    catch (Exception ex)
                    {
                        //TODO
                    }
                }
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
        /// <param name="channel"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="messageId"></param>
        private void SendMessage(IScsServerClient channel, RequestMessage reqMsg, ResponseMessage resMsg, string messageId)
        {
            IScsMessage scsMessage = null;

            try
            {
                scsMessage = new ScsResultMessage(resMsg, messageId);

                //发送消息
                channel.SendMessage(scsMessage);
            }
            catch (Exception ex)
            {
                try
                {
                    resMsg = IoCHelper.GetResponse(reqMsg, ex);

                    scsMessage = new ScsResultMessage(resMsg, messageId);

                    //发送消息
                    channel.SendMessage(scsMessage);
                }
                catch (Exception e)
                {

                }
            }
        }

        /// <summary>
        /// 获取AppCaller
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private AppCaller CreateCaller(IScsServerClient channel, RequestMessage reqMsg)
        {
            //获取AppPath
            var appPath = (channel.UserToken == null) ? null : (channel.UserToken as AppClient).AppPath;

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
        /// <param name="channel"></param>
        /// <param name="caller"></param>
        private OperationContext GetOperationContext(IScsServerClient channel, AppCaller caller)
        {
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(caller.ServiceName))
            {
                callbackType = callbackTypes[caller.ServiceName];
            }

            return new OperationContext(channel, callbackType)
            {
                Container = container,
                Caller = caller
            };
        }

        /// <summary>
        /// Gets the asyncCaller.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private AsyncCaller GetAsyncCaller(AppCaller caller)
        {
            if (!asyncCallers.ContainsKey(caller.ServiceName))
            {
                string body = string.Format("The server【{1}({2})】not find matching service ({0})."
                    , caller.ServiceName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                //获取异常
                throw IoCHelper.GetException(caller, body);
            }

            return asyncCallers[caller.ServiceName];
        }

        #region IDisposable 成员

        /// <summary>
        /// Disposes this object and closes underlying connection.
        /// </summary>
        public void Dispose()
        {
            Handler = null;
            callbackTypes.Clear();
            asyncCallers.Clear();

            callbackTypes = null;
            asyncCallers = null;
        }

        #endregion
    }
}
