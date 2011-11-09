using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// RESTful 帮助 
    /// </summary>
    public class RESTfulHelper
    {
        /// <summary>
        /// 获取异常详细信息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetErrorMessage(Exception ex)
        {
            ex = ErrorHelper.GetInnerException(ex);
            return string.Format("Error:{0}\r\nSource:{1}\r\nTargetSite:{2}", ex.Message, ex.Source, ex.TargetSite);
        }
    }
}
