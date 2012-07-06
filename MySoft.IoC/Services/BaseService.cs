using System;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Diagnostics;

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
        private IServiceContainer logger;
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
        public BaseService(IServiceContainer logger, Type serviceType)
        {
            this.logger = logger;
            this.serviceType = serviceType;
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected abstract ResponseMessage Run(RequestMessage reqMsg);

        /// <summary>
        /// Dispose
        /// </summary>
        public abstract void Dispose();

        #region IService Members

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //开始计时
            var watch = Stopwatch.StartNew();

            var resMsg = Run(reqMsg);

            //停止计时
            watch.Stop();

            //设置耗时
            resMsg.ElapsedMilliseconds = watch.ElapsedMilliseconds;

            //如果出错，通知客户端
            if (resMsg.IsError)
            {
                string body = string.Format("Remote client【{0}】call service ({1},{2}) error.\r\n\r\nParameters => {3}\r\nMessage => {4}",
                    reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), resMsg.Message);

                //获取异常
                var exception = IoCHelper.GetException(OperationContext.Current, reqMsg, body, resMsg.Error);

                logger.WriteError(exception);
            }

            return resMsg;
        }

        #endregion
    }
}
