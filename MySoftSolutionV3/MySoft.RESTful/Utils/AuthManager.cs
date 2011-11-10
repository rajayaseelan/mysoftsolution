using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.RESTful.Configuration;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using MySoft.Logger;
using MySoft.RESTful.Auth;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// 认证工厂
    /// </summary>
    public static class AuthManager
    {
        /// <summary>
        /// RESTful配置文件
        /// </summary>
        private static IList<IAuthentication> auths = new List<IAuthentication>();

        static AuthManager()
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
            var incomingRequest = WebOperationContext.Current.IncomingRequest;

            //初始化AuthenticationContext
            AuthenticationToken authToken = new AuthenticationToken(incomingRequest.UriTemplateMatch.RequestUri, incomingRequest.UriTemplateMatch.QueryParameters, incomingRequest.Method);
            AuthenticationContext.Current = new AuthenticationContext(authToken)
            {
                //赋值TokenId
                TokenId = incomingRequest.UriTemplateMatch.QueryParameters["tokenId"]
            };

            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.Cookies != null)
                    AuthenticationContext.Current.Token.Cookies = HttpContext.Current.Request.Cookies;
            }
            else
            {
                string cookie = incomingRequest.Headers[HttpRequestHeader.Cookie];
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
            response.StatusCode = HttpStatusCode.Unauthorized;

            //进行认证处理
            var result = new RESTfulResult
            {
                Code = (int)RESTfulCode.AUTH_FAULT,
                Message = "Authentication fault!"
            };

            try
            {
                if (auths.Count == 0)
                {
                    result.Code = (int)RESTfulCode.AUTH_ERROR;
                    result.Message = "No any authentication!";
                    return result;
                }

                List<string> errors = new List<string>();
                bool isAuthentication = false;

                //如果配置了服务
                foreach (IAuthentication auth in auths)
                {
                    if (auth.Authorize())
                    {
                        //认证成功
                        result.Code = (int)RESTfulCode.OK;
                        result.Message = "Authentication success!";

                        isAuthentication = true;
                        break;
                    }
                    else
                    {
                        errors.Add(result.Message);
                    }
                }

                if (!isAuthentication)
                {
                    if (result.Code == 0) result.Code = (int)RESTfulCode.AUTH_ERROR;
                    result.Message = string.Join(" | ", errors.ToArray());
                }
            }
            catch (AuthenticationException ex)
            {
                result.Code = ex.Code;
                result.Message = RESTfulHelper.GetErrorMessage(ex, null);
            }
            catch (Exception ex)
            {
                result.Code = (int)RESTfulCode.AUTH_ERROR;
                result.Message = RESTfulHelper.GetErrorMessage(ex, null);
            }

            return result;
        }
    }
}
