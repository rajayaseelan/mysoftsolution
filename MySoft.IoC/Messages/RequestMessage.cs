using System;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// request base
    /// </summary>
    [Serializable]
    public abstract class MessageBase
    {
        private string serviceName;
        private string methodName;
        private Guid transactionId;
        private ParameterCollection parameters = new ParameterCollection();

        /// <summary>
        /// Gets or sets the transaction id.
        /// </summary>
        /// <value>The transaction id.</value>
        public Guid TransactionId
        {
            get
            {
                return transactionId;
            }
            set
            {
                transactionId = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public string ServiceName
        {
            get
            {
                return serviceName;
            }
            set
            {
                serviceName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        /// <value>The name of the sub service.</value>
        public string MethodName
        {
            get
            {
                return methodName;
            }
            set
            {
                methodName = value;
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        /// <summary>
        /// 响应的消息
        /// </summary>
        public abstract string Message { get; }
    }

    /// <summary>
    /// The request msg.
    /// </summary>
    [Serializable]
    public class RequestMessage : MessageBase
    {
        #region Private Members

        private string appName;
        private string hostName;
        private string requestAddress;
        private bool invokeMethod;

        #endregion

        /// <summary>
        /// Gets or sets the name of the appName.
        /// </summary>
        /// <value>The name of the appName.</value>
        public string AppName
        {
            get
            {
                return appName;
            }
            set
            {
                appName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the hostName.
        /// </summary>
        /// <value>The name of the hostName.</value>
        public string HostName
        {
            get
            {
                return hostName;
            }
            set
            {
                hostName = value;
            }
        }

        /// <summary>
        /// Gets or sets the request address
        /// </summary>
        public string IPAddress
        {
            get
            {
                return requestAddress;
            }
            set
            {
                requestAddress = value;
            }
        }

        /// <summary>
        /// invoke method
        /// </summary>
        public bool InvokeMethod
        {
            get
            {
                return invokeMethod;
            }
            set
            {
                invokeMethod = value;
            }
        }

        private int cacheTime = -1;
        /// <summary>
        /// 数据缓存时间（单位：秒）
        /// </summary>
        public int CacheTime
        {
            get
            {
                return cacheTime;
            }
            set
            {
                cacheTime = value;
            }
        }

        #region 额外的参数

        [NonSerialized]
        private System.Reflection.MethodInfo method;

        /// <summary>
        /// 响应的方法
        /// </summary>
        internal System.Reflection.MethodInfo MethodInfo
        {
            get
            {
                return method;
            }
            set
            {
                method = value;
            }
        }

        [NonSerialized]
        private ResponseType resptype;

        /// <summary>
        /// 传输的数据类型
        /// </summary>
        internal ResponseType RespType
        {
            get
            {
                return resptype;
            }
            set
            {
                resptype = value;
            }
        }

        #endregion

        /// <summary>
        /// 请求的消息
        /// </summary>
        public override string Message
        {
            get
            {

                return string.Format("{0}：{1}({2})", this.appName, this.hostName, this.requestAddress);
            }
        }
    }
}
