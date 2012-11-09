using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using MySoft.Logger;

namespace MySoft.Web
{
    /// <summary>
    /// 静态页处理类
    /// </summary>
    public abstract class StaticPageManager
    {
        private const int INTERVAL = 60;

        public static event LogEventHandler OnLog;

        public static event ErrorLogEventHandler OnError;

        private static bool isRunning = false;

        //静态页生成项
        private static List<IStaticPageItem> staticPageItems = new List<IStaticPageItem>();

        #region 启动静态页生成

        /// <summary>
        /// 启动静态管理类
        /// </summary>
        public static void Start()
        {
            Start(INTERVAL);
        }

        /// <summary>
        /// 启动静态管理类
        /// </summary>
        public static void Start(bool isStartUpdate)
        {
            Start(INTERVAL, isStartUpdate);
        }

        /// <summary>
        /// 启动静态管理类
        /// </summary>
        /// <param name="interval">检测间隔时间(默认为一分钟) ：单位（秒）</param>
        public static void Start(int interval)
        {
            Start(interval, false);
        }

        /// <summary>
        /// 启动静态管理类
        /// </summary>
        /// <param name="interval">检测间隔时间：单位（秒）</param>
        public static void Start(int interval, bool isStartUpdate)
        {
            if (isRunning) return;

            isRunning = true;

            if (isStartUpdate)
            {
                //启动一个临时线程生成
                ThreadPool.QueueUserWorkItem(state => RunUpdate(DateTime.MaxValue));
            }

            //启动一个循环线程生成
            Thread thread = new Thread(state =>
            {
                if (state == null) return;

                var timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(state));

                while (true)
                {
                    try
                    {
                        RunUpdate(DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                    }

                    //休眠间隔
                    Thread.Sleep(timeSpan);
                }
            });

            //单位：秒
            thread.Start(interval);
        }

