using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MySoft.Mail;
using System.Threading;
using System.Linq;
using MySoft.Threading;

namespace MySoft.Logger
{
    /// <summary>
    /// 简单日志管理类(按日期生成文件)
    /// </summary>
    public class SimpleLog
    {
        /// <summary>
        /// 简单日志的单例 (默认路径为根目录下的Logs目录)
        /// </summary>
        public static readonly SimpleLog Instance = new SimpleLog(AppDomain.CurrentDomain.BaseDirectory);

        private string basedir;
        private Queue<LogInfo> logqueue;

        /// <summary>
        /// 设置基准路径
        /// </summary>
        /// <param name="basedir"></param>
        public void SetBaseDir(string basedir)
        {
            this.basedir = basedir;
        }

        /// <summary>
        /// 实例化简单日志组件
        /// </summary>
        /// <param name="basedir">日志存储根目录，下面会自动创建Log与ErrorLog文件夹</param>
        public SimpleLog(string basedir)
        {
            this.basedir = basedir;
            this.logqueue = new Queue<LogInfo>();

            //启动生成文件线程
            ManagedThreadPool.QueueUserWorkItem((state) =>
            {
                while (true)
                {
                    if (logqueue.Count > 0)
                    {
                        var list = new List<LogInfo>();
                        lock (logqueue)
                        {
                            list.AddRange(logqueue.ToArray());
                            logqueue.Clear();
                        }

                        //对日志按路径进行分组
                        var logs = list.GroupBy(p => p.FilePath).Select(p => new LogInfo
                        {
                            FilePath = p.Key,
                            Log = string.Concat(p.Select(l => l.Log).ToArray())
                        }).ToList();

                        //批量写日志
                        logs.ForEach(loginfo =>
                        {
                            try
                            {
                                string dir = Path.GetDirectoryName(loginfo.FilePath);
                                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                File.AppendAllText(loginfo.FilePath, loginfo.Log, Encoding.UTF8);
                            }
                            catch { }

                            Thread.Sleep(10);
                        });
                    }

                    //等待100毫秒
                    Thread.Sleep(100);
                }
            });
        }

