using System;
using System.Collections.Generic;
using MySoft.Cache;
using MySoft.IoC.Configuration;
using MySoft.IoC.Logger;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using System.Diagnostics;

namespace MySoft.IoC
{
    /// <summary>
    /// The base impl class of the service interface, this class is used by service factory to emit service interface impl automatically at runtime.
    /// </summary>
    public class ServiceInvocationHandler : IProxyInvocationHandler
    {
        private CastleFactoryConfiguration config;
        private IDictionary<string, int> cacheTimes;
        private IDictionary<string, string> errors;
        private IServiceContainer container;
        private AsyncCaller asyncCaller;
        private IService service;
        private Type serviceType;
        private IServiceLog logger;
        private string hostName;
        private string ipAddress;

        /// <summary>
        ///  Initializes a new instance of the <see cref="ServiceInvocationHandler"/> class.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container"></param>
        /// <param name="service"></param>
        /// <param name="serviceType"></param>
        /// <param name="cache"></param>
        public ServiceInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, IService service, Type serviceType, ICacheStrategy cache, IServiceLog logger)
        {
            this.config = config;
            this.container = container;
            this.serviceType = serviceType;
            this.service = service;
            this.logger = logger;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();

            this.cacheTimes = new Dictionary<string, int>();
            this.errors = new Dictionary<string, string>();

            var waitTime = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_WAIT_TIMEOUT);

            //实例化异步服务
            if (config.EnableCache)
                this.asyncCaller = new AsyncCaller(container, service, waitTime, cache, false);
            else
                this.asyncCaller = new AsyncCaller(container, service, waitTime, false);

            var methods = CoreHelper.GetMethodsFromType(serviceType);
            foreach (var method in methods)
            {
                var contract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
                if (contract != null)
                {
                    if (contract.CacheTime > 0)
                        cacheTimes[method.ToString()] = contract.CacheTime;

                    if (!string.IsNullOrEmpty(contract.ErrorMessage))
                        errors[method.ToString()] = contract.ErrorMessage;
                }
            }
        }

        #region IInvocationHandler 成员

        /// <summary>
        /// 响应委托
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, System.Reflection.MethodInfo method, object[] parameters)
        {
            #region 设置请求信息

            var collection = IoCHelper.CreateParameters(method, parameters);

            var reqMsg = new RequestMessage
            {
                AppName = config.AppName,                       //应用名称
                HostName = hostName,                            //客户端名称
                IPAddress = ipAddress,                          //客户端IP地址
                ReturnType = method.ReturnType,                 //返回类型
                ServiceName = serviceType.FullName,             //服务名称
                MethodName = method.ToString(),                 //方法名称
                TransactionId = Guid.NewGuid(),                 //传输ID号
                MethodInfo = method,                            //设置调用方法
                Parameters = collection,                        //设置参数
                TransferType = TransferType.Binary              //数据类型
            };

            //设置缓存时间
            if (cacheTimes.ContainsKey(method.ToString()))
            {
                reqMsg.CacheTime = cacheTimes[method.ToString()];
            }

            #endregion

            //调用方法
            var resMsg = CallService(reqMsg);

            //定义返回值
            object returnValue = null;

            if (resMsg != null)
            {
                returnValue = resMsg.Value;

                //处理参数
                IoCHelper.SetRefParameters(method, resMsg.Parameters, parameters);
            }

            //返回结果
            return returnValue;
        }

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">Name of the sub service.</param>
        /// <returns>The result.</returns>
        protected virtual ResponseMessage CallService(RequestMessage reqMsg)
        {
            ResponseMessage resMsg = null;

            try
            {
                //写日志开始
                logger.BeginRequest(reqMsg);

                //获取上下文
                var context = GetOperationContext(reqMsg);

                //开始计时
                var watch = Stopwatch.StartNew();

                try
                {
                    //异步调用服务
                    resMsg = asyncCaller.AsyncCall(context, reqMsg);
                }
                finally
                {
                    watch.Stop();
                }

                //写日志结束
                logger.EndRequest(reqMsg, resMsg, watch.ElapsedMilliseconds);

                //如果有异常，向外抛出
                if (resMsg.IsError) throw resMsg.Error;
            }
            catch (BusinessException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                if (config.ThrowError)
                {
                    //判断是否有自定义异常
                    if (errors.ContainsKey(reqMsg.MethodName))
                        throw new BusinessException(errors[reqMsg.MethodName]);
                    else
                        throw ex;
                }
            }

            return resMsg;
        }

        /// <summary>
        /// 获取上下文对象
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private OperationContext GetOperationContext(RequestMessage reqMsg)
        {
            var caller = new AppCaller
            {
                AppPath = AppDomain.CurrentDomain.BaseDirectory,
                AppName = reqMsg.AppName,
                IPAddress = reqMsg.IPAddress,
                HostName = reqMsg.HostName,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName,
                Parameters = reqMsg.Parameters.ToString(),
                CallTime = DateTime.Now
            };

            return new OperationContext
            {
                Container = container,
                Caller = caller
            };
        }

        #endregion
    }
}
