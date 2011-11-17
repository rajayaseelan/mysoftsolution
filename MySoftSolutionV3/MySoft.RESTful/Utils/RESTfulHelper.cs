using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.RESTful.Auth;

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
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetErrorMessage(Exception ex, string parameter)
        {
            ex = ErrorHelper.GetInnerException(ex);
            var errors = new Dictionary<string, string>();
            errors["error"] = ex.Message;
            if (!string.IsNullOrEmpty(parameter)) errors["params"] = parameter;
            if (!string.IsNullOrEmpty(ex.Source)) errors["source"] = ex.Source;
            if (ex.TargetSite != null) errors["target"] = ex.TargetSite.ToString();
            if (AuthenticationContext.Current != null && AuthenticationContext.Current.User != null)
                errors["user"] = AuthenticationContext.Current.User.Name;

            StringBuilder sbMsg = new StringBuilder();
            foreach (var kv in errors)
            {
                sbMsg.AppendFormat("【{0}】: {1}", kv.Key, kv.Value);
                sbMsg.AppendLine();
            }

            return sbMsg.ToString();
        }
    }
}
