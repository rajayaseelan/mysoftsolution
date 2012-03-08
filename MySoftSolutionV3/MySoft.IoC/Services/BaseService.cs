using System;
using System.Collections.Generic;
using MySoft.IoC.Messages;
using MySoft.Cache;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The base class of services.
    /// </summary>
    [Serializable]
    public abstract class BaseService : IService
    {
        /// <summary>
        ///  The service logger
        /// </summary>
        private IServiceContainer container;
        private IServiceCache cache;
        private Type serviceType;
        private IDictionary<string, OperationContractAttribute> opContracts;

        /// <summary>
        /// The service name.
        /// </summary>
        protected string serviceName;

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public string ServiceName
        {
            get { return serviceName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public BaseService(IServiceContainer container, IServiceCache cache, Type serviceType)
        {
            this.container = container;
            this.cache = cache;
            this.serviceType = serviceType;
            this.serviceName = serviceType.FullName;

            this.opContracts = new Dictionary<string, OperationContractAttribute>();
            foreach (var method in CoreHelper.GetMethodsFromType(serviceType))
            {
                string methodKey = string.Format("{0}_{1}", serviceType.FullName, method.ToString());
                opContracts[methodKey] = CoreHelper.GetMemberAttribute<OperationContractAttribute>(method);
            }
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected abstract ResponseMessage Run(RequestMessage reqMsg);

        #region IService Members

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            OperationContractAttribute opContract = null;
            string methodKey = string.Format("{0}_{1}", reqMsg.ServiceName, reqMsg.MethodName);
            if (opContracts.ContainsKey(methodKey))
            {
                opContract = opContracts[methodKey];
            }

            //从缓存获取值
            string cacheKey = ServiceConfig.GetCacheKey(reqMsg, opContract);
            ResponseMessage resMsg = null;
            int serverCacheTime = -1;
            if (opContract != null)
            {
                if (opContract.ServerCacheTime > 0) serverCacheTime = opContract.ServerCacheTime;
            }

            if (serverCacheTime > 0)
            {
                //从缓存获取数据
                resMsg = cache.Get<ResponseMessage>(cacheKey);
            }

            //运行请求获得结果
            if (resMsg == null)
            {
                resMsg = Run(reqMsg);

                if (resMsg.IsError)
                {
                    //如果是业务异常，则不抛出错误
                    if (!resMsg.IsBusinessError)
                    {
                        var ex = resMsg.Error;
                        string body = string.Format("【{5}】Dynamic ({0}) service ({1},{2}) error. \r\nMessage ==> {4}\r\nParameters ==> {3}", reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters, resMsg.Message, resMsg.TransactionId);
                        var exception = new IoCException(body, ex)
                        {
                            ApplicationName = reqMsg.AppName,
                            ServiceName = reqMsg.ServiceName,
                            ErrorHeader = string.Format("Application【{0}】occurs error. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                        };
                        container.WriteError(exception);
                    }
                }
                else if (serverCacheTime > 0) //判断是否需要缓存
                {
                    //加入缓存
                    cache.Insert(cacheKey, resMsg, serverCacheTime);
                }
            }
            else
            {
                resMsg.TransactionId = reqMsg.TransactionId;
            }

            return resMsg;
        }

        #endregion
    }
}
