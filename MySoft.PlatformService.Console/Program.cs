using System;
using MySoft.Cache;
using MySoft.IoC;
using MySoft.IoC.Configuration;
using MySoft.IoC.HttpServer;
using MySoft.Logger;
using MySoft.Net.Http;
using MySoft.IoC.Logger;
using MySoft.IoC.Messages;

namespace MySoft.PlatformService.Console
{
    class Program
    {
        private static readonly object syncobj = new object();
        //private static readonly IMongo mongo = new Mongo("mongodb://192.168.1.223");
        static void Main(string[] args)
        {
            var color = System.Console.ForegroundColor;

            System.Console.BackgroundColor = ConsoleColor.DarkBlue;
            System.Console.ForegroundColor = ConsoleColor.White;
            Program_OnLog("Service ready started...", LogType.Normal);

            var config = CastleServiceConfiguration.GetConfig();
            var server = new CastleService(config);
            server.OnLog += new LogEventHandler(Program_OnLog);
            server.OnError += new ErrorLogEventHandler(Program_OnError);
            server.Start();

            Program_OnLog(string.Format("Tcp server started. {0}", server.ServerUrl), LogType.Normal);
            Program_OnLog(string.Format("Service count -> {0} services.", server.ServiceCount), LogType.Normal);
            Program_OnLog(string.Format("Press any key to exit and stop service..."), LogType.Normal);

            System.Console.ForegroundColor = color;
            System.Console.ReadLine();

            server.Stop();
            System.Console.ReadLine();
        }

        static void Program_OnLog(string log, LogType type)
        {
            string message = "[" + DateTime.Now.ToString() + "] " + "=> <" + type + "> " + log;
            lock (syncobj)
            {
                var color = System.Console.ForegroundColor;

                if (type == LogType.Error)
                    System.Console.ForegroundColor = ConsoleColor.Red;
                else if (type == LogType.Warning)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else if (type == LogType.Information)
                    System.Console.ForegroundColor = ConsoleColor.Green;

                System.Console.WriteLine(message);

                System.Console.ForegroundColor = color;
            }
        }

        static void Program_OnError(Exception error)
        {
            if (error is NullReferenceException)
            {
                var a = 1;
            }

            string message = "[" + DateTime.Now.ToString() + "] => " + error.Message;
            if (error.InnerException != null)
            {
                message += "\r\n´íÎóÐÅÏ¢ => " + ErrorHelper.GetInnerException(error).Message;
            }

            lock (syncobj)
            {
                var color = System.Console.ForegroundColor;

                if (error is WarningException)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    System.Console.ForegroundColor = ConsoleColor.Red;

                System.Console.WriteLine(message);

                System.Console.ForegroundColor = color;
            }
        }
    }
}
