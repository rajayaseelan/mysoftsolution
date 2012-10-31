using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace MySoft.Web
{
    /// <summary>
    /// 通用方法处理类
    /// </summary>
    public class WebHelper
    {
        #region 注册文件到页面

        /// <summary>
        /// 注册Ajax所需js到页面
        /// </summary>
        /// <param name="type"></param>
        public static void RegisterPageForAjax(Page page, string url)
        {
            RegisterPageForAjax(page, page.GetType(), url);
        }

        /// <summary>
        /// 注册Ajax所需js到页面
        /// </summary>
        /// <param name="type"></param>
        public static void RegisterPageForAjax(Page page, Type urlType, string url)
        {
            #region 生成当前path的资源文件

            Type type = page.GetType();
            string ajaxKey = "AjaxProcess";
            if (url.IndexOf("?") >= 0) url = url.Remove(url.IndexOf("?"));
            if (page.Request.QueryString.Count > 0) url += page.Request.Url.Query;

            string query = CoreHelper.Encrypt(url, ajaxKey);
            if (urlType != null)
            {
                UI.AjaxNamespaceAttribute ajaxSpace = CoreHelper.GetMemberAttribute<UI.AjaxNamespaceAttribute>(urlType);
                if (ajaxSpace != null) query += ";" + CoreHelper.Encrypt(ajaxSpace.Name ?? urlType.Name, ajaxKey);
            }

            string urlResource = page.Request.ApplicationPath + (page.Request.ApplicationPath.EndsWith("/") ? "" : "/") + "Ajax/" + type.FullName + ".ashx?" + query;

            #endregion

            RegisterForAjax(page, urlResource);
        }

        /// <summary>
        /// 注册Ajax所需js到页面
        /// </summary>
        /// <param name="type"></param>
        private static void RegisterForAjax(Page page, string urlResource)
        {
            List<string> jslist = new List<string>();

            jslist.Add(page.ClientScript.GetWebResourceUrl(typeof(UI.AjaxPage), "MySoft.Web.Resources.request.js"));
            jslist.Add(page.ClientScript.GetWebResourceUrl(typeof(UI.AjaxPage), "MySoft.Web.Resources.ajax.js"));
            jslist.Add(urlResource);

            //将js注册到页面
            RegisterPageJsFile(page, jslist.ToArray());
        }

        /// <summary>
        /// 向页面中加载js文件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsurls"></param>
        public static void RegisterPageJsFile(Page page, params string[] jsfiles)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string jsfile in jsfiles)
            {
                sb.Append("<script src=\"" + jsfile + "\" type=\"text/javascript\"></script>\r\n");
            }

            string cskey = "key" + Guid.NewGuid().ToString();
            ClientScriptManager cs = page.ClientScript;
            Type type = page.GetType();
            if (!cs.IsClientScriptBlockRegistered(type, cskey))
            {
                cs.RegisterClientScriptBlock(type, cskey, sb.ToString(), false);
            }
        }

        /// <summary>
        /// 添加Javascript脚本到页面
        /// </summary>
        /// <param name="scriptString"></param>
        public static void RegisterPageScript(Page page, params string[] scripts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string script in scripts)
            {
                sb.Append(script);
            }

            string cskey = "key" + Guid.NewGuid().ToString();
            ClientScriptManager cs = page.ClientScript;
            Type type = page.GetType();
            if (!cs.IsClientScriptBlockRegistered(type, cskey))
            {
                cs.RegisterClientScriptBlock(type, cskey, sb.ToString(), true);
            }
        }


        /// <summary>
        /// 向页面中加载css文件
        /// </summary>
        /// <param name="page"></param>
        /// <param name="cssurls"></param>
        public static void RegisterPageCssFile(Page page, params string[] cssfiles)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cssfile in cssfiles)
            {
                sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + cssfile + "\" />\r\n");
            }

            string cskey = "key" + Guid.NewGuid().ToString();
            ClientScriptManager cs = page.ClientScript;
            Type type = page.GetType();
            if (!cs.IsClientScriptBlockRegistered(type, cskey))
            {
                cs.RegisterClientScriptBlock(type, cskey, sb.ToString(), false);
            }
        }

        #endregion

        #region 检测和保存Cookie

        public static void SaveCookie(string cookieName, string value)
        {
            SaveCookie(cookieName, value, null);
        }

        public static void SaveCookie(string cookieName, string value, CookieExpiresType expiresType, int cookieTime)
        {
            SaveCookie(cookieName, value, null, expiresType, cookieTime);
        }

        public static void SaveCookie(string cookieName, string[] paramNames, string[] paramValues)
        {
            SaveCookie(cookieName, paramNames, paramValues, null);
        }

        public static void SaveCookie(string cookieName, string value, string cookieDomain)
        {
            SaveCookie(cookieName, new string[] { cookieName }, new string[] { value }, cookieDomain, CookieExpiresType.None, 0);
        }

        public static void SaveCookie(string cookieName, string value, string cookieDomain, CookieExpiresType expiresType, int cookieTime)
        {
            SaveCookie(cookieName, new string[] { cookieName }, new string[] { value }, cookieDomain, expiresType, cookieTime);
        }

        public static void SaveCookie(string cookieName, string[] paramNames, string[] paramValues, string cookieDomain)
        {
            SaveCookie(cookieName, paramNames, paramValues, cookieDomain, CookieExpiresType.None, 0);
        }

        /// <summary>
        /// 保存一个Cookie,0为永不过期
        /// </summary>
        /// <param name="cookieName">Cookie名称</param>
        /// <param name="CookieValue">Cookie值</param>
        /// <param name="CookieTime">Cookie过期时间(天数),0为关闭页面失效</param>
        public static void SaveCookie(string cookieName, string[] paramNames, string[] paramValues, string cookieDomain, CookieExpiresType expiresType, int cookieTime)
        {
            HttpCookie myCookie = new HttpCookie(cookieName);
            DateTime now = DateTime.Now;

            if (paramNames.Length == 1 && paramNames[0] == cookieName)
            {
                myCookie.Value = paramValues[0];
            }
            else
            {
                for (int index = 0; index < paramNames.Length; index++)
                {
                    myCookie.Values[paramNames[index]] = paramValues[index];
                }
            }

            if (cookieDomain != string.Empty && cookieDomain != null)
            {
                myCookie.Domain = cookieDomain;
            }

            //设置过期时间
            switch (expiresType)
            {
                case CookieExpiresType.None:
                    //不处理过期
                    break;
                case CookieExpiresType.Year:
                    myCookie.Expires = now.AddYears(cookieTime);
                    break;
                case CookieExpiresType.Month:
                    myCookie.Expires = now.AddMonths(cookieTime);
                    break;
                case CookieExpiresType.Day:
                    myCookie.Expires = now.AddDays(cookieTime);
                    break;
                case CookieExpiresType.Hour:
                    myCookie.Expires = now.AddHours(cookieTime);
                    break;
                case CookieExpiresType.Minute:
                    myCookie.Expires = now.AddMinutes(cookieTime);
                    break;
                case CookieExpiresType.Second:
                    myCookie.Expires = now.AddSeconds(cookieTime);
                    break;
            }

            if (HttpContext.Current.Response.Cookies[cookieName] != null)
            {
                HttpContext.Current.Response.Cookies.Remove(cookieName);
            }

            HttpContext.Current.Response.AppendCookie(myCookie);
        }

        /// <summary>
        /// 检测cookid中是否存在值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ExistCookie(string cookieName)
        {
            HttpCookie myCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (myCookie == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 取得CookieValue
        /// </summary>
        /// <param name="cookieName">Cookie名称</param>
        /// <returns>Cookie的值</returns>
        public static HttpCookie GetCookie(string cookieName)
        {
            HttpCookie myCookie = new HttpCookie(cookieName);
            myCookie = HttpContext.Current.Request.Cookies[cookieName];

            if (myCookie != null)
                return myCookie;
            else
                return null;
        }

        /// <summary>
        /// 清除CookieValue
        /// </summary>
        /// <param name="cookieName">Cookie名称</param>
        public static void ClearCookie(string cookieName)
        {
            HttpCookie myCookie = new HttpCookie(cookieName);
            DateTime now = DateTime.Now;

            myCookie.Expires = now.AddYears(-1);

            HttpContext.Current.Response.SetCookie(myCookie);
        }

        #endregion

        #region 检测和保存Session
        /// <summary>
        /// 保存Session值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SaveSession(string key, object value)
        {
            HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// 从页面中移除Session值
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        public static void RemoveSession(string key)
        {
            try
            {
                HttpContext.Current.Session.Remove(key);
            }
            catch { }
        }

        /// <summary>
        /// 检测Session中是否存在值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ExistSession(string key)
        {
            try
            {
                if (HttpContext.Current.Session[key] == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从Session中获取对象
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TObject GetSession<TObject>(string key)
        {
            try
            {
                return (TObject)HttpContext.Current.Session[key];
            }
            catch
            {
                return default(TObject);
            }
        }
        #endregion

        #region 对字符串的加密/解密

        /// <summary>
        /// 对字符串进行适应 ServU 的 MD5 加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ServUEncrypt(string str)
        {
            string strResult = "";
            strResult = RandomSTR(2);
            str = strResult + str;
            str = NoneEncrypt(str, 1);
            str = strResult + str;

            return str;
        }

        /// <summary>
        /// 获取一个由26个小写字母组成的指定长度的随即字符串
        /// </summary>
        /// <param name="intLong">指定长度</param>
        /// <returns></returns>
        private static string RandomSTR(int intLong)
        {
            string strResult = "";
            string[] array = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            Random r = new Random();

            for (int i = 0; i < intLong; i++)
            {
                strResult += array[r.Next(26)];
            }

            return strResult;
        }

        /// <summary>
        /// 对字符串进行普通md5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string str)
        {
            return NoneEncrypt(str, 1);
        }

        /// <summary>
        /// 对字符串进行加密（不可逆）
        /// </summary>
        /// <param name="Password">要加密的字符串</param>
        /// <param name="Format">加密方式,0 is SHA1,1 is MD5</param>
        /// <returns></returns>
        public static string NoneEncrypt(string Password, int Format)
        {
            string strResult = "";
            switch (Format)
            {
                case 0:
                    strResult = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "SHA1");
                    break;
                case 1:
                    strResult = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "MD5");
                    break;
                default:
                    strResult = Password;
                    break;
            }

            return strResult;
        }


        /// <summary>
        /// 对字符串进行加密
        /// </summary>
        /// <param name="value">待加密的字符串</param>
        /// <returns>string</returns>
        public static string Encrypt(string value)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(value, true, 2);
            return FormsAuthentication.Encrypt(ticket).ToString();
        }

        /// <summary>
        /// 对字符串进行解密
        /// </summary>
        /// <param name="value">已加密的字符串</param>
        /// <returns></returns>
        public static string Decrypt(string value)
        {
            return FormsAuthentication.Decrypt(value).Name.ToString();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the string param.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="paramName">Name of the param.</param>
        /// <returns>The param value.</returns>
        public static TReturn GetRequestParam<TReturn>(System.Web.HttpRequest request, string paramName)
        {
            return GetRequestParam<TReturn>(request, paramName, default(TReturn));
        }

        /// <summary>
        /// Gets the string param.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="errorReturn">The error return.</param>
        /// <returns>The param value.</returns>
        public static TReturn GetRequestParam<TReturn>(System.Web.HttpRequest request, string paramName, TReturn errorReturn)
        {
            string retStr = request.Headers[paramName];
            if (retStr == null)
            {
                retStr = request.Form[paramName];
            }
            if (retStr == null)
            {
                retStr = request.QueryString[paramName];
            }
            if (retStr == null)
            {
                return errorReturn;
            }

            return CoreHelper.ConvertTo<TReturn>(retStr, errorReturn);
        }

        /// <summary>
        /// Strongs the typed.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The strong typed instance.</returns>
        public static TObject StrongTyped<TObject>(object obj)
        {
            return (TObject)obj;
        }

        /// <summary>
        /// Toes the js single quote safe string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns>The formated str.</returns>
        public static string ToJsSingleQuoteSafeString(string str)
        {
            return str.Replace("'", "\\'");
        }

        /// <summary>
        /// Toes the js double quote safe string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns>The formated str.</returns>
        public static string ToJsDoubleQuoteSafeString(string str)
        {
            return str.Replace("\"", "\\\"");
        }

        /// <summary>
        /// Toes the VBS quote safe string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns>The formated str.</returns>
        public static string ToVbsQuoteSafeString(string str)
        {
            return str.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Toes the SQL quote safe string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns>The formated str.</returns>
        public static string ToSqlQuoteSafeString(string str)
        {
            return str.Replace("'", "''");
        }

        /// <summary>
        /// Texts to HTML.
        /// </summary>
        /// <param name="txtStr">The TXT STR.</param>
        /// <returns>The formated str.</returns>
        public static string TextToHtml(string txtStr)
        {
            return txtStr.Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").
                Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r", "").Replace("\n", "<br />");
        }

        /// <summary>
        /// 处理内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ReplaceContext(string content, string extension)
        {
            string p0 = "(<a\\s*href\\s*=\\s*[\'|\"]+)(?:\\s*(?<url>(?!=(?<http>http.\\/\\/)|(?<www>www\\.))([\\w+-\\/]*).aspx)(.*?[\'|\"]))";
            string p1 = "(<option\\s*value\\s*=\\s*[\'|\"]+)(?:\\s*(?<url>(?!=(?<http>http.\\/\\/)|(?<www>www\\.))([\\w+-\\/]*).aspx)(.*?[\'|\"]))";
            string p2 = "(action\\s*=\\s*[\'|\"]+)(?:\\s*(?<url>(?!=(?<http>http.\\/\\/)|(?<www>www\\.))([\\w+-\\/]*)" + extension + ")(.*?[\'|\"]))";
            string p = "$1$2{0}$3";

            string[,] pattern = {
                                        { p0, string.Format(p, extension) },
                                        { p1, string.Format(p, extension) },
                                        { p2, string.Format(p, ".aspx") }
                                    };

            //将生成的内容进行替换
            for (int i = 0; i < pattern.GetLength(0); i++)
            {
                Regex reg = new Regex(pattern[i, 0], RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (reg.IsMatch(content))
                {
                    content = reg.Replace(content, pattern[i, 1]);
                }
            }

            return content;
        }

        #endregion

        #region Resource

        private static Dictionary<string, Hashtable> stringResources = new Dictionary<string, Hashtable>();

        private static System.Globalization.CultureInfo defaultCulture = null;

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        /// <value>The default culture.</value>
        public static System.Globalization.CultureInfo DefaultCulture
        {
            get
            {
                return defaultCulture ?? System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                defaultCulture = value;
            }
        }

        /// <summary>
        /// Loads the resources.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="ci">The ci.</param>
        public static void LoadResources(string resourceName, System.Globalization.CultureInfo ci)
        {
            string resFileName = System.Web.HttpRuntime.BinDirectory + resourceName + "." + ci.ToString() + ".resources";
            if (System.IO.File.Exists(resFileName))
            {
                lock (stringResources)
                {
                    if (!stringResources.ContainsKey(ci.ToString()))
                    {
                        stringResources.Add(ci.ToString(), new Hashtable());

                        try
                        {
                            ResourceReader reader = new ResourceReader(resFileName);
                            IDictionaryEnumerator en = reader.GetEnumerator();
                            while (en.MoveNext())
                            {
                                stringResources[ci.ToString()].Add(en.Key, en.Value);
                            }
                            reader.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the resources.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        public static void LoadResources(string resourceName)
        {
            LoadResources(resourceName, DefaultCulture);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The resouce value.</returns>
        public static string GetResourceString(string key)
        {
            return GetResourceString(key, WebHelper.DefaultCulture);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="ci">The ci.</param>
        /// <returns>The resouce value.</returns>
        public static string GetResourceString(string key, System.Globalization.CultureInfo ci)
        {
            if (stringResources.ContainsKey(ci.ToString()))
            {
                if (stringResources[ci.ToString()].Contains(key))
                {
                    return stringResources[ci.ToString()][key].ToString();
                }
            }

            return string.Empty;
        }

        #endregion

        #region ClientScriptFactoryHelper

        /// <summary>
        /// Common Client Script
        /// </summary>
        public class ClientScriptFactoryHelper
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ClientScriptFactoryHelper"/> class.
            /// </summary>
            public ClientScriptFactoryHelper()
            {
            }

            #endregion

            /// <summary>
            /// Wraps the script tag.
            /// </summary>
            /// <param name="scripts">The scripts.</param>
            /// <returns>The script.</returns>
            public string WrapScriptTag(params string[] scripts)
            {
                if (scripts != null && scripts.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("\r\n<script language=\"javascript\" type=\"text/javascript\">\r\n<!--\r\n");

                    foreach (string script in scripts)
                    {
                        sb.Append(script.EndsWith(";") || script.EndsWith("}") ? script : script + ";");
                    }

                    sb.Append("\r\n-->\r\n</script>\r\n");
                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// Pops the alert.
            /// </summary>
            /// <param name="msg">The MSG.</param>
            /// <returns>The script.</returns>
            public string PopAlert(string msg)
            {
                return string.Format(" window.alert('{0}'); ", ToJsSingleQuoteSafeString(msg));
            }

            /// <summary>
            /// Pops the confirm.
            /// </summary>
            /// <param name="msg">The MSG.</param>
            /// <returns>The script.</returns>
            public string PopConfirm(string msg)
            {
                return string.Format(" window.confirm('{0}') ", ToJsSingleQuoteSafeString(msg));
            }

            /// <summary>
            /// Pops the prompt.
            /// </summary>
            /// <param name="msg">The MSG.</param>
            /// <param name="defaultValue">The default value.</param>
            /// <returns>The script.</returns>
            public string PopPrompt(string msg, string defaultValue)
            {
                return string.Format(" window.prompt('{0}', '{1}') ", ToJsSingleQuoteSafeString(msg), ToJsSingleQuoteSafeString(defaultValue));
            }

            /// <summary>
            /// Closes the self.
            /// </summary>
            /// <returns>The script.</returns>
            public string CloseSelf()
            {
                return " window.close(); ";
            }

            /// <summary>
            /// Closes the parent.
            /// </summary>
            /// <returns>The script.</returns>
            public string CloseParent()
            {
                return " if (window.parent) { window.parent.close(); } ";
            }

            /// <summary>
            /// Closes the opener.
            /// </summary>
            /// <returns>The script.</returns>
            public string CloseOpener()
            {
                return " if (window.opener) { window.opener.close(); } ";
            }

            /// <summary>
            /// Refreshes the self.
            /// </summary>
            /// <returns>The script.</returns>
            public string RefreshSelf()
            {
                return " window.location += ' '; ";
            }

            /// <summary>
            /// Refreshes the opener.
            /// </summary>
            /// <returns>The script.</returns>
            public string RefreshOpener()
            {
                return " if (window.opener) { window.opener.location += ' '; } ";
            }

            /// <summary>
            /// Refreshes the parent.
            /// </summary>
            /// <returns>The script.</returns>
            public string RefreshParent()
            {
                return " if (window.parent) { window.parent.location += ' '; } ";
            }

            /// <summary>
            /// Shows the modal dialog.
            /// </summary>
            /// <param name="url">The URL.</param>
            /// <param name="status">if set to <c>true</c> [status].</param>
            /// <param name="resizable">if set to <c>true</c> [resizable].</param>
            /// <param name="height">The height.</param>
            /// <param name="width">The width.</param>
            /// <param name="top">The top.</param>
            /// <param name="left">The left.</param>
            /// <param name="scroll">if set to <c>true</c> [scroll].</param>
            /// <returns>The script.</returns>
            public string ShowModalDialog(string url, bool status, bool resizable, int height, int width, int top, int left, bool scroll)
            {
                return string.Format(" window.showModalDialog('{0}', window, 'status={1},resizable={2},dialogHeight={3}px,dialogWidth={4}px,dialogTop={5},dialogLeft={6},scroll={7},unadorne=yes'); ",
                    ToJsSingleQuoteSafeString(url), (status ? 1 : 0), (resizable ? 1 : 0), height, width, top, left, (scroll ? 1 : 0));
            }

            /// <summary>
            /// Shows the modal dialog.
            /// </summary>
            /// <param name="url">The URL.</param>
            /// <param name="status">if set to <c>true</c> [status].</param>
            /// <param name="resizable">if set to <c>true</c> [resizable].</param>
            /// <param name="height">The height.</param>
            /// <param name="width">The width.</param>
            /// <param name="center">if set to <c>true</c> [center].</param>
            /// <param name="scroll">if set to <c>true</c> [scroll].</param>
            /// <returns>The script.</returns>
            public string ShowModalDialog(string url, bool status, bool resizable, int height, int width, bool center, bool scroll)
            {
                return string.Format(" window.showModalDialog('{0}', window, 'status={1},resizable={2},dialogHeight={3}px,dialogWidth={4}px,center={5},scroll={6},unadorne=yes'); ",
                    ToJsSingleQuoteSafeString(url), (status ? 1 : 0), (resizable ? 1 : 0), height, width, (center ? 1 : 0), (scroll ? 1 : 0));
            }

            /// <summary>
            /// Shows the modeless dialog.
            /// </summary>
            /// <param name="url">The URL.</param>
            /// <param name="status">if set to <c>true</c> [status].</param>
            /// <param name="resizable">if set to <c>true</c> [resizable].</param>
            /// <param name="height">The height.</param>
            /// <param name="width">The width.</param>
            /// <param name="top">The top.</param>
            /// <param name="left">The left.</param>
            /// <param name="scroll">if set to <c>true</c> [scroll].</param>
            /// <returns>The script.</returns>
            public string ShowModelessDialog(string url, bool status, bool resizable, int height, int width, int top, int left, bool scroll)
            {
                return string.Format(" window.showModelessDialog('{0}', window, 'status={1},resizable={2},dialogHeight={3}px,dialogWidth={4}px,dialogTop={5},dialogLeft={6},scroll={7},unadorne=yes'); ",
                    ToJsSingleQuoteSafeString(url), (status ? 1 : 0), (resizable ? 1 : 0), height, width, top, left, (scroll ? 1 : 0));
            }

            /// <summary>
            /// Shows the modeless dialog.
            /// </summary>
            /// <param name="url">The URL.</param>
            /// <param name="status">if set to <c>true</c> [status].</param>
            /// <param name="resizable">if set to <c>true</c> [resizable].</param>
            /// <param name="height">The height.</param>
            /// <param name="width">The width.</param>
            /// <param name="center">if set to <c>true</c> [center].</param>
            /// <param name="scroll">if set to <c>true</c> [scroll].</param>
            /// <returns>The script.</returns>
            public string ShowModelessDialog(string url, bool status, bool resizable, int height, int width, bool center, bool scroll)
            {
                return string.Format(" window.showModelessDialog('{0}', window, 'status={1},resizable={2},dialogHeight={3}px,dialogWidth={4}px,center={5},scroll={6},unadorne=yes'); ",
                    ToJsSingleQuoteSafeString(url), (status ? 1 : 0), (resizable ? 1 : 0), height, width, (center ? 1 : 0), (scroll ? 1 : 0));
            }

            /// <summary>
            /// Selfs the go back.
            /// </summary>
            /// <returns>The script.</returns>
            public string SelfGoBack()
            {
                return " window.history.back(); ";
            }

            /// <summary>
            /// Parents the go back.
            /// </summary>
            /// <returns>The script.</returns>
            public string ParentGoBack()
            {
                return " if (window.parent) { window.parent.history.back(); } ";
            }

            /// <summary>
            /// Openers the go back.
            /// </summary>
            /// <returns>The script.</returns>
            public string OpenerGoBack()
            {
                return " if (window.opener) { window.opener.history.back(); } ";
            }

            /// <summary>
            /// Opens the specified URL.
            /// </summary>
            /// <param name="url">The URL.</param>
            /// <param name="frameName">Name of the frame.</param>
            /// <param name="status">if set to <c>true</c> [status].</param>
            /// <param name="location">if set to <c>true</c> [location].</param>
            /// <param name="menubar">if set to <c>true</c> [menubar].</param>
            /// <param name="resizable">if set to <c>true</c> [resizable].</param>
            /// <param name="height">The height.</param>
            /// <param name="width">The width.</param>
            /// <param name="top">The top.</param>
            /// <param name="left">The left.</param>
            /// <param name="scrollbars">if set to <c>true</c> [scrollbars].</param>
            /// <param name="toolbar">if set to <c>true</c> [toolbar].</param>
            /// <returns>The script.</returns>
            public string Open(string url, string frameName, bool status, bool location, bool menubar,
                bool resizable, int height, int width, int top, int left, bool scrollbars, bool toolbar)
            {
                return string.Format(" window.open('{0}', '{1}', 'status={2},location={3},menubar={4},resizable={5},height={6}px,width={7}px,top={8},left={9},scrollbars={10},toolbar={11}'); ",
                    ToJsSingleQuoteSafeString(url), ToJsSingleQuoteSafeString(frameName), (status ? 1 : 0), (location ? 1 : 0), (menubar ? 1 : 0), (resizable ? 1 : 0), height, width, top, left, (scrollbars ? 1 : 0), (toolbar ? 1 : 0));
            }

            /// <summary>
            /// Opens the specified URL.
            /// </summary>
            /// <param name="url">The URL.</param>
            /// <param name="frameName">Name of the frame.</param>
            /// <returns>The script.</returns>
            public string Open(string url, string frameName)
            {
                return string.Format(" window.open('{0}', '{1}'); ", ToJsSingleQuoteSafeString(url), ToJsSingleQuoteSafeString(frameName));
            }

            /// <summary>
            /// Calls the client validator.
            /// </summary>
            /// <param name="prefix">The prefix.</param>
            /// <param name="validators">The validators.</param>
            /// <returns>The script.</returns>
            public string CallClientValidator(params System.Web.UI.WebControls.BaseValidator[] validators)
            {
                if (validators != null && validators.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (System.Web.UI.WebControls.BaseValidator validator in validators)
                    {
                        sb.Append(string.Format(" ValidatorValidate({0}); ", validator.ID));
                    }
                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// Toes the js string array.
            /// </summary>
            /// <param name="strs">The STRS.</param>
            /// <returns>The script.</returns>
            public string ToJsStringArray(params string[] strs)
            {
                if (strs != null && strs.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(" new Array(");

                    foreach (string str in strs)
                    {
                        sb.Append(string.Format("'{0}', ", str.Replace("'", "\\'")));
                    }

                    return sb.ToString().TrimEnd(',', ' ') + ");";
                }
                else
                {
                    return " new Array;";
                }
            }
        }

        #endregion
    }
}
