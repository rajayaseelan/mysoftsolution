using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using MySoft.Logger;
using System.Text.RegularExpressions;

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
            if (isStartUpdate)
            {
                ThreadPool.QueueUserWorkItem(StartUpdate);
            }

            Thread thread = new Thread(DoWork);

            //单位：秒
            thread.Start(interval * 1000);
        }

        //开始生成
        static void StartUpdate(object value)
        {
            RunUpdate(DateTime.MaxValue);
        }

        static void RunUpdate(DateTime updateTime)
        {
            lock (staticPageItems)
            {
                foreach (IStaticPageItem sti in staticPageItems)
                {
                    try
                    {
                        //需要生成才启动线程
                        if (sti.NeedUpdate(updateTime))
                        {
                            System.Threading.ThreadPool.QueueUserWorkItem(obj =>
                            {
                                if (obj == null) return;

                                ArrayList arr = obj as ArrayList;
                                IStaticPageItem item = arr[0] as IStaticPageItem;
                                DateTime time = (DateTime)arr[1];
                                item.Update(time);
                            }, new ArrayList { sti, updateTime });
                        }
                    }
                    catch (Exception ex)
                    {
                        var exception = new StaticPageException("执行页面生成出现异常：" + ex.Message, ex);
                        if (OnError != null)
                        {
                            try { OnError(exception); }
                            catch { }
                        }
                    }
                }
            }
        }

        //执行生成事件
        static void DoWork(object value)
        {
            while (true)
            {
                RunUpdate(DateTime.Now);

                //休眠间隔
                Thread.Sleep(Convert.ToInt32(value));
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

        #endregion

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
                string path = templatePath.TrimStart('/');
                HttpRuntime.ProcessRequest(new EncodingWorkerRequest(path, query, sw, encoding));
            }

            string content = sb.ToString();

            //验证字符串
            if (string.IsNullOrEmpty(validateString))
            {
                throw new Exception("执行本地页面" + templatePath + (query == null ? "" : "?" + query) + "出错，验证字符串不能为空。");
            }
            else
            {
                if (content.IndexOf(validateString) >= 0) return content;
                throw new Exception("执行本地页面" + templatePath + (query == null ? "" : "?" + query) + "出错，页面内容和验证字符串匹配失败。");
            }
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
            WebClient wc = new WebClient();
            wc.Encoding = encoding;

            //下载内容
            string result = wc.DownloadString(templatePath);

            //验证字符串
            if (string.IsNullOrEmpty(validateString))
            {
                throw new Exception("执行远程页面" + templatePath + "出错，验证字符串不能为空。");
            }
            else
            {
                if (result.IndexOf(validateString) >= 0) return result;
                throw new Exception("执行远程页面" + templatePath + "出错，页面内容和验证字符串匹配失败。");
            }
        }

        /// <summary>
        ///  保存字符串到路径
        /// </summary>
        /// <param name="content">结果字符串</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="outEncoding">文件保存页面编码</param>
        internal static void SaveFile(string content, string savePath, Encoding outEncoding)
        {
            try
            {
                //将内容写入文件
                string dir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                //创建一个写文件流
                using (StreamWriter sw = new StreamWriter(File.Create(savePath), outEncoding))
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.Write(content);
                        sw.Flush();
                    }

                    sw.Close();
                }

                //生成文件成功写日志
                SaveLog(string.Format("生成文件【{0}】成功！", savePath), LogType.Information);
            }
            catch (IOException ex)
            {
                string logText = string.Format("{0}\r\n生成文件【{1}】失败！", ex.Message, savePath);

                //将日志写入文件
                SimpleLog.Instance.WriteLog(new StaticPageException(logText, ex));
            }
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
        private MemoryStream stream;

        public EncodingWorkerRequest(string page, string query, TextWriter output)
            : base(page, query, output)
        {
            this.output = output;
            this.encoding = Encoding.UTF8;
            this.stream = new MemoryStream();
        }

        public EncodingWorkerRequest(string page, string query, TextWriter output, Encoding encoding)
            : base(page, query, output)
        {
            this.output = output;
            this.encoding = encoding;
            this.stream = new MemoryStream();
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (length > 0 && data != null)
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public override void FlushResponse(bool finalFlush)
        {
            if (finalFlush)
            {
                StreamReader sr = new StreamReader(stream, encoding);
                stream.Position = 0;
                stream.Flush();

                string content = sr.ReadToEnd();

                //把内容写入输出流中
                output.Write(content);
            }
        }
    }
}
