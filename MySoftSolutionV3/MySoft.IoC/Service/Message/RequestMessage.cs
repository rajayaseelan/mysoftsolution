using System;
using System.Collections.Generic;
using System.Text;
using MySoft.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Message
{
    /// <summary>
    /// request base
    /// </summary>
    [Serializable]
    public abstract class MessageBase
    {
        private string serviceName;
        private string subServiceName;
        private Guid transactionId;
        private ParameterCollection parameters = new ParameterCollection();
        private DateTime expiration;
        private Type returnType;
        private bool compress = false;
        private bool encrypt = false;

        /// <summary>
        /// Gets or sets the returnType.
        /// </summary>
        /// <value>The returnType.</value>
        public Type ReturnType
        {
            get
            {
                return returnType;
            }
            set
            {
                returnType = value;
            }
        }

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
        /// Gets or sets the name of the sub service.
        /// </summary>
        /// <value>The name of the sub service.</value>
        public string SubServiceName
        {
            get
            {
                return subServiceName;
            }
            set
            {
                subServiceName = value;
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
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>The expiration.</value>
        public DateTime Expiration
        {
            get
            {
                return expiration;
            }
            set
            {
                expiration = value;
            }
        }

        /// <summary>
        /// Gets or sets the compress of the service.
        /// </summary>
        public bool Compress
        {
            get
            {
                return compress;
            }
            set
            {
                compress = value;
            }
        }

        /// <summary>
        /// Gets or sets the encrypt of the service.
        /// </summary>
        public bool Encrypt
        {
            get
            {
                return encrypt;
            }
            set
            {
                encrypt = value;
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
        private double timeout = -1;

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
        /// Gets or sets the timeout of the service.
        /// </summary>
        public double Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
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
