using System;
using System.Diagnostics;
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
        ///  The service type
        /// </summary>
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
        public BaseService(Type serviceType)
        {
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
        /// <returns>The msg.</returns>
        public ResponseMessage CallService(RequestMessage reqMsg)
        {
            //开始计时
            var watch = Stopwatch.StartNew();

            try
            {
                var resMsg = Run(reqMsg);

                watch.Stop();

                //计算耗时时间
                resMsg.ElapsedTime = watch.ElapsedMilliseconds;

                return resMsg;
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }
        }

        #endregion
    }
}
