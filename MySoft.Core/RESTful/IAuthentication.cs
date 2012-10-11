using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.RESTful
{
    /// <summary>
    /// 认证接口
    /// </summary>
    public interface IAuthentication
    {
        /// <summary>
        /// 认证处理接口
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        AuthorizeUser Authorize(AuthorizeToken token);
    }
}
