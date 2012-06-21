using System;

namespace MySoft
{
    /// <summary>
    /// 异常类型
    /// </summary>
    [Serializable]
    public enum ExceptionType
    {
        /// <summary>
        /// 未知异常
        /// </summary>
        Unknown,

        /// <summary>
        /// Data异常
        /// </summary>
        DataException,

        /// <summary>
        /// Web异常
        /// </summary>
        WebException,

        /// <summary>
        /// Remoting异常
        /// </summary>
        RemotingException,

        /// <summary>
        /// IoC异常
        /// </summary>
        IoCException,

        /// <summary>
        /// Task异常
        /// </summary>
        TaskException,
    }

    /// <summary>
    /// MySoft异常类
    /// </summary>
    [Serializable]
    public class MySoftException : Exception
    {
        private ExceptionType exceptionType;

        /// <summary>
        /// 异常类型
        /// </summary>
        public ExceptionType ExceptionType
        {
            get { return this.exceptionType; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="msg"></param>
        public MySoftException(string msg)
            : base(msg)
        {
            this.exceptionType = ExceptionType.Unknown;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="inner"></param>
        public MySoftException(string msg, Exception inner)
            : base(msg, inner)
        {
            this.exceptionType = ExceptionType.Unknown;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="t">异常类型</param>
        public MySoftException(ExceptionType t)
            : base("MySoft Component Error.")
        {
            this.exceptionType = t;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="t">异常类型</param>
        /// <param name="msg">异常消息</param>
        public MySoftException(ExceptionType t, string msg)
            : base(msg)
        {
            this.exceptionType = t;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="t">异常类型</param>
        /// <param name="msg">异常消息</param>
        /// <param name="inner">内部异常</param>
        public MySoftException(ExceptionType t, string msg, Exception inner)
            : base(msg, inner)
        {
            this.exceptionType = t;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">存储对象序列化和反序列化所需的全部数据</param>
        /// <param name="context">描述给定的序列化流的源和目标，并提供一个由调用方定义的附加上下文</param>
        protected MySoftException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            this.exceptionType = (ExceptionType)info.GetValue("ExceptionType", typeof(ExceptionType));
        }

        /// <summary>
        /// 重载GetObjectData方法
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ExceptionType", this.exceptionType);
        }
    }
}
