﻿using System;
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
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetErrorMessage(Exception ex, string parameter)
        {
            ex = ErrorHelper.GetInnerException(ex);
            if (!string.IsNullOrEmpty(parameter))
                return string.Format("{0}, {1}, request params: {2}", ex.Message, ex.TargetSite, parameter);
            else
                return string.Format("{0}, {1}", ex.Message, ex.TargetSite);
        }
    }
}