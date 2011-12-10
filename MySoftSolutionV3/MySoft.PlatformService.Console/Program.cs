using System;
using System.Collections.Generic;
using System.Text;
using MySoft.IoC;
using System.Collections;
using MySoft.Remoting;
using MySoft.IoC.Configuration;
using MySoft.Logger;
using MySoft.IoC.Status;

namespace MySoft.PlatformService.Console
{
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
            server.OnCalling += new EventHandler<CallEventArgs>(server_OnCaller);
            server.OnLog += new LogEventHandler(Program_OnLog);
            server.OnError += new ErrorLogEventHandler(Program_OnError);
            server.Start();

            //mongo.Connect();

            System.Console.WriteLine("Server host -> {0}", server.ServerUrl);
            System.Console.WriteLine("Press any key to exit and stop service...");
            System.Console.ReadLine();
        }

        static void server_OnCaller(object sender, CallEventArgs e)
        {
            //var database = mongo.GetDatabase("ServiceMonitor");
            //var collection = database.GetCollection("ServiceInfo");

            //var doc = new Document();
            //doc["AppName"] = e.Caller.AppName;
            //doc["IPAddress"] = e.Caller.IPAddress;
            //doc["HostName"] = e.Caller.HostName;
            //doc["ServiceName"] = e.Caller.ServiceName;
            //doc["SubServiceName"] = e.Caller.SubServiceName;
            //doc["ElapsedTime"] = Convert.ToDouble(e.ElapsedTime);
            //doc["RowCount"] = e.RowCount;
            //doc["Error"] = e.CallError;
            //if (e.CallError != null)
            //    doc["IsError"] = true;
            //else
            //    doc["IsError"] = false;

            //collection.Insert(doc);
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

        static void Program_OnError(Exception exception)
        {
            string message = "[" + DateTime.Now.ToString() + "] => " + exception.Message;
            if (exception.InnerException != null)
            {
                message += "\r\n´íÎóÐÅÏ¢ => " + ErrorHelper.GetInnerException(exception).Message;
            }

            lock (syncobj)
            {
                if (exception is WarningException)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(message);

                //SimpleLog.Instance.WriteLogWithSendMail(exception, "maoyong@fund123.cn");

                //SimpleLog.Instance.WriteLog(message);
            }
        }
    }
}
