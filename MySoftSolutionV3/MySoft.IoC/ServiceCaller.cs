using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using System.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller : IDisposable
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
        public ServiceCaller(IScsServer server, IServiceContainer container, CastleServiceConfiguration config)
        {
            this.status = new ServerStatusService(server, container, config);
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
                //设置上下文
                SetOperationContext(client, reqMsg);

                //判断是否为状态服务
                if (IsStatusService(reqMsg))
                {
                    //调用方法
                    return CallMethod(reqMsg, false);
                }
                else
                {
                    //启动计时
                    var watch = Stopwatch.StartNew();

                    //调用方法
                    var resMsg = CallMethod(reqMsg, true);

                    //停止计时
                    watch.Stop();

                    var callArgs = new CallEventArgs
                    {
                        CallTime = DateTime.Now,
                        Caller = OperationContext.Current.Caller,
                        Error = resMsg.Error,
                        ElapsedTime = watch.ElapsedMilliseconds,
                        Count = resMsg.Count,
                        Value = resMsg.Value
                    };

                    //如果是Json方式调用，则需要处理异常
                    if (resMsg.IsError && reqMsg.Invoked)
                    {
                        resMsg.Parameters.Clear();
                        resMsg.Error = new ApplicationException(callArgs.Error.Message);
                    }

                    //调用计数
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        try
                        {
                            var eventArgs = state as CallEventArgs;
                            status.CounterNotify(eventArgs);
                        }
                        catch (Exception ex)
                        {
                            container.WriteError(ex);
                            //To Do
                        }
                    }, callArgs);

                    return resMsg;
                }
            }
            catch (Exception ex)
            {
                //输出错误
                container.WriteError(ex);

                //处理异常
                return new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters,
                    ReturnType = reqMsg.ReturnType,
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
        /// 服务集合
        /// </summary>
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 调用异步方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        private ResponseMessage CallMethod(RequestMessage reqMsg, bool isAsync)
        {
            //定义服务
            IService service = null;

            //判断是否是异步服务
            if (!isAsync)
            {
                //解析服务
                service = ParseService(reqMsg);
            }
            else
            {
                var queueKey = string.Format("{0}_{1}", reqMsg.ServiceName, reqMsg.MethodName);

                //同一方法使用同一Queue处理
                if (!hashtable.ContainsKey(queueKey))
                {
                    //等待超时
                    var time = TimeSpan.FromSeconds(config.Timeout);
                    if (callTimeouts.ContainsKey(reqMsg.ServiceName))
                    {
                        time = TimeSpan.FromSeconds(callTimeouts[reqMsg.ServiceName]);
                    }

                    //解析服务
                    var s = ParseService(reqMsg);
                    hashtable[queueKey] = new QueueService(s, container, time);
                }

                service = hashtable[queueKey] as IService;
            }

            //调用服务
            return service.CallService(reqMsg);
        }

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private void SetOperationContext(IScsServerClient client, RequestMessage reqMsg)
        {
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(reqMsg.ServiceName))
            {
                callbackType = callbackTypes[reqMsg.ServiceName];
            }

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
            var service = container.Resolve<IService>("Service_" + reqMsg.ServiceName);
            if (service == null)
            {
                string body = string.Format("The server not find matching service ({0}).", reqMsg.ServiceName);
                throw new WarningException(body)
                {
                    ApplicationName = reqMsg.AppName,
                    ServiceName = reqMsg.ServiceName,
                    ErrorHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                };
            }

            return service;
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
            callTimeouts.Clear();
            status = null;
        }

        #endregion
    }
}
