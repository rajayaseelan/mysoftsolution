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
            ResponseMessage resMsg = null;

            //开始计时
            var watch = Stopwatch.StartNew();

            try
            {
                resMsg = Run(reqMsg);

                if (resMsg == null) return null;
            }
            finally
            {
                watch.Stop();
            }

            //计算超时
            resMsg.ElapsedTime = watch.ElapsedMilliseconds;

            //返回结果数据
            if (reqMsg.InvokeMethod)
            {
                resMsg.Value = new InvokeData
                {
                    Value = SerializationManager.SerializeJson(resMsg.Value),
                    Count = resMsg.Count,
                    ElapsedTime = resMsg.ElapsedTime,
                    OutParameters = resMsg.Parameters.ToString()
                };

                //清除参数集合
                resMsg.Parameters.Clear();
            }

            return resMsg;
        }

        #endregion
    }
}
