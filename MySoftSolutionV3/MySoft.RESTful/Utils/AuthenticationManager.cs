using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.RESTful.Configuration;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using MySoft.Logger;

namespace MySoft.RESTful
{
    /// <summary>
    /// 认证工厂
    /// </summary>
    public static class AuthenticationManager
    {
        /// <summary>
        /// RESTful配置文件
        /// </summary>
        private static IList<IAuthentication> auths = new List<IAuthentication>();

        static AuthenticationManager()
        {
            //读取配置文件
            var config = RESTfulConfiguration.GetConfig();
            if (config != null && config.Auths != null)
            {
                foreach (Authentication auth in config.Auths)
                {
                    try
                    {
                        var type = Type.GetType(auth.Type);
                        if (type == null) continue;
                        var obj = Activator.CreateInstance(type);
                        if (obj is IAuthentication)
                        {
                            auths.Add((IAuthentication)obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        SimpleLog.Instance.WriteLog(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化上下文
        /// </summary>
        private static void InitializeContext()
        {
            var request = WebOperationContext.Current.IncomingRequest;

            //初始化AuthenticationContext
            AuthenticationToken authToken = new AuthenticationToken(request.UriTemplateMatch.RequestUri, request.UriTemplateMatch.QueryParameters, request.Method);
            AuthenticationContext.Current = new AuthenticationContext(authToken);

            if (HttpContext.Current != null)
            {
                AuthenticationContext.Current.Token.Cookies = HttpContext.Current.Request.Cookies;
            }
            else
            {
                string cookie = request.Headers[HttpRequestHeader.Cookie];
                SetCookie(cookie);
            }
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="cookie"></param>
        private static void SetCookie(string cookie)
        {
            if (!string.IsNullOrEmpty(cookie))
            {
                HttpCookieCollection collection = new HttpCookieCollection();
                string[] cookies = cookie.Split(';');
                HttpCookie cook = null;
                foreach (string e in cookies)
                {
                    if (!string.IsNullOrEmpty(e))
                    {
                        string[] values = e.Split(new char[] { '=' }, 2);
                        if (values.Length == 2)
                        {
                            cook = new HttpCookie(values[0], values[1]);
                        }
                        collection.Add(cook);
                    }
                }

                AuthenticationContext.Current.Token.Cookies = collection;
            }
        }

        /// <summary>
        /// 进行认证
        /// </summary>
        /// <returns></returns>
        public static RESTfulResult Authorize()
        {
            //初始化上下文
            InitializeContext();

            var response = WebOperationContext.Current.OutgoingResponse;

            //进行认证处理
            var result = new RESTfulResult
            {
                Code = RESTfulCode.AUTH_FAULT.ToString(),
                Message = "Authentication fault!"
            };
            response.StatusCode = HttpStatusCode.Unauthorized;

            try
            {
                if (auths.Count == 0)
                {
                    result.Code = RESTfulCode.AUTH_ERROR.ToString();
                    result.Message = "No any authentication!";
                    return result;
                }

                //如果配置了服务
                foreach (IAuthentication auth in auths)
                {
                    if (auth.Authorize())
                    {
                        //认证成功
                        result.Code = RESTfulCode.OK.ToString();
                        result.Message = "Authentication success!";
                        break;
                    }
                }
            }
            catch (AuthenticationException ex)
            {
                result.Code = ex.Code;
                result.Message = ex.Message;
                response.StatusCode = ex.StatusCode;
            }
            catch (Exception ex)
            {
                result.Code = RESTfulCode.AUTH_ERROR.ToString();
                result.Message = ErrorHelper.GetInnerException(ex).Message;
            }

            return result;
        }
    }
}
