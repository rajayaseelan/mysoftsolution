using System;

namespace MySoft.RESTful
{
    /// <summary>
    /// RESTful异常
    /// </summary>
    [Serializable]
    public class RESTfulException : Exception
    {
        /// <summary>
        /// 实例 化RESTfulException
        /// </summary>
        /// <param name="message"></param>
        public RESTfulException(string message)
            : base(message)
        { }
    }
}
