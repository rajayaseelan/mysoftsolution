using System.Net;

namespace MySoft.RESTful
{
    /// <summary>
    /// RESTful异常
    /// </summary>
    public class RESTfulException : MySoftException
    {
        private int code = (int)HttpStatusCode.OK;
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code
        {
            get { return code; }
            set { this.code = value; }
        }

        /// <summary>
        /// 实例 化RESTfulException
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public RESTfulException(int code, string message)
            : base(message)
        {
            this.code = code;
        }
    }
}