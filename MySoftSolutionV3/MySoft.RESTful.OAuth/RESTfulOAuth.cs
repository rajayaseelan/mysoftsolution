using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.RESTful.Auth;

namespace MySoft.RESTful.OAuth
{
    /// <summary>
    /// 实现RESTful的OAuth认证
    /// </summary>
    public class RESTfulOAuth : IAuthentication
    {
        #region IAuthentication 成员

        /// <summary>
        /// 实现OAuth认证
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool Authorize()
        {
            AuthenticationToken token = AuthenticationContext.Current.Token;
            if (!string.IsNullOrEmpty(token.Parameters["username"]))
            {
                AuthenticationContext.Current.User = new AuthenticationUser
                {
                    AuthName = token.Parameters["username"]
                };
            }
            return true;
        }

        #endregion
    }
}