        static void RunUpdate(DateTime updateTime)
        {
            lock (staticPageItems)
            {
                foreach (IStaticPageItem sti in staticPageItems)
                {
                    //需要生成才启动线程
                    if (sti.NeedUpdate(updateTime))
                    {
                        try
                        {
                            sti.Update(updateTime);
                        }
                        catch (Exception ex)
                        {
                            var exception = new StaticPageException("执行页面生成出现异常：" + ex.Message, ex);
                            if (OnError != null)
                            {
                                try
                                {
                                    OnError(exception);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 成批添加静态页生成项
        /// </summary>
        /// <param name="items">静态生成项</param>
        public static void AddItem(params IStaticPageItem[] items)
        {
            lock (staticPageItems)
            {
                foreach (IStaticPageItem item in items)
                {
                    if (!staticPageItems.Contains(item))
                        staticPageItems.Add(item);
                }
            }
        }

        /// <summary>
        /// 设置统一远程请求与域名信息
        /// </summary>
        /// <param name="domainUri"></param>
        public static void SetRemote(string domainUri)
        {
            lock (staticPageItems)
            {
                foreach (IStaticPageItem item in staticPageItems)
                {
                    if (item.IsRemote) continue;

                    item.SetDomain(domainUri);
                }
            }
        }

        #endregion

        /// <summary>
        /// 生成本地页面
        /// </summary>
        /// <param name="templatePath">来路径。如:/Default.aspx?code=123456</param>
        /// <param name="savePath">保存到路径。全路径。</param>
        /// <param name="validateString">验证字符串</param>
        internal static bool CreateStaticPage(String templatePath, String savePath, String validateString)
        {
            try
            {
                var content = string.Empty;

                //判断是否有http://
                if (!templatePath.ToLower().StartsWith("http://"))
                {
                    var arr = templatePath.Split('?');
                    var url = arr.Length > 0 ? arr[0] : string.Empty;
                    var query = arr.Length > 1 ? arr[1] : string.Empty;

                    content = StaticPageManager.GetLocalPageString(url, query, Encoding.UTF8, validateString);
                }
                else
                {
                    content = StaticPageManager.GetRemotePageString(templatePath, Encoding.UTF8, validateString);
                }

                var dynamicurl = templatePath;
                var staticurl = savePath;

                //加入静态页生成元素
                content = string.Format("{3}\r\n\r\n<!-- 生成方式：Redirect方式生成 -->\r\n<!-- 更新时间：{0} -->\r\n<!-- 动态URL：{1} -->\r\n<!-- 静态URL：{2} -->",
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dynamicurl, RemoveRootPath(staticurl), content.Trim());


                StaticPageManager.SaveFile(content, savePath, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException("执行页面" + templatePath + "生成出现异常！", ex));
                return false;
            }
        }

        /// <summary>
        /// 去除根目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string RemoveRootPath(string path)
        {
            try
            {
                return path.Replace(AppDomain.CurrentDomain.BaseDirectory, "/").Replace("\\", "/").Replace("//", "/");
            }
            catch
            {
                return path;
            }
        }

        #region 生成页面

        /// <summary>
        /// 生成远程页面
        /// </summary>
        /// <param name="templatePath">模板文件路径，如:http://www.163.com</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="inEncoding">模板页面编码</param>
        /// <param name="outEncoding">文件保存页面编码</param>
        /// <param name="validateString">验证字符串</param>
        public static bool CreateRemotePage(string templatePath, string savePath, string validateString, Encoding inEncoding, Encoding outEncoding)
        {
            try
            {
                SaveFile(GetRemotePageString(templatePath, inEncoding, validateString), savePath, outEncoding);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException(string.Format("生成静态文件{0}失败！", savePath), ex));
                return false;
            }
        }

        /// <summary>
        /// 生成远程页面
        /// </summary>
        /// <param name="templatePath">模板文件路径，如:http://www.163.com</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="validateString">验证字符串</param>
        public static bool CreateRemotePage(string templatePath, string savePath, string validateString)
        {
            try
            {
                SaveFile(GetRemotePageString(templatePath, Encoding.UTF8, validateString), savePath, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException(string.Format("生成静态文件{0}失败！", savePath), ex));
                return false;
            }
        }

        /// <summary>
        /// 生成本地页面
        /// </summary>
        /// <param name="templatePath">模板文件路径，如:/Default.aspx</param>
        /// <param name="query">查询字符串</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="inEncoding">模板页面编码</param>
        /// <param name="outEncoding">文件保存页面编码</param>
        /// <param name="validateString">验证字符串</param>
        public static bool CreateLocalPage(string templatePath, string query, string savePath, string validateString, Encoding inEncoding, Encoding outEncoding)
        {
            try
            {
                SaveFile(GetLocalPageString(templatePath, query, inEncoding, validateString), savePath, outEncoding);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException(string.Format("生成静态文件{0}失败！", savePath), ex));
                return false;
            }
        }

        /// <summary>
        /// 生成本地页面
        /// </summary>
        /// <param name="templatePath">模板文件路径，如:/Default.aspx</param>
        /// <param name="query">查询字符串</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="validateString">验证字符串</param>
        public static bool CreateLocalPage(string templatePath, string query, string savePath, string validateString)
        {
            try
            {
                SaveFile(GetLocalPageString(templatePath, query, Encoding.UTF8, validateString), savePath, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException(string.Format("生成静态文件{0}失败！", savePath), ex));
                return false;
            }
        }

        /// <summary>
        /// 生成本地页面
        /// </summary>
        /// <param name="templatePath">模板文件路径，如:/Default.aspx</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="validateString">验证字符串</param>
        /// <param name="outEncoding">文件保存页面编码</param>
        /// <param name="validateString">验证字符串</param>
        public static bool CreateLocalPage(string templatePath, string savePath, string validateString, Encoding inEncoding, Encoding outEncoding)
        {
            try
            {
                SaveFile(GetLocalPageString(templatePath, null, inEncoding, validateString), savePath, outEncoding);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException(string.Format("生成静态文件{0}失败！", savePath), ex));
                return false;
            }
        }


        /// <summary>
        /// 生成本地页面
        /// </summary>
        /// <param name="templatePath">模板文件路径，如:/Default.aspx</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="validateString">验证字符串</param>
        public static bool CreateLocalPage(string templatePath, string savePath, string validateString)
        {
            try
            {
                SaveFile(GetLocalPageString(templatePath, null, Encoding.UTF8, validateString), savePath, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                SaveError(new StaticPageException(string.Format("生成静态文件{0}失败！", savePath), ex));
                return false;
            }
        }

        #endregion

        #region 获取和保存页面内容

        /// <summary>
        /// 获取本地页面内容
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="query"></param>
        /// <param name="encoding"></param>
        /// <param name="validateString"></param>
        /// <returns></returns>
        internal static string GetLocalPageString(string templatePath, string query, Encoding encoding, string validateString)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                try
                {
                    var page = templatePath.Replace("/", "\\").TrimStart('\\');
                    HttpRuntime.ProcessRequest(new EncodingWorkerRequest(page, query, sw, encoding));
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
            }

            string content = sb.ToString();

            //验证字符串
            if (string.IsNullOrEmpty(validateString))
            {
                throw new WebException("执行本地页面" + templatePath + (query == null ? "" : "?" + query) + "出错，验证字符串不能为空。");
            }
            else if (content.IndexOf(validateString) < 0)
            {
                throw new WebException("执行本地页面" + templatePath + (query == null ? "" : "?" + query) + "出错，页面内容和验证字符串匹配失败。");
            }

            return content;
        }

        /// <summary>
        /// 获取远程页面内容
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="encoding"></param>
        /// <param name="validateString"></param>
        /// <returns></returns>
        internal static string GetRemotePageString(string templatePath, Encoding encoding, string validateString)
        {
            //判断是否有http://
            if (!templatePath.ToLower().StartsWith("http://"))
            {
                templatePath = "http://" + templatePath;
            }

            //下载内容
            WebClient wc = new WebClient();
            wc.Encoding = encoding;
            string content = wc.DownloadString(templatePath);

            //验证字符串
            if (string.IsNullOrEmpty(validateString))
            {
                throw new WebException("执行远程页面" + templatePath + "出错，验证字符串不能为空。");
            }
            else if (content.IndexOf(validateString) < 0)
            {
                throw new WebException("执行远程页面" + templatePath + "出错，页面内容和验证字符串匹配失败。");
            }

            return content;
        }

        /// <summary>
        ///  保存字符串到路径
        /// </summary>
        /// <param name="content">结果字符串</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="outEncoding">文件保存页面编码</param>
        internal static void SaveFile(string content, string savePath, Encoding outEncoding)
        {
            //将内容写入文件
            string dir = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //将内容写入文件中
            var newSavePath = string.Format("{0}.tmp", savePath);
            File.WriteAllText(newSavePath, content, outEncoding);

            while (true)
            {
                if (File.Exists(newSavePath))
                {
                    try
                    {
                        using (var sr = new StreamReader(newSavePath, outEncoding))
                        using (var sw = new StreamWriter(savePath, false, outEncoding))
                        {
                            sw.Write(sr.ReadToEnd());
                            sw.Flush();
                        }

                        File.Delete(newSavePath);

                        break;
                    }
                    catch
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                else
                {
                    break;
                }
            }

            //生成文件成功写日志
            SaveLog(string.Format("生成文件【{0}】成功！", savePath), LogType.Information);
        }

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="log"></param>
        internal static void SaveLog(string log, LogType type)
        {
            if (OnLog != null)
            {
                OnLog(log, type);
            }
        }

        /// <summary>
        /// 保存错误
        /// </summary>
        /// <param name="ex"></param>
        internal static void SaveError(StaticPageException ex)
        {
            if (OnError != null)
            {
                try
                {
                    OnError(ex);
                }
                catch (Exception e)
                {
                }
            }
        }

        #endregion
    }


    /// <summary>
    /// 简单请求处理类
    /// </summary>
    internal class EncodingWorkerRequest : SimpleWorkerRequest
    {
        private TextWriter output;
        private Encoding encoding;

        public EncodingWorkerRequest(string page, string query, TextWriter output)
            : base(page, query, output)
        {
            this.output = output;
            this.encoding = Encoding.UTF8;
        }

        public EncodingWorkerRequest(string page, string query, TextWriter output, Encoding encoding)
            : base(page, query, output)
        {
            this.output = output;
            this.encoding = encoding;
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            output.Write(encoding.GetString(data));
        }
    }
}
