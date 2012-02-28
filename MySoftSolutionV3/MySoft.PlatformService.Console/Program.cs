using System;
using System.Collections.Generic;
using System.Text;
using MySoft.IoC;
using System.Collections;
using MySoft.Remoting;
using MySoft.IoC.Configuration;
using MySoft.Logger;
using MySoft.IoC.HttpServer;
using MySoft.Net.Http;
using System.Collections.Specialized;

namespace MySoft.PlatformService.Console
{
    public class ServiceCache : IServiceCache
    {
        #region IServiceCache 成员

        public void AddCache(string key, object value, int seconds)
        {
            throw new NotImplementedException();
        }

        public T GetCache<T>(string key)
        {
            //throw new NotImplementedException();

            return default(T);
        }

        public void RemoveCache(string key)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }

    class Program
    {
        private static readonly object syncobj = new object();
        //private static readonly IMongo mongo = new Mongo("mongodb://192.168.1.223");
        static void Main(string[] args)
        {
            System.Console.BackgroundColor = ConsoleColor.DarkBlue;
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine("Service ready started...");

            CastleServiceConfiguration config = CastleServiceConfiguration.GetConfig();
            CastleService server = new CastleService(config);
            server.OnLog += new LogEventHandler(Program_OnLog);
            server.OnError += new ErrorLogEventHandler(Program_OnError);
            server.Start();

            if (config.HttpEnabled)
            {
                var caller = new HttpServiceCaller(server.Container, config.HttpPort);
                var factory = new HttpRequestHandlerFactory(caller);
                var httpServer = new HTTPServer(factory, config.HttpPort);
                httpServer.OnServerStart += () => { System.Console.WriteLine("Http server started. http://{0}:{1}", DnsHelper.GetIPAddress(), config.HttpPort); };
                httpServer.OnServerStop += () => { System.Console.WriteLine("Http server stoped."); };
                httpServer.OnServerException += ex => Program_OnError(ex);
                httpServer.Start();
            }

            System.Console.WriteLine("Server host -> {0}.", server.ServerUrl);
            System.Console.WriteLine("Service count -> {0} services.", server.ServiceCount);
            System.Console.WriteLine("Press any key to exit and stop service...");
            System.Console.ReadLine();
        }

        static void Program_OnLog(string log, LogType type)
        {
            string message = "[" + DateTime.Now.ToString() + "] " + "=> <" + type + "> " + log;
            lock (syncobj)
            {
                if (type == LogType.Error)
                    System.Console.ForegroundColor = ConsoleColor.Red;
                else if (type == LogType.Warning)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine(message);
            }
        }

        static void Program_OnError(Exception error)
        {
            string message = "[" + DateTime.Now.ToString() + "] => " + error.Message;
            if (error.InnerException != null)
            {
                message += "\r\n错误信息 => " + ErrorHelper.GetInnerException(error).Message;
            }

            lock (syncobj)
            {
                if (error is WarningException)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(message);

                //SimpleLog.Instance.WriteLogWithSendMail(error, "maoyong@fund123.cn");

                //SimpleLog.Instance.WriteLog(message);
            }
        }
    }
}
