using System;
using MySoft.IoC.Messages;

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
        private Type serviceType;

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public string ServiceName
        {
            get { return serviceType.FullName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public BaseService(IServiceContainer container, Type serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
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
        /// <param name="method"></param>
        /// <returns>The msg.</returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //运行服务返回值
            var resMsg = Run(reqMsg);

            //如果出错，通知客户端
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

            return resMsg;
        }

        #endregion
    }
}
