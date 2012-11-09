using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using MySoft.Web.Configuration;

namespace MySoft.Web
{
    public class RedirectPageHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        // 摘要:
        //     获取一个值，该值指示其他请求是否可以使用 System.Web.IHttpHandler 实例。

        //
        // 返回结果:
        //     如果 System.Web.IHttpHandler 实例可再次使用，则为 true；否则为 false。

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        // 摘要:
        //     通过实现 System.Web.IHttpHandler 接口的自定义 HttpHandler 启用 HTTP Web 请求的处理。

        //
        // 参数:
        //   context:
        //     System.Web.HttpContext 对象，它提供对用于为 HTTP 请求提供服务的内部服务器对象（如 Request、Response、Session
        //     和 Server）的引用。
        public void ProcessRequest(HttpContext context)
        {
            string url = context.Request.RawUrl;

            // get the configuration rules
            RedirectPageRuleCollection rules = RedirectPageConfiguration.GetConfig().Rules;

            // iterate through each rule...
            for (int i = 0; i < rules.Count; i++)
            {
                // get the pattern to look for, and Resolve the Url (convert ~ into the appropriate directory)
                string lookFor = "^" + RewriterUtils.ResolveUrl(context.Request.ApplicationPath, rules[i].LookFor) + "$";

                // Create a regex (note that IgnoreCase is set...)
                Regex re = new Regex(lookFor, RegexOptions.IgnoreCase);

                if (re.IsMatch(url))
                {
                    string redirectUrl = RewriterUtils.ResolveUrl(context.Request.ApplicationPath, re.Replace(url, rules[i].WriteTo));

                    // match found - do any replacement needed
                    try
                    {
                        RedirectUrl(context, rules[i], redirectUrl);

                        return;
                    }
                    catch
                    {
                        //如果跳转出错，则直接转入错误页
                        if (!string.IsNullOrEmpty(rules[i].ErrorTo))
                        {
                            string errorUrl = RewriterUtils.ResolveUrl(context.Request.ApplicationPath, re.Replace(url, rules[i].ErrorTo));
                            context.Server.Transfer(errorUrl);

                            return;
                        }
                    }
                }
            }

            string sendToUrl = context.Request.Url.PathAndQuery;
            string filePath = context.Request.PhysicalApplicationPath;
            string sendToUrlLessQString;
            RewriterUtils.RewriteUrl(context, sendToUrl, out sendToUrlLessQString, out filePath);
            IHttpHandler handler = PageParser.GetCompiledPageInstance(sendToUrlLessQString, filePath, context);

            handler.ProcessRequest(context);
        }

        /// <summary>
        /// 转入新的URL地址
        /// </summary>
        /// <param name="context"></param>
        /// <param name="rule"></param>
        /// <param name="redirectUrl"></param>
        private void RedirectUrl(HttpContext context, RedirectPageRule rule, string redirectUrl)
        {
            // iterate through each rule...
            if (rule.GenHtml)
            {
                string rawUrl = context.Request.RawUrl;
                string htmlFile = context.Server.MapPath(rawUrl);
                string extension = Path.GetExtension(htmlFile);

                //判断扩展名是否为null
                if (string.IsNullOrEmpty(extension))
                    htmlFile += rule.Extension;
                else
                    htmlFile = htmlFile.Replace(extension, rule.Extension);

                //需要生成静态页面
                if (!File.Exists(htmlFile))
                {
                    //静态页面不存在
                    if (StaticPageManager.CreateStaticPage(redirectUrl, htmlFile, rule.ValidateString))
                    {
                        context.Response.WriteFile(htmlFile);
                        return;
                    }
                }
                else
                {
                    //静态页面存在
                    FileInfo file = new FileInfo(htmlFile);

                    //按秒检测页面重新生成
                    int span = (int)DateTime.Now.Subtract(file.LastWriteTime).TotalSeconds;

                    if (rule.Timeout > 0 && span >= rule.Timeout)
                    {
                        lock (hashtable.SyncRoot)
                        {
                            if (!hashtable.ContainsKey(redirectUrl))
                            {
                                var item = new WorkerItem
                                {
                                    RedirectUrl = redirectUrl,
                                    HtmlFile = htmlFile,
                                    ValidateString = rule.ValidateString
                                };

                                hashtable[redirectUrl] = item;

                                //更新页面
                                UpdatePageItem(redirectUrl);
                            }
                        }
                    }

                    context.Response.WriteFile(htmlFile);
                    return;
                }
            }

            //转到新地址处理
            context.Server.Transfer(redirectUrl);
        }

        /// <summary>
        /// 更新页面
        /// </summary>
        /// <param name="redirectUrl"></param>
        private void UpdatePageItem(string redirectUrl)
        {
            //过期重新生成
            ThreadPool.QueueUserWorkItem(state =>
            {
                if (state == null) return;

                var pageKey = Convert.ToString(state);

                try
                {
                    var worker = hashtable[pageKey] as WorkerItem;
                    StaticPageManager.CreateStaticPage(worker.RedirectUrl, worker.HtmlFile, worker.ValidateString);
                }
                finally
                {
                    hashtable.Remove(pageKey);
                }
            }, redirectUrl);
        }
    }

    internal class WorkerItem
    {
        /// <summary>
        /// 请求的Url
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// 生成的静态文件
        /// </summary>
        public string HtmlFile { get; set; }

        /// <summary>
        /// 验证的字符串
        /// </summary>
        public string ValidateString { get; set; }
    }
}
