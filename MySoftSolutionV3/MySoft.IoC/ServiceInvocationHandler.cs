using System;
using System.Collections.Generic;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Cache;

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
        private IService service;
        private Type serviceType;
        private IServiceCache cache;
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
        public ServiceInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, IService service, Type serviceType, IServiceCache cache)
        {
            this.config = config;
            this.container = container;
            this.serviceType = serviceType;
            this.service = service;
            this.cache = cache;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();

            this.cacheTimes = new Dictionary<string, int>();
            var methods = CoreHelper.GetMethodsFromType(serviceType);
            foreach (var method in methods)
            {
                var contract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
                if (contract != null && contract.CacheTime > 0)
                    cacheTimes[method.ToString()] = contract.CacheTime;
            }
        }

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">Name of the sub service.</param>
        /// <param name="method">The param values.</param>
        /// <returns>The result.</returns>
        private ResponseMessage CallService(RequestMessage reqMsg, System.Reflection.MethodInfo method)
        {
            ResponseMessage resMsg = null;

            try
            {
                var pis = method.GetParameters();

                //处理参数
                if (pis.Length > 0)
                {
                    if (config.DataType == DataType.Json)
                        JsonInParameter(reqMsg);
                }

                //调用服务
                resMsg = service.CallService(reqMsg);

                //如果数据为null,则返回null
                if (resMsg == null)
                {
                    var errMsg = string.Format("Request to return to service ({0}, {1}) the data is empty!", reqMsg.ServiceName, reqMsg.MethodName);
                    throw new WarningException(errMsg);
                }

                //如果有异常，向外抛出
                if (resMsg.IsError) throw resMsg.Error;

                //处理参数
                if (pis.Length > 0)
                {
                    if (config.DataType == DataType.Json)
                        JsonOutParameter(pis, resMsg);
                }
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
        /// Json输入处理
        /// </summary>
        /// <param name="reqMsg"></param>
        protected virtual void JsonInParameter(RequestMessage reqMsg)
        {
            //Json输入参数
        }

        /// <summary>
        /// Json输出处理
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="resMsg"></param>
        protected virtual void JsonOutParameter(System.Reflection.ParameterInfo[] parameters, ResponseMessage resMsg)
        {
            //Json输出参数
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

            #region 设置请求信息

            RequestMessage reqMsg = new RequestMessage();
            reqMsg.AppName = config.AppName;                                //应用名称
            reqMsg.HostName = hostName;                                     //客户端名称
            reqMsg.IPAddress = ipAddress;                                   //客户端IP地址
            reqMsg.ServiceName = serviceType.FullName;                      //服务名称
            reqMsg.MethodName = method.ToString();                      //方法名称
            reqMsg.ReturnType = method.ReturnType;                      //返回类型
            reqMsg.TransactionId = Guid.NewGuid();                          //传输ID号

            #endregion

            reqMsg.Parameters = ServiceConfig.CreateParameters(method, parameters);
            string cacheKey = ServiceConfig.GetCacheKey(serviceType, method, reqMsg.Parameters);
            var cacheValue = cache.Get<CacheObject>(cacheKey);

            //缓存无值
            if (cacheValue == null)
            {
                var resMsg = CallService(reqMsg, method);
                if (resMsg != null)
                {
                    returnValue = resMsg.Value;

                    //处理参数
                    ServiceConfig.SetParameterValue(method, parameters, resMsg.Parameters);

                    //如果需要缓存，则存入本地缓存
                    if (returnValue != null && cacheTimes.ContainsKey(method.ToString()))
                    {
                        int cacheTime = cacheTimes[method.ToString()];
                        cacheValue = new CacheObject
                        {
                            Value = resMsg.Value,
                            Parameters = resMsg.Parameters
                        };

                        cache.Insert(cacheKey, cacheValue, cacheTime);
                    }
                }
            }
            else
            {
                //处理返回值
                returnValue = cacheValue.Value;

                //处理参数
                ServiceConfig.SetParameterValue(method, parameters, cacheValue.Parameters);
            }

            //返回结果
            return returnValue;
        }

        #endregion
    }
}
