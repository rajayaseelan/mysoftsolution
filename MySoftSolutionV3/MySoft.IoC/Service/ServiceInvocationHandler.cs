using System;
using System.Reflection;
using MySoft.IoC.Configuration;
using MySoft.IoC.Message;

namespace MySoft.IoC
{
    /// <summary>
    /// The base impl class of the service interface, this class is used by service factory to emit service interface impl automatically at runtime.
    /// </summary>
    public class ServiceInvocationHandler : IProxyInvocationHandler
    {
        private CastleFactoryConfiguration config;
        private IServiceContainer container;
        private IService service;
        private Type serviceType;
        private string hostName;
        private string ipAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInvocationHandler"/> class.
        /// </summary>
        /// <param name="container">config.</param>
        /// <param name="container">The container.</param>
        /// <param name="serviceInterfaceType">Type of the service interface.</param>
        public ServiceInvocationHandler(CastleFactoryConfiguration config, IServiceContainer container, IService service, Type serviceType)
        {
            this.config = config;
            this.container = container;
            this.serviceType = serviceType;
            this.service = service;

            this.hostName = DnsHelper.GetHostName();
            this.ipAddress = DnsHelper.GetIPAddress();
        }

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="methodInfo">Name of the sub service.</param>
        /// <param name="paramValues">The param values.</param>
        /// <returns>The result.</returns>
        private object CallService(MethodInfo methodInfo, object[] paramValues)
        {
            #region 设置请求信息

            RequestMessage reqMsg = new RequestMessage();
            reqMsg.AppName = config.AppName;                                //应用名称
            reqMsg.HostName = hostName;                                     //客户端名称
            reqMsg.IPAddress = ipAddress;                                   //客户端IP地址
            reqMsg.ServiceName = serviceType.FullName;                      //服务名称
            reqMsg.SubServiceName = methodInfo.ToString();                  //方法名称
            reqMsg.ReturnType = methodInfo.ReturnType;                      //返回类型
            reqMsg.TransactionId = Guid.NewGuid();                          //传输ID号
            //reqMsg.Encrypt = config.Encrypt;                                                              //传递传输与压缩格式
            //reqMsg.Compress = config.Compress;                                                       //设置压缩格式
            reqMsg.Timeout = config.Timeout;                                //设置超时时间
            reqMsg.Expiration = DateTime.Now.AddSeconds(config.Timeout);    //设置过期时间

            #endregion

            #region 处理参数

            ParameterInfo[] pis = methodInfo.GetParameters();
            if ((pis.Length == 0 && paramValues != null && paramValues.Length > 0) || (paramValues != null && pis.Length != paramValues.Length))
            {
                //参数不正确直接返回异常
                string title = string.Format("Invalid parameters ({0},{1}).", reqMsg.ServiceName, reqMsg.SubServiceName);
                string body = string.Format("{0}\r\nParameters ==> {1}", title, reqMsg.Parameters.SerializedData);
                throw new WarningException(body)
                {
                    ApplicationName = reqMsg.AppName,
                    ExceptionHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                };
            }

            if (pis.Length > 0)
            {
                for (int i = 0; i < paramValues.Length; i++)
                {
                    if (paramValues[i] != null)
                    {
                        if (!pis[i].ParameterType.IsByRef)
                        {
                            //如果传递的是引用，则跳过
                            reqMsg.Parameters[pis[i].Name] = paramValues[i];
                        }
                        else if (!pis[i].IsOut)
                        {
                            //如果传递的是引用，则跳过
                            reqMsg.Parameters[pis[i].Name] = paramValues[i];
                        }
                    }
                    else
                    {
                        //传递参数值为null
                        reqMsg.Parameters[pis[i].Name] = null;
                    }
                }
            }

            #endregion

            #region 处理缓存

            //处理cacheKey信息
            string cacheKey = string.Format("{0}_{1}_{2}", reqMsg.ServiceName, reqMsg.SubServiceName, reqMsg.Parameters);

            bool isAllowCache = false;
            double cacheTime = config.CacheTime; //默认缓存时间与系统设置的时间一致

            #region 读取约束信息

            //获取约束信息
            var serviceContract = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(serviceType);

            //获取约束信息
            var operationContract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(methodInfo);

            //判断约束
            if (serviceContract != null)
            {
                isAllowCache = serviceContract.AllowCache;
                if (serviceContract.CacheTime > 0) cacheTime = serviceContract.CacheTime;
                if (serviceContract.Timeout > 0) reqMsg.Timeout = serviceContract.Timeout;
            }

            //判断约束
            if (operationContract != null)
            {
                if (isAllowCache) isAllowCache = operationContract.AllowCache;
                if (operationContract.CacheTime > 0) cacheTime = operationContract.CacheTime;
                if (operationContract.Timeout > 0) reqMsg.Timeout = operationContract.Timeout;

                //压缩处理
                reqMsg.Compress = config.Compress && operationContract.Compress;
                reqMsg.Encrypt = config.Encrypt && operationContract.Encrypt;
            }
            else
            {
                //默认方法不进行缓存
                isAllowCache = false;
            }

            serviceContract = null;
            operationContract = null;

            //设置过期时间
            reqMsg.Expiration = DateTime.Now.AddSeconds(reqMsg.Timeout);

            #endregion

            //定义返回的值
            object returnValue = null;
            ParameterCollection parameters = null;

            //缓存对象
            ServiceCache cacheValue = null;

            //缓存的处理
            if (isAllowCache && container.Cache != null)
            {
                //从缓存获取数据
                cacheValue = container.Cache.GetCache(serviceType, cacheKey) as ServiceCache;
            }

            //如果缓存不为null;
            if (cacheValue != null)
            {
                parameters = cacheValue.Parameters;
                returnValue = cacheValue.CacheObject;
            }
            else
            {
                ResponseMessage resMsg = null;
                try
                {
                    //调用服务
                    resMsg = service.CallService(reqMsg, config.LogTime);

                    //如果有异常，向外抛出
                    if (resMsg != null && resMsg.Exception != null)
                    {
                        throw resMsg.Exception;
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

                //参数
                parameters = resMsg.Parameters;


                //如果数据为null,则返回null
                if (resMsg == null || resMsg.Data == null || resMsg.Data.Value == null)
                {
                    //给引用的参数赋值
                    for (int i = 0; i < pis.Length; i++)
                    {
                        if (pis[i].ParameterType.IsByRef)
                        {
                            //给参数赋值
                            paramValues[i] = parameters[pis[i].Name];
                        }
                    }

                    return CoreHelper.GetTypeDefaultValue(methodInfo.ReturnType);
                }

                //从返回结果中取值
                returnValue = resMsg.Data.Value;

                if (returnValue != null)
                {
                    //缓存的处理
                    if (isAllowCache && container.Cache != null)
                    {
                        cacheValue = new ServiceCache { CacheObject = returnValue, Parameters = resMsg.Parameters };

                        //把值添加到缓存中
                        container.Cache.AddCache(serviceType, cacheKey, cacheValue, cacheTime);
                    }
                }
            }

            #endregion

            //给引用的参数赋值
            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i].ParameterType.IsByRef)
                {
                    //给参数赋值
                    paramValues[i] = parameters[pis[i].Name];
                }
            }

            //返回数据
            return returnValue;
        }

        #region IInvocationHandler 成员

        /// <summary>
        /// 响应委托
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invoke(object proxy, MethodInfo method, object[] args)
        {
            return CallService(method, args);
        }

        #endregion
    }
}
