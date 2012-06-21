using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Web
{
    /// <summary>
    /// 静态页异常
    /// </summary>
    [Serializable]
    public class StaticPageException : WebException
    {
        /// <summary>
        /// 普通异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        public StaticPageException(string message) : base(message) { }

        /// <summary>
        /// 内嵌异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public StaticPageException(string message, Exception ex) : base(message, ex) { }
    }
}
