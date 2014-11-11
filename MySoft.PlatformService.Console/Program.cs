using System;
using MySoft.IoC;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.PlatformService.Console
{
    class UserA
    {
        public int id { get; set; }
        public int age { get; set; }
    }

    class UserB
    {
        public int id { get; set; }
        public string sex { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //var userlist1 = new List<UserA> { new UserA { id = 1, age = 20 }, new UserA { id = 2, age = 30 } };
            //var userlist2 = new List<UserB> { new UserB { id = 1, sex = "aaa" }, new UserB { id = 3, sex = "bbb" } };

            //var query = from p1 in userlist1
            //            join p2 in userlist2 on p1.id equals p2.id
            //            select new { p1.id, p1.age, p2.sex };

            //var list = query.ToList();


            server_OnLog("Server ready started...", LogType.Normal);

            var config = CastleServiceConfiguration.GetConfig();
            var server = new CastleService(config);
            server.OnLog += new LogEventHandler(server_OnLog);
            server.OnError += new ErrorLogEventHandler(server_OnError);
            server.Completed += new EventHandler<CallEventArgs>(server_OnCalling);
            server.Start();

            server_OnLog(string.Format("Tcp server host -> {0}", server.ServerUrl), LogType.Normal);
            server_OnLog(string.Format("Server publish ({0}) services.", server.ServiceCount), LogType.Normal);
            server_OnLog(string.Format("Press any key to exit and stop service..."), LogType.Normal);

            System.Console.ReadLine();

            server_OnLog("Server ready stopped...", LogType.Normal);
            server.Stop();

            System.Console.ReadLine();
        }

        /// <summary>
        /// 服务调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void server_OnCalling(object sender, CallEventArgs e)
        {
            if (e.IsError)
            {
                var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
                var body = string.Format("Remote client【{0}】call service ({1},{2}) error.\r\nParameters => {3}",
                            message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters);

                var error = IoCHelper.GetException(e.Caller, body, e.Error);

                //写异常日志
                server_OnError(error);
            }
            else
            {
                //耗时小于1秒返回
                if (e.ElapsedTime < 30000) return;

                var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
                var body = string.Format("Remote client【{0}】call service ({1},{2}), result ({4}) rows, elapsed time ({5}) ms.\r\nParameters => {3}",
                            message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters, e.Count, e.ElapsedTime);

                server_OnLog(body, LogType.Information);
            }
        }

        static void server_OnLog(string log, LogType type)
        {
            string message = "[" + DateTime.Now.ToString() + "] " + "=> <" + type + "> " + log;

            if (type == LogType.Error)
            {
                IoCHelper.WriteLine(ConsoleColor.Red, message);
            }
            else if (type == LogType.Warning)
            {
                IoCHelper.WriteLine(ConsoleColor.Yellow, message);
            }
            else if (type == LogType.Information)
            {
                IoCHelper.WriteLine(ConsoleColor.Green, message);
            }
            else
            {
                IoCHelper.WriteLine(message);
            }
        }

        static void server_OnError(Exception error)
        {
            string message = "[" + DateTime.Now.ToString() + "] => " + error.Message;
            if (error.InnerException != null)
            {
                message += "\r\n错误信息 => " + ErrorHelper.GetInnerException(error).Message;
            }

            if (error is WarningException)
            {
                IoCHelper.WriteLine(ConsoleColor.Yellow, message);
            }
            else
            {
                IoCHelper.WriteLine(ConsoleColor.Red, message);
            }
        }
    }
}
