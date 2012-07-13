using System;

namespace MySoft.IoC
{
    /// <summary>
    /// IoC异常
    /// </summary>
    [Serializable]
    public class IoCException : MySoftException
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string ApplicationName
        {
            get
            {
                if (base.Data.Contains("ApplicationName"))
                    return base.Data["ApplicationName"].ToString();
                else
                    return null;
            }
            set { base.Data["ApplicationName"] = value; }
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get
            {
                if (base.Data.Contains("ServiceName"))
                    return base.Data["ServiceName"].ToString();
                else
                    return null;
            }
            set { base.Data["ServiceName"] = value; }
        }

        /// <summary>
        /// 错误头
        /// </summary>
        public string ErrorHeader
        {
            get
            {
                if (base.Data.Contains("ErrorHeader"))
                    return base.Data["ErrorHeader"].ToString();
                else
                    return null;
            }
            set { base.Data["ErrorHeader"] = value; }
        }

        /// <summary>
        /// 普通异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        public IoCException(string message)
            : base(ExceptionType.IoCException, message) { }

        /// <summary>
        /// 内嵌异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public IoCException(string message, Exception ex)
            : base(ExceptionType.IoCException, message, ex) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">存储对象序列化和反序列化所需的全部数据</param>
        /// <param name="context">描述给定的序列化流的源和目标，并提供一个由调用方定义的附加上下文</param>
        protected IoCException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
