using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful异常
    /// </summary>
    [Serializable]
    public class RESTfulException : ApplicationException
    {
        private int code = 200;
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        /// <summary>
        /// 实例化RESTfulException
        /// </summary>
        /// <param name="message"></param>
        public RESTfulException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 实例化RESTfulException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RESTfulException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 实例化RESTfulException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public RESTfulException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            info.AddValue("Code", this.code);
        }

        /// <summary>
        /// 实例化RESTfulException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.code = (int)info.GetValue("Code", typeof(int));
            base.GetObjectData(info, context);
        }
    }
}