        #region 自动创建文件

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="ex"></param>
        public void WriteLog(Exception ex)
        {
            WriteLogForDir(string.Empty, ex);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="ex"></param>
        public void WriteLogForDir(string dir, Exception ex)
        {
            if (ex == null) return;

            string filePath = Path.Combine(Path.Combine(basedir, "ErrorLogs"), dir);
            filePath = Path.Combine(filePath, DateTime.Now.ToString("yyyy-MM-dd"));

            var appName = GetApplicationName(ex);
            if (!string.IsNullOrEmpty(appName)) filePath = Path.Combine(filePath, appName);

            filePath = Path.Combine(filePath, string.Format("{0}.log", GetServiceFile(ex)));

            WriteFileLog(filePath, ex);
        }

        /// <summary>
        /// 获取服务路径
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetServiceFile(Exception ex)
        {
            var errorName = ex.GetType().Name;

            //获取内部异常
            if (ex.InnerException != null)
                errorName = ex.InnerException.GetType().Name;

            return errorName;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        public void WriteLog(string log)
        {
            WriteLogForDir(string.Empty, log);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        public void WriteLogForDir(string dir, string log)
        {
            if (string.IsNullOrEmpty(log)) return;

            string filePath = Path.Combine(Path.Combine(basedir, "Logs"), dir);
            filePath = Path.Combine(filePath, string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd")));

            WriteFileLog(filePath, log);
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string log, string mailTo)
        {
            WriteLogWithSendMail(log, new string[] { mailTo });
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string log, string[] mailTo)
        {
            WriteLog(log);
            SendMail(log, mailTo);
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(Exception ex, string mailTo)
        {
            WriteLogWithSendMail(ex, new string[] { mailTo });
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(Exception ex, string[] mailTo)
        {
            WriteLog(ex);
            SendMail(ex, mailTo);
        }

        #endregion

        #region 传入文件信息

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ex"></param>
        public void WriteLog(string fileName, Exception ex)
        {
            string log = ErrorHelper.GetErrorWithoutHtml(ex);
            WriteLog(fileName, log);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="log"></param>
        public void WriteLog(string fileName, string log)
        {
            string filePath = Path.Combine(basedir, fileName);
            WriteFileLog(filePath, log);
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, string log, string mailTo)
        {
            WriteLogWithSendMail(fileName, log, new string[] { mailTo });
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, string log, string[] mailTo)
        {
            WriteLog(fileName, log);
            SendMail(log, mailTo);
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, Exception ex, string mailTo)
        {
            WriteLogWithSendMail(fileName, ex, new string[] { mailTo });
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, Exception ex, string[] mailTo)
        {
            WriteLog(fileName, ex);
            SendMail(ex, mailTo);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="to"></param>
        private void SendMail(string log, string[] to)
        {
            if (to == null || to.Length == 0)
            {
                throw new ArgumentException("请传入收件人地址信息参数！");
            }

            var body = CoreHelper.GetSubString(log, 20, "...");
            string title = string.Format("{2} - 普通邮件由【{0}({1})】发出", DnsHelper.GetHostName(), DnsHelper.GetIPAddress(), body);
            SmtpMail.Instance.SendAsync(title, log, to);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="to"></param>
        private void SendMail(Exception ex, string[] to)
        {
            if (to == null || to.Length == 0)
            {
                throw new ArgumentException("请传入收件人地址信息参数！");
            }

            string title = string.Format("【{2}】({3}) - 异常邮件由【{0}({1})】发出", DnsHelper.GetHostName(), DnsHelper.GetIPAddress(),
                                        GetAppTitle(ex), GetServiceTitle(ex));

            SmtpMail.Instance.SendExceptionAsync(ex, title, to);
        }

        /// <summary>
        /// 获取App标题
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetAppTitle(Exception ex)
        {
            var appName = GetApplicationName(ex);
            return string.IsNullOrEmpty(appName) ? "未知应用" : appName;
        }

        /// <summary>
        /// 获取服务标题
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetServiceTitle(Exception ex)
        {
            var serviceName = GetServiceName(ex);
            var errorName = ex.GetType().Name;

            //获取内部异常
            if (ex.InnerException != null)
                errorName = ex.InnerException.GetType().Name;

            return string.IsNullOrEmpty(serviceName) ? errorName : errorName + " : " + serviceName;
        }

        private string GetApplicationName(Exception ex)
        {
            if (ex == null) return null;
            if (ex.Data.Contains("ApplicationName"))
                return ex.Data["ApplicationName"].ToString();
            else
                return null;
        }

        private string GetServiceName(Exception ex)
        {
            if (ex == null) return null;
            if (ex.Data.Contains("ServiceName"))
                return ex.Data["ServiceName"].ToString();
            else
                return null;
        }

        /// <summary>
        /// 写文件日志
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ex"></param>
        private void WriteFileLog(string filePath, Exception ex)
        {
            string log = ErrorHelper.GetErrorWithoutHtml(ex);
            WriteFileLog(filePath, log);
        }

        /// <summary>
        /// 写文件日志
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="log"></param>
        private void WriteFileLog(string filePath, string log)
        {
            lock (logqueue)
            {
                log = string.Format("【{0}】 ==> {1}{2}{2}========================================================================================================================{2}{2}",
                                    DateTime.Now.ToLongTimeString(), log, Environment.NewLine);
                var loginfo = new LogInfo { FilePath = filePath, Log = log };

                //将信息入队列
                logqueue.Enqueue(loginfo);
            }
        }

        #endregion

        private class LogInfo
        {
            public string FilePath { get; set; }
            public string Log { get; set; }
        }
    }
}
