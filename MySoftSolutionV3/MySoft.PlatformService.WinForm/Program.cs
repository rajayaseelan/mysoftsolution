using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using MySoft.Logger;

namespace MySoft.PlatformService.WinForm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program_UnhandledException);
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(Program_UnhandledException);
        }

        static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            //SimpleLog.Instance.WriteLog(exception);
            SimpleLog.Instance.WriteLogWithSendMail(exception, "my181@163.com");
        }
    }
}
