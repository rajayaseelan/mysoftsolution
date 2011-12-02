using System;
using System.Collections.Generic;
using System.Text;
using MySoft.IoC;
using MySoft.Remoting;
using System.Threading;
using System.Diagnostics;
using MySoft.PlatformService.UserService;
using MySoft.Logger;
using MySoft.Cache;

namespace MySoft.PlatformService.Client
{
    class Program
    {
        ////写线程将数据写入myData
        //static int myData = 0;

        ////读写次数
        //const int readWriteCount = 10;

        ////false:初始时没有信号
        //static AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        //static void Main(string[] args)
        //{
        //    //开启一个读线程(子线程)
        //    Thread readerThread = new Thread(new ThreadStart(ReadThreadProc));
        //    readerThread.Name = "ReaderThread";
        //    readerThread.Start();

        //    for (int i = 1; i <= readWriteCount; i++)
        //    {
        //        Console.WriteLine("MainThread writing : {0}", i);

        //        //主(写)线程将数据写入
        //        myData = 0;

        //        //主(写)线程发信号，说明值已写过了
        //        //即通知正在等待的线程有事件发生
        //        autoResetEvent.Set();

        //        Thread.Sleep(1);
        //    }

        //    //终止线程
        //    //readerThread.Abort();

        //    Console.ReadKey();
        //}

        //static void ReadThreadProc()
        //{
        //    while (true)
        //    {
        //        //在数据被写入前，读线程等待（实际上是等待写线程发出数据写完的信号）
        //        autoResetEvent.WaitOne();
        //        Console.WriteLine("{0} reading : {1}", Thread.CurrentThread.Name, myData);
        //    }
        //}

        private static readonly object syncobj = new object();
        static void Main(string[] args)
        {
            //CastleFactoryConfiguration config = CastleFactoryConfiguration.GetConfig();

            //LogEventHandler logger = Console.WriteLine;
            //MemoryServiceMQ mq = new MemoryServiceMQ();
            //mq.OnLog += new LogEventHandler(mq_OnLog);
            //mq.OnError += new ErrorLogEventHandler(mq_OnError);

            //CastleServiceHelper cs = new CastleServiceHelper(config);
            //cs.OnLog += logger;
            //cs.PublishWellKnownServiceInstance(mq);

            //Console.WriteLine("Service MQ Server started...");
            //Console.WriteLine("Logger Status: On");
            //Console.WriteLine("Press any key to exit and stop server...");
            //Console.ReadLine();

            //CastleFactory.Create().OnError += new ErrorLogEventHandler(mq_OnError);
            //CastleFactory.Create().OnLog += new LogEventHandler(mq_OnLog);
            //Console.ReadKey();

            //int count = 1;

            //var castle = CastleFactory.Create();
            ////castle.RegisterCacheDependent(DefaultCacheDependent.Create());
            //castle.OnLog += new LogEventHandler(castle_OnLog);
            //castle.OnError += new ErrorLogEventHandler(castle_OnError);
            //IUserService service = castle.GetService<IUserService>();

            //IList<ServiceInfo> list = castle.GetService<IStatusService>().GetServiceInfoList();
            //var str = service.GetUserID();

            //service.GetUsers();
            //service.GetDictUsers();

            //try
            //{
            //    int userid;
            //    var user = service.GetUserInfo("maoyong", out userid);
            //    user = service.GetUserInfo("maoyong", out userid);
            //    user = service.GetUserInfo("maoyong", out userid);
            //    user = service.GetUserInfo("maoyong", out userid);
            //    user = service.GetUserInfo("maoyong", out userid);

            //    service.SetUser(null, ref userid);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            //for (int i = 0; i < count; i++)
            //{
            //    try
            //    {
            //        Thread thread = new Thread(DoWork);
            //        thread.Name = string.Format("Thread-->{0}", i);
            //        thread.IsBackground = true;
            //        thread.Start(service);
            //    }
            //    catch (Exception ex)
            //    {
            //        //WriteMessage(msg);
            //        castle_OnError(ex);
            //    }
            //}

            for (int i = 0; i < 2; i++)
            {
                Thread thread = new Thread(DoWork1);
                thread.Start();
            }
            Console.ReadKey();
        }

        static void DoWork1()
        {
            while (true)
            {
                try
                {
                    //CastleFactory.Create().OnError += new ErrorLogEventHandler(Program_OnError);
                    //var sub = CastleFactory.Create().GetChannel<IMessagePublishService>(new MessageListener());
                    //sub.Subscribe();
                    ////sub.Subscribe();

                    ////for (int i = 0; i < 10; i++)
                    ////{
                    ////    sub.Compute(100, i);
                    ////    Thread.Sleep(1000);
                    ////}

                    //Console.WriteLine("订阅服务IMessagePublishService成功！");
                    //Console.ReadLine();

                    //sub.Unsubscribe();

                    var users = CastleFactory.Create().GetChannel<IUserService>().GetUsers();
                    Console.WriteLine(users[0].Description);

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static void Program_OnError(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }

        public class MessageListener : IMessageListener
        {
            #region IMessageListener 成员

            public void Publish(string message)
            {
                Console.Write(DateTime.Now.ToString() + " --> ");
                Console.WriteLine(message);
            }

            #endregion

            #region IMessageListener 成员


            public void ShowData(int x, int y, int value)
            {
                Console.Write(DateTime.Now.ToString() + " --> ");
                Console.WriteLine("{0} + {1} = {2}", x, y, value);
            }

            #endregion
        }

        static void castle_OnError(Exception exception)
        {
            string message = "[" + DateTime.Now.ToString() + "] " + exception.Message;
            if (exception.InnerException != null)
            {
                message += "\r\n错误信息 => " + exception.InnerException.Message;
            }
            lock (syncobj)
            {
                if (exception is WarningException)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(message);
            }
        }

        static void castle_OnLog(string log, LogType type)
        {
            string message = "[" + DateTime.Now.ToString() + "] " + log;
            lock (syncobj)
            {
                if (type == LogType.Error)
                    System.Console.ForegroundColor = ConsoleColor.Red;
                else if (type == LogType.Warning)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine(message);
            }
        }

        static void DoWork(object value)
        {
            IUserService service = value as IUserService;
            while (true)
            {
                Stopwatch watch = Stopwatch.StartNew();
                try
                {
                    //int userid = service.GetUserID();
                    //UserInfo info = service.GetUserInfo("maoyong_" + new Random().Next(10000000), out userid);
                    //UserInfo info = service.GetUserInfo("maoyong", out userid);

                    var users = service.GetUsers();

                    if (users == null)
                    {
                        string msg = string.Format("线程：{0} 耗时：{1} ms 数据为null", Thread.CurrentThread.Name, watch.ElapsedMilliseconds);
                        //WriteMessage(msg);
                        castle_OnLog(msg, LogType.Error);
                    }
                    else
                    {
                        string msg = string.Format("线程：{0} 耗时：{1} ms 数据：{2}", Thread.CurrentThread.Name, watch.ElapsedMilliseconds, users.Count); //info.Description
                        //WriteMessage(msg);
                        castle_OnLog(msg, LogType.Information);
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("线程：{0} 耗时：{1} ms 异常：{2}", Thread.CurrentThread.Name, watch.ElapsedMilliseconds, ex.Message);
                    //WriteMessage(msg);
                    castle_OnLog(msg, LogType.Error);
                }

                Thread.Sleep(10);
            }
        }
    }
}
