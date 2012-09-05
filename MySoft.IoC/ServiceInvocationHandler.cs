using System;
using System.Collections.Generic;
using MySoft.Cache;
using MySoft.IoC.Configuration;
using MySoft.IoC.Logger;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;

namespace MySoft.IoC
{
    /// <summary>
    /// The base impl class of the service interface, this class is used by service factory to emit service interface impl automatically at runtime.
    /// </summary>
    public class ServiceInvocationHandler : IProxyInvocationHandler
    {
        private CastleFactoryConfiguration config;
        private IDictionary<string, int> cacheTimes;
        private IServiceContainer container;
        private AsyncCaller asyncCaller;
        private IService service;
        private Type serviceType;
        private ICacheStrategy cache;
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
            this.cache = cache;
            this.logger = logger;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();

            this.cacheTimes = new Dictionary<string, int>();

            //实例化异步服务
            this.asyncCaller = new AsyncCaller(container, service, true, false);

            var methods = CoreHelper.GetMethodsFromType(serviceType);
            foreach (var method in methods)
            {
                var contract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
                if (contract != null)
                {
                    if (contract.CacheTime > 0)
                        cacheTimes[method.ToString()] = contract.CacheTime;
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
            object returnValue = null;
            var collection = IoCHelper.CreateParameters(method, parameters);
            string cacheKey = IoCHelper.GetCacheKey(serviceType, method, collection);
            var cacheValue = cache.GetCache<CacheObject>(cacheKey);

            //缓存无值
            if (cacheValue == null)
            {
                //调用方法
                var resMsg = InvokeMethod(method, collection);

                if (resMsg != null)
                {
                    returnValue = resMsg.Value;

                    //处理参数
                    IoCHelper.SetRefParameterValues(method, resMsg.Parameters, parameters);

                    if (resMsg.Count > 0) //数据条数不为0进行缓存
                    {
                        //如果需要缓存，则存入本地缓存
                        if (returnValue != null && cacheTimes.ContainsKey(method.ToString()))
                        {
                            int cacheTime = cacheTimes[method.ToString()];
                            cacheValue = new CacheObject
                            {
                                Value = resMsg.Value,
                                Parameters = resMsg.Parameters
                            };

                            cache.InsertCache(cacheKey, cacheValue, cacheTime);
                        }
                    }
                }
            }
            else
            {
                //处理返回值
                returnValue = CoreHelper.CloneObject(cacheValue.Value);

                //处理参数
                IoCHelper.SetRefParameterValues(method, cacheValue.Parameters, parameters);
            }

            //返回结果
            return returnValue;
        }

        /// <summary>
        /// 调用方法返回
        /// </summary>
        /// <param name="method"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        private ResponseMessage InvokeMethod(System.Reflection.MethodInfo method, ParameterCollection collection)
        {
            #region 设置请求信息

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

            #endregion

            //调用服务
            return CallService(reqMsg);
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

                //异步调用服务
                resMsg = asyncCaller.AsyncCall(context, reqMsg);

                //写日志结束
                logger.EndRequest(reqMsg, resMsg, resMsg.ElapsedTime);

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
                    throw ex;
                else
                    container.WriteError(ex);
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
