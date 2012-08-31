using System;
using System.Collections.Generic;
using MySoft.IoC.Callback;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller
    {
        private IDictionary<string, Type> callbackTypes;
        private IDictionary<string, AsyncCaller> asyncCallers;
        private ServerStatusService status;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="status"></param>
        public ServiceCaller(ServerStatusService status)
        {
            this.status = status;
            this.callbackTypes = new Dictionary<string, Type>();
            this.asyncCallers = new Dictionary<string, AsyncCaller>();

            //注册状态服务
            var hashtable = new Dictionary<Type, object>();
            hashtable[typeof(IStatusService)] = status;

            //注册组件
            status.Container.RegisterComponents(hashtable);

            //初始化服务
            InitServiceCaller(status.Container);
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

                IService service = null;
                string serviceKey = "Service_" + type.FullName;

                if (container.Kernel.HasComponent(serviceKey))
                {
                    service = container.Resolve<IService>(serviceKey);
                }

                if (service != null)
                {
                    //计算超时时间
                    var elapsedTime = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_SERVER_CALL_TIMEOUT);

                    //实例化AsyncCaller
                    asyncCallers[type.FullName] = new AsyncCaller(container, service, elapsedTime, true);
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

            //定义响应的消息
            ResponseMessage resMsg = null;

            try
            {
                //创建Caller;
                var caller = CreateCaller(client, reqMsg);

                //获取上下文
                var context = GetOperationContext(client, caller);

                //解析服务
                var asyncCaller = GetAsyncCaller(reqMsg, context);

                //异步调用服务
                resMsg = asyncCaller.AsyncCall(context, reqMsg);

                //判断返回的消息
                if (resMsg != null)
                {
                    //处理响应信息
                    resMsg = HandleResponse(context, reqMsg, resMsg);
                }
            }
            catch (Exception ex)
            {
                //将异常信息写出
                status.Container.WriteError(ex);

                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }

            //判断返回的消息
            if (resMsg != null)
            {
                //发送消息
                SendMessage(client, reqMsg, resMsg, messageId);
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private ResponseMessage HandleResponse(OperationContext context, RequestMessage reqMsg, ResponseMessage resMsg)
        {
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

            return resMsg;
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
            IScsMessage scsMessage = null;

            try
            {
                scsMessage = new ScsResultMessage(resMsg, messageId);

                //发送消息
                client.SendMessage(scsMessage);
            }
            catch (Exception ex)
            {
                try
                {
                    resMsg = IoCHelper.GetResponse(reqMsg, ex);

                    scsMessage = new ScsResultMessage(resMsg, messageId);

                    //发送消息
                    client.SendMessage(scsMessage);
                }
                catch (Exception e)
                {

                }
            }
            finally
            {
                scsMessage = null;

                reqMsg = null;
                resMsg = null;
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

                //写异常日志
                SimpleLog.Instance.WriteLogForDir(string.Format("Timeout\\{0}\\{1}", reqMsg.AppName, reqMsg.ServiceName), body);
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
            var appPath = (client.ClientState == null) ? null : (client.ClientState as AppClient).AppPath;

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
        /// Gets the asyncCaller.
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private AsyncCaller GetAsyncCaller(RequestMessage reqMsg, OperationContext context)
        {
            if (!asyncCallers.ContainsKey(reqMsg.ServiceName))
            {
                string body = string.Format("The server【{1}({2})】not find matching service ({0})."
                    , reqMsg.ServiceName, DnsHelper.GetHostName(), DnsHelper.GetIPAddress());

                //获取异常
                throw IoCHelper.GetException(context, reqMsg, new WarningException(body));
            }

            return asyncCallers[reqMsg.ServiceName];
        }
    }
}
