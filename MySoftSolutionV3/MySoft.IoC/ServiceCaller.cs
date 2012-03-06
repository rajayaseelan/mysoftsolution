using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller : IDisposable
    {
        private IServiceContainer container;
        private IDictionary<string, Type> callbackTypes;
        private ServerStatusService service;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="container"></param>
        /// <param name="service"></param>
        public ServiceCaller(IServiceContainer container, ServerStatusService service)
        {
            this.container = container;
            this.service = service;
            this.callbackTypes = GetCallbackTypes(container);

            //注册状态服务
            var hashtable = new Dictionary<Type, object>();
            hashtable[typeof(IStatusService)] = service;

            this.container.RegisterComponents(hashtable);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage CallMethod(IScsServerClient client, ScsResultMessage message)
        {
            ResponseMessage resMsg = null;

            try
            {
                var reqMsg = message.MessageValue as RequestMessage;

                //服务参数信息
                var caller = new AppCaller
                {
                    AppName = reqMsg.AppName,
                    IPAddress = reqMsg.IPAddress,
                    HostName = reqMsg.HostName,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters.ToString()
                };

                //判断是否为状态服务
                if (IsStatusService(reqMsg))
                {
                    //调用方法
                    resMsg = CallMethod(client, reqMsg, caller);
                }
                else
                {
                    //启动计时
                    var watch = Stopwatch.StartNew();

                    //调用方法
                    resMsg = CallMethod(client, reqMsg, caller);

                    //停止计时
                    watch.Stop();

                    var args = new CallEventArgs
                    {
                        CallTime = DateTime.Now,
                        Caller = caller,
                        Error = resMsg.Error,
                        ElapsedTime = watch.ElapsedMilliseconds,
                        Count = resMsg.Count,
                        Value = resMsg.Value
                    };

                    //如果是Json方式调用，则需要处理异常
                    if (resMsg.IsError && reqMsg.InvokeMethod)
                    {
                        resMsg.Parameters.Clear();
                        resMsg.Error = new ApplicationException(args.Error.Message);
                    }

                    //调用计数
                    service.CounterNotify(args);
                }
            }
            catch (Exception ex)
            {
                container.WriteError(ex);

                resMsg = new ResponseMessage
                {
                    TransactionId = new Guid(message.RepliedMessageId),
                    ReturnType = ex.GetType(),
                    Error = ex
                };
            }

            return resMsg;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private ResponseMessage CallMethod(IScsServerClient client, RequestMessage reqMsg, AppCaller caller)
        {
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(reqMsg.ServiceName))
                callbackType = callbackTypes[reqMsg.ServiceName];

            OperationContext.Current = new OperationContext(client, callbackType)
            {
                ServiceCache = container.ServiceCache,
                Container = container,
                Caller = caller
            };

            ResponseMessage resMsg = null;

            try
            {
                //获取返回结果
                resMsg = container.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                container.WriteError(ex);

                //处理异常
                resMsg = new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters,
                    ReturnType = reqMsg.ReturnType,
                    Error = ex
                };
            }

            return resMsg;
        }

        private IDictionary<string, Type> GetCallbackTypes(IServiceContainer container)
        {
            var dicTypes = new Dictionary<string, Type>();
            dicTypes[typeof(IStatusService).FullName] = typeof(IStatusListener);

            var types = container.GetServiceTypes<ServiceContractAttribute>();
            foreach (var type in types)
            {
                var contract = CoreHelper.GetMemberAttribute<ServiceContractAttribute>(type);
                if (contract != null && contract.CallbackType != null)
                {
                    dicTypes[type.FullName] = contract.CallbackType;
                }
            }

            return dicTypes;
        }

        /// <summary>
        /// 判断是否是状态服务
        /// </summary>
        /// <param name="reqMsg"></param>
        private bool IsStatusService(RequestMessage reqMsg)
        {
            return reqMsg.ServiceName == typeof(IStatusService).FullName;
        }

        #region IDisposable 成员

        public void Dispose()
        {
            callbackTypes.Clear();
            service = null;
        }

        #endregion
    }
}
