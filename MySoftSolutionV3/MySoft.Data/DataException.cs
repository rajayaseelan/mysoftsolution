using System;

namespace MySoft.Data
{
    /// <summary>
    /// Data异常
    /// </summary>
    [Serializable]
    public class DataException : MySoftException
    {
        /// <summary>
        /// 普通异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        public DataException(string message) : base(ExceptionType.DataException, message) { }

        /// <summary>
        /// 内嵌异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public DataException(string message, Exception ex) : base(ExceptionType.DataException, message, ex) { }
    }
}
