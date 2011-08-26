using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace MySoft
{
    /// <summary>
    /// 错误处理
    /// </summary>
    public class ErrorHelper
    {
        /// <summary>
        /// 获取最内部的异常
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception GetInnerException(Exception ex)
        {
            if (ex == null) return ex;
            if (ex.InnerException != null)
                return GetInnerException(ex.InnerException);
            else
                return ex;
        }

        /// <summary>
        /// 获取异常日志
        /// </summary>
        /// <param name="ex"></param>
        public static string GetErrorWithoutHtml(Exception ex)
        {
            try
            {
                StringBuilder sbLog = new StringBuilder();
                sbLog.Append("\r\nType:" + ex.GetType().FullName);
                sbLog.Append("\r\nMessage:" + ex.Message);
                sbLog.Append("\r\nSource:" + ex.Source);
                sbLog.Append("\r\nTargetSite:" + (ex.TargetSite == null ? null : ex.TargetSite.ToString()));
                sbLog.Append("\r\nStackTrace:" + ex.StackTrace);
                sbLog.Append("\r\n");

                if (ex.InnerException != null)
                {
                    sbLog.AppendLine("------------------------------------------------------------------------------------------------------------------------");
                    sbLog.Append(GetErrorWithoutHtml(ex.InnerException));
                }

                return sbLog.ToString();
            }
            catch (Exception)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Returns HTML an formatted error message.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetHtmlError(Exception ex)
        {
            // Heading Template
            const string heading = "<TABLE BORDER=\"0\" WIDTH=\"100%\" CELLPADDING=\"1\" CELLSPACING=\"0\"><TR><TD bgcolor=\"black\" COLSPAN=\"2\"><FONT face=\"Arial\" color=\"white\"><B>&nbsp;<!--HEADER--></B></FONT></TD></TR></TABLE>";

            // Error Message Header
            string html = "<FONT face=\"Arial\" size=\"5\" color=\"red\">Error - " + ReplaceNewline(ex.Message) + "</FONT><BR><BR>";

            // Populate Error Information Collection
            NameValueCollection error_info = new NameValueCollection();
            error_info.Add("Type", ex.GetType().FullName);
            error_info.Add("Message", ReplaceNewline(ex.Message));
            error_info.Add("Source", ReplaceNewline(ex.Source));
            error_info.Add("TargetSite", ReplaceNewline(ex.TargetSite == null ? null : ex.TargetSite.ToString()));
            error_info.Add("StackTrace", ReplaceNewline(ex.StackTrace));

            // Error Information
            html += heading.Replace("<!--HEADER-->", "Error Information");
            html += CollectionToHtmlTable(error_info);

            if (HttpContext.Current != null)
            {
                // Query Information
                NameValueCollection info = new NameValueCollection();
                info.Add("Url", String.Format("<a target='_blank' href='{0}'>{0}</a>", HttpContext.Current.Request.Url.ToString()));
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Query Information");
                html += CollectionToHtmlTable(info);

                // QueryString Collection
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "QueryString Collection");
                html += CollectionToHtmlTable(HttpContext.Current.Request.QueryString);

                // Form Collection
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Form Collection");
                html += CollectionToHtmlTable(HttpContext.Current.Request.Form);

                // Cookies Collection
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Cookies Collection");
                html += CollectionToHtmlTable(HttpContext.Current.Request.Cookies);

                // Session Variables
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Session Variables");
                html += CollectionToHtmlTable(HttpContext.Current.Session);

                // Server Variables
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Server Variables");
                html += CollectionToHtmlTable(HttpContext.Current.Request.ServerVariables);
            }

            if (ex.InnerException != null) html += GetHtmlError(ex.InnerException);

            return html;
        }

        /// <summary>
        /// Get Log Html
        /// </summary>
        /// <returns>Result</returns>
        public static string GetHtmlWithoutError()
        {
            // Heading Template
            const string heading = "<TABLE BORDER=\"0\" WIDTH=\"100%\" CELLPADDING=\"1\" CELLSPACING=\"0\"><TR><TD bgcolor=\"black\" COLSPAN=\"2\"><FONT face=\"Arial\" color=\"white\"><B>&nbsp;<!--HEADER--></B></FONT></TD></TR></TABLE>";

            string html = "";

            if (HttpContext.Current != null)
            {
                // Query Information
                NameValueCollection info = new NameValueCollection();
                info.Add("Url", String.Format("<a target='_blank' href='{0}'>{0}</a>", HttpContext.Current.Request.Url.ToString()));
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Query Information");
                html += CollectionToHtmlTable(info);

                // QueryString Collection
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "QueryString Collection");
                html += CollectionToHtmlTable(HttpContext.Current.Request.QueryString);

                // Form Collection
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Form Collection");
                html += CollectionToHtmlTable(HttpContext.Current.Request.Form);

                // Cookies Collection
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Cookies Collection");
                html += CollectionToHtmlTable(HttpContext.Current.Request.Cookies);

                // Session Variables
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Session Variables");
                html += CollectionToHtmlTable(HttpContext.Current.Session);

                // Server Variables
                html += "<BR><BR>" + heading.Replace("<!--HEADER-->", "Server Variables");
                html += CollectionToHtmlTable(HttpContext.Current.Request.ServerVariables);

                //html = HttpContext.Current.Request.ServerVariables["Remote_Addr"];
            }

            return html;
        }

        private static string CollectionToHtmlTable(NameValueCollection collection)
        {
            // <TD>...</TD> Template
            const string TD = "<TD><FONT face=\"Arial\" size=\"2\"><!--VALUE--></FONT></TD>";

            // Table Header
            string html = "\n<TABLE width=\"100%\">\n"
                + "  <TR bgcolor=\"#C0C0C0\">" + TD.Replace("<!--VALUE-->", "&nbsp;<B>Name</B>")
                + "  " + TD.Replace("<!--VALUE-->", "&nbsp;<B>Value</B>") + "</TR>\n";

            // No Body? -> N/A
            if (collection != null)
            {
                if (collection.Count == 0)
                {
                    collection = new NameValueCollection();
                    collection.Add("N/A", "");
                }

                // Table Body
                for (int i = 0; i < collection.Count; i++)
                {
                    html += "<TR valign=\"top\" bgcolor=\"" + ((i % 2 == 0) ? "white" : "#EEEEEE") + "\">"
                        + TD.Replace("<!--VALUE-->", collection.Keys[i]) + "\n"
                        + TD.Replace("<!--VALUE-->", collection[i]) + "</TR>\n";
                }
            }
            else
            {
                collection = new NameValueCollection();
                collection.Add("N/A", "");
            }

            // Table Footer
            return html + "</TABLE>";
        }

        private static string CollectionToString(NameValueCollection collection)
        {
            string text = "";

            if (collection != null)
            {
                if (collection.Count == 0)
                {
                    collection = new NameValueCollection();
                    collection.Add("N/A", "");
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    text += collection.Keys[i];
                    text += collection[i] + "\r\n";
                }
            }
            else
            {
                collection = new NameValueCollection();
                collection.Add("N/A", "");
            }

            return text;
        }

        private static string CollectionToHtmlTable(HttpCookieCollection collection)
        {
            // Overload for HttpCookieCollection collection.
            // Converts HttpCookieCollection to NameValueCollection
            NameValueCollection NVC = new NameValueCollection();
            if (collection != null)
            {
                foreach (string item in collection)
                {
                    if (collection[item] != null)
                    {
                        NVC.Add(item, collection[item].Value);
                    }
                }
            }
            return CollectionToHtmlTable(NVC);
        }

        private static string CollectionToHtmlTable(System.Web.SessionState.HttpSessionState collection)
        {
            // Overload for HttpSessionState collection.
            // Converts HttpSessionState to NameValueCollection
            NameValueCollection NVC = new NameValueCollection();
            if (collection != null)
            {
                foreach (string item in collection)
                {
                    if (collection[item] != null)
                    {
                        NVC.Add(item, collection[item].ToString());
                    }
                }
            }
            return CollectionToHtmlTable(NVC);
        }

        private static string ReplaceNewline(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            // Cleans the string for HTML friendly display
            return html.Replace("\r\n", "<br/ >").Replace("\n", "<br/ >");
        }
    }
}
