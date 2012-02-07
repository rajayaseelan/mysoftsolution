using System;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// The base impl class of the service interface, this class is used by service factory to emit service interface impl automatically at runtime.
    /// </summary>
    public class ServiceInvocationHandler : IProxyInvocationHandler
    {
        protected CastleFactoryConfiguration config;
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
        private object CallService(System.Reflection.MethodInfo methodInfo, object[] paramValues)
        {
            #region 设置请求信息

            RequestMessage reqMsg = new RequestMessage();
            reqMsg.AppName = config.AppName;                                //应用名称
            reqMsg.HostName = hostName;                                     //客户端名称
            reqMsg.IPAddress = ipAddress;                                   //客户端IP地址
            reqMsg.ServiceName = serviceType.FullName;                      //服务名称
            reqMsg.MethodName = methodInfo.ToString();                      //方法名称
            reqMsg.ReturnType = methodInfo.ReturnType;                      //返回类型
            reqMsg.TransactionId = Guid.NewGuid();                          //传输ID号
            reqMsg.CacheTime = -1;                                          //设置缓存时间

            #endregion

            #region 处理参数

            var pis = methodInfo.GetParameters();
            if (paramValues != null && pis.Length != paramValues.Length)
            {
                //参数不正确直接返回异常
                string title = string.Format("Invalid parameters ({0},{1}).", reqMsg.ServiceName, reqMsg.MethodName);
                string body = string.Format("{0}\r\nParameters ==> {1}", title, reqMsg.Parameters);
                throw new WarningException(body)
                {
                    ApplicationName = reqMsg.AppName,
                    ServiceName = reqMsg.ServiceName,
                    ErrorHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
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

                //处理参数
                if (config.Json) JsonInParameter(reqMsg);
            }

            #endregion

            //获取约束信息
            var opContract = CoreHelper.GetMemberAttribute<OperationContractAttribute>(methodInfo);
            int clientCacheTime = -1;
            if (opContract != null)
            {
                if (opContract.ServerCacheTime > 0) reqMsg.CacheTime = opContract.ServerCacheTime;
                if (opContract.ClientCacheTime > 0) clientCacheTime = opContract.ClientCacheTime;
            }

            try
            {
                string cacheKey = GetCacheKey(reqMsg, opContract);
                var resMsg = CacheHelper.Get<ResponseMessage>(cacheKey);

                //调用服务
                if (resMsg == null)
                {
                    //调用多次
                    var timesCount = config.Times;
                    if (timesCount < 1) timesCount = 1;
                    for (int times = 0; times < timesCount; times++)
                    {
                        resMsg = service.CallService(reqMsg);
                        if (resMsg != null) break;
                    }

                    //如果数据为null,则返回null
                    if (resMsg == null)
                    {
                        return CoreHelper.GetTypeDefaultValue(methodInfo.ReturnType);
                    }

                    //如果有异常，向外抛出
                    if (resMsg.IsError) throw resMsg.Error;

                    //处理参数
                    if (config.Json) JsonOutParameter(pis, resMsg);

                    //如果客户端缓存时间大于0
                    if (clientCacheTime > 0)
                    {
                        //没有异常，则缓存数据
                        CacheHelper.Insert(cacheKey, resMsg, clientCacheTime);
                    }
                }

                //给引用的参数赋值
                for (int i = 0; i < pis.Length; i++)
                {
                    if (pis[i].ParameterType.IsByRef)
                    {
                        //给参数赋值
                        paramValues[i] = resMsg.Parameters[pis[i].Name];
                    }
                }

                //返回数据
                return resMsg.Value;
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

            //返回默认值
            return CoreHelper.GetTypeDefaultValue(methodInfo.ReturnType);
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
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invoke(object proxy, System.Reflection.MethodInfo method, object[] args)
        {
            return CallService(method, args);
        }

        /// <summary>
        /// 获取缓存Key值
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="opContract"></param>
        /// <returns></returns>
        private string GetCacheKey(RequestMessage reqMsg, OperationContractAttribute opContract)
        {
            if (opContract != null && !string.IsNullOrEmpty(opContract.CacheKey))
            {
                string cacheKey = opContract.CacheKey;
                foreach (var key in reqMsg.Parameters.Keys)
                {
                    string name = "{" + key + "}";
                    if (cacheKey.Contains(name))
                    {
                        var parameter = reqMsg.Parameters[key];
                        if (parameter != null)
                            cacheKey = cacheKey.Replace(name, parameter.ToString());
                    }
                }

                return string.Format("{0}_{1}", reqMsg.ServiceName, cacheKey);
            }

            return string.Format("ClientCache_{0}_{1}_{2}", reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters);
        }

        #endregion
    }
}
