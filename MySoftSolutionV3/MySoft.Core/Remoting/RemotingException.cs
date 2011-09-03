using System;

namespace MySoft.Remoting
{
    /// <summary>
    /// Remoting异常
    /// </summary>
    [Serializable]
    public class RemotingException : MySoftException
    {
        /// <summary>
        /// 普通异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        public RemotingException(string message) : base(ExceptionType.RemotingException, message) { }

        /// <summary>
        /// 内嵌异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public RemotingException(string message, Exception ex) : base(ExceptionType.RemotingException, message, ex) { }
    }
}
