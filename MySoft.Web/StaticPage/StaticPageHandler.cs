using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using MySoft.Web.Configuration;

namespace MySoft.Web
{
    /// <summary>
    /// 静态页生成Handler
    /// </summary>
    public class StaticPageHandler : IHttpHandler, IRequiresSessionState
    {
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
            string sendToUrl = context.Request.Url.PathAndQuery;
            string applicationPath = context.Request.PhysicalApplicationPath;

            //检测是否为Ajax调用
            bool AjaxProcess = WebHelper.GetRequestParam<bool>(context.Request, "X-Ajax-Process", false);
            if (!AjaxProcess && context.Request.RequestType == "GET")
            {
                var config = StaticPageConfiguration.GetConfig();

                //判断config配置信息
                if (config != null && config.Enabled)
                {
                    bool isMatch = false;

                    // iterate through the rules
                    foreach (StaticPageRule rule in config.Rules)
                    {
                        // get the pattern to look for, and Resolve the Url (convert ~ into the appropriate directory)
                        string lookFor = "^" + RewriterUtils.ResolveUrl(context.Request.ApplicationPath, rule.LookFor) + "$";

                        // Create a regex (note that IgnoreCase is set...)
                        Regex reg = new Regex(lookFor, RegexOptions.IgnoreCase);

                        if (reg.IsMatch(sendToUrl))
                        {
                            isMatch = true;

                            // match found - do any replacement needed
                            string staticUrl = RewriterUtils.ResolveUrl(context.Request.ApplicationPath, reg.Replace(sendToUrl, rule.WriteTo));
                            string staticFile = context.Server.MapPath(staticUrl);

                            //将域名进行替换
                            staticFile = staticFile.Replace("{domain}", context.Request.Url.Authority);

                            //需要生成静态页面
                            if (!File.Exists(staticFile))  //静态页面不存在
                            {
                                var filter = new ResponseFilter(context.Response.Filter, staticFile, rule.ValidateString, config.Replace, config.Extension);
                                context.Response.Filter = filter;
                                break;
                            }
                            else
                            {
                                //静态页面存在
                                FileInfo file = new FileInfo(staticFile);

                                //按秒检测页面重新生成
                                int span = (int)DateTime.Now.Subtract(file.LastWriteTime).TotalSeconds;
                                if (rule.Timeout > 0 && span >= rule.Timeout) //静态页面过期
                                {
                                    var filter = new ResponseFilter(context.Response.Filter, staticFile, rule.ValidateString, config.Replace, config.Extension);
                                    context.Response.Filter = filter;
                                    break;
                                }
                                //检测是否需要更新
                                else if (!string.IsNullOrEmpty(rule.UpdateFor) && CheckUpdate(context, rule.UpdateFor))
                                {
                                    var filter = new ResponseFilter(context.Response.Filter, staticFile, rule.ValidateString, config.Replace, config.Extension);
                                    context.Response.Filter = filter;
                                    break;
                                }
                                else
                                {
                                    //设置格式
                                    context.Response.Buffer = true;
                                    context.Response.Clear();
                                    context.Response.ClearHeaders();

                                    SetContentType(context, file.Extension);

                                    //将文件写入流中
                                    context.Response.WriteFile(staticFile);
                                    context.Response.End();
                                }
                            }
                        }
                    }

                    //如果没有匹配上
                    if (!isMatch)
                    {
                        //处理updates
                        if (config.Replace && !string.IsNullOrEmpty(config.Extension))
                        {
                            var filter = new AspNetFilter(context.Response.Filter, config.Replace, config.Extension);
                            context.Response.Filter = filter;
                        }
                    }
                }
            }

            //进行错误处理，如果出错，则转到原有的静态页面
            try
            {
                //if (sendToUrl.IndexOf('?') > 0) sendToUrl = sendToUrl.Substring(0, sendToUrl.IndexOf('?'));
                //IHttpHandler handler = BuildManager.CreateInstanceFromVirtualPath(sendToUrl, typeof(Page)) as IHttpHandler;

                string sendToUrlLessQString;
                RewriterUtils.RewriteUrl(context, sendToUrl, out sendToUrlLessQString, out applicationPath);
                IHttpHandler handler = PageParser.GetCompiledPageInstance(sendToUrlLessQString, applicationPath, context);
                handler.ProcessRequest(context);
            }
            catch (ThreadAbortException) { }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 设置内容格式
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileExtension"></param>
        private void SetContentType(HttpContext context, string fileExtension)
        {
            //判断是否为xml格式
            if (fileExtension.ToLower() == ".xml")
                context.Response.ContentType = "text/xml;charset=utf-8";
            else if (fileExtension.ToLower() == ".js")
                context.Response.ContentType = "text/javascript;charset=utf-8";
        }

        /// <summary>
        /// 检测是否需要更新
        /// </summary>
        /// <param name="context"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool CheckUpdate(HttpContext context, string url)
        {
            try
            {
                if (!url.ToLower().Contains("http://"))
                {
                    url = string.Format("http://{0}/{1}", context.Request.Url.Authority, url.TrimStart('~', '/'));
                }

                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                string value = client.DownloadString(url);

                //转换值
                bool ret = false;
                if (!bool.TryParse(value, out ret))
                {
                    int r = 0;
                    if (int.TryParse(value, out r))
                    {
                        ret = r == 1 ? true : false;
                    }
                }
                return ret;
            }
            catch
            {
                return false;
            }
        }
    }
}
