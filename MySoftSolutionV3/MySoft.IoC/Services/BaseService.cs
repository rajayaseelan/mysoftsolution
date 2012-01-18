using System;
using MySoft.IoC.Messages;
using MySoft.Logger;

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
        private ILog logger;

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
        public BaseService(ILog logger, string serviceName)
        {
            this.logger = logger;
            this.serviceName = serviceName;
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
            //处理cacheKey信息
            string cacheKey = string.Format("ServerCache_{0}_{1}_{2}", reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters);

            //运行请求获得结果
            ResponseMessage resMsg = null;
            if (OperationContext.Current.Cache != null)
                resMsg = OperationContext.Current.Cache.GetCache<ResponseMessage>(cacheKey);
            else
                resMsg = CacheHelper.Get<ResponseMessage>(cacheKey);

            //如果未获取值
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
                        logger.WriteError(exception);
                    }
                }
                else if (reqMsg.CacheTime > 0) //判断是否需要缓存
                {
                    //加入缓存
                    if (OperationContext.Current.Cache != null)
                        OperationContext.Current.Cache.AddCache(cacheKey, resMsg, reqMsg.CacheTime);
                    else
                        CacheHelper.Insert(cacheKey, resMsg, reqMsg.CacheTime);
                }
            }
            else
            {
                resMsg.TransactionId = reqMsg.TransactionId;
                resMsg.Expiration = reqMsg.Expiration;
            }

            return resMsg;
        }

        #endregion
    }
}
