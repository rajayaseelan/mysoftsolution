using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 异步调用委托
    /// </summary>
    /// <param name="context"></param>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    public delegate ResponseMessage AsyncMethodCaller(OperationContext context, RequestMessage reqMsg);

    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller : IDisposable
    {
        private IServiceContainer container;
        private int timeout;
        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, int> callTimeouts;
        private ServerStatusService service;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="container"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        public ServiceCaller(ServerStatusService service, IServiceContainer container, int timeout)
        {
            this.container = container;
            this.timeout = timeout;
            this.service = service;
            this.callbackTypes = new Dictionary<string, Type>();
            this.callTimeouts = new Dictionary<string, int>();

            //初始化服务
            InitServiceCaller(container);

            //注册状态服务
            var hashtable = new Dictionary<Type, object>();
            hashtable[typeof(IStatusService)] = service;

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
            ResponseMessage resMsg = null;

            try
            {
                //设置上下文
                SetOperationContext(client, reqMsg);

                //判断是否为状态服务
                if (IsStatusService(reqMsg))
                {
                    //调用方法
                    resMsg = AsyncCallMethod(reqMsg);
                }
                else
                {
                    //启动计时
                    var watch = Stopwatch.StartNew();

                    //调用方法
                    resMsg = AsyncCallMethod(reqMsg);

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
                    service.CounterNotify(callArgs);
                }
            }
            catch (Exception ex)
            {
                //输出错误
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
            finally
            {
                //初始化上下文
                OperationContext.Current = null;
            }

            return resMsg;
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
        /// 异步调用方法
        /// </summary>
        /// <param name="reqMsg"></param>
        public ResponseMessage AsyncCallMethod(RequestMessage reqMsg)
        {
            //实例化异步调用委托
            var caller = new AsyncMethodCaller((context, req) =>
            {
                //设置上下文
                OperationContext.Current = context;

                //解析服务
                var service = ParseService(reqMsg);

                //返回调用值
                return service.CallService(req);
            });

            //异常调用
            var ar = caller.BeginInvoke(OperationContext.Current, reqMsg, p => { }, caller);

            //等待超时
            var elapsedTime = TimeSpan.FromSeconds(timeout);
            if (callTimeouts.ContainsKey(reqMsg.ServiceName))
            {
                elapsedTime = TimeSpan.FromSeconds(callTimeouts[reqMsg.ServiceName]);
            }

            //信号等待
            if (!ar.AsyncWaitHandle.WaitOne(elapsedTime))
            {
                throw new WarningException(string.Format("Call service ({0}, {1}) timeout ({2}) ms."
                    , reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds));
            }

            //关闭
            ar.AsyncWaitHandle.Close();

            //释放资源
            return caller.EndInvoke(ar);
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
            service = null;
        }

        #endregion
    }
}
