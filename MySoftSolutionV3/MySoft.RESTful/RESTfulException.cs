
namespace MySoft.RESTful
{
    /// <summary>
    /// RESTful异常
    /// </summary>
    public class RESTfulException : MySoftException
    {
        private RESTfulCode code = RESTfulCode.BUSINESS_ERROR;
        /// <summary>
        /// 状态码
        /// </summary>
        public RESTfulCode Code
        {
            get { return code; }
            set { this.code = value; }
        }

        /// <summary>
        /// 实例 化RESTfulException
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public RESTfulException(RESTfulCode code, string message)
            : base(message)
        {
            this.code = code;
        }

        /// <summary>
        /// 实例 化RESTfulException
        /// </summary>
        /// <param name="message"></param>
        public RESTfulException(string message)
            : base(message)
        { }
    }
}
