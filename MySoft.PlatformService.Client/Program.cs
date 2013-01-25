using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using MySoft.IoC;
using MySoft.IoC.Logger;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.PlatformService.UserService;

namespace MySoft.PlatformService.Client
{
    public class ServiceLog : IServiceLog
    {
        #region IServiceLog 成员

        public void Begin(CallMessage reqMsg)
        {
            //throw new NotImplementedException();
        }

        public void End(CallMessage reqMsg, ReturnMessage resMsg, long elapsedTime)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }

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
        private static int counter = 0;

        public class Test
        {
            public void Show()
            {
                Thread.Sleep(10);
            }
        }

        static void Main(string[] args)
        {
            //while (true)
            //{
            //    CacheHelper<IList<User>>.Get(LocalCacheType.File, "aaa", TimeSpan.FromSeconds(5), () => new List<User> { new User() });

            //    Thread.Sleep(1000);
            //}

            //var service = CastleFactory.Create().GetChannel<IUserService>();

            //var a = service.GetUser(new UserInfo());
            //Console.ReadLine();

            //var list = new List<ServerNode>()
            //{
            //    new ServerNode()
            //};

            //var fileName = CoreHelper.GetFullPath("/config/serverNode.config");
            //SimpleLog.WriteFile(fileName, SerializationManager.SerializeXml(list));

            //Console.ReadLine();

            //var ps = new ParameterCollection();
            //ps["aaa"] = 1;
            //ps["bbb"] = "2222";
            //ps["ccc"] = new { a = 1, b = 2 };

            //Console.WriteLine(ps.ToString());

            //var v = "%3CP+align%3Dcenter%3E%3C%2FP%3E%3CP+class%3Dpictext+align%3Dcenter%3E%E6%A0%91%E6%9C%A8%E5%B2%AD%E9%87%8D%E5%9E%8B%E6%9C%BA%E6%A2%B0%E5%8E%82%E4%B8%80%E7%89%A9%E6%B5%81%E5%9B%AD%E3%80%82%3C%2FP%3E%3CP+class%3Dpictext+align%3Dcenter%3E%3C%2FP%3E%3CP+class%3Dpictext+align%3Dcenter%3E%E4%BE%9B%E5%BA%94%E9%93%BE%E8%9E%8D%E8%B5%84%E5%8D%A0%E4%BC%81%E4%B8%9A%E5%85%A8%E9%83%A8%E8%9E%8D%E8%B5%8490%EF%BC%85%E4%BB%A5%E4%B8%8A%E7%9A%84%E6%9C%895%E5%AE%B6%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%85%B3%E4%BA%8E%E9%92%A2%E4%BB%B7%E7%9A%84%E6%9C%AA%E6%9D%A5%E8%B5%B0%E5%8A%BF%EF%BC%8C%E5%B8%82%E5%9C%BA%E7%9C%8B%E6%B3%95%E4%BB%8E%E6%9D%A5%E6%B2%A1%E6%9C%89%E7%BB%9F%E4%B8%80%E8%BF%87%EF%BC%8C%E8%80%8C%E5%AF%B9%E4%BA%8E%E5%BD%93%E5%89%8D%E9%A1%BD%E7%97%87%E7%9A%84%E8%AE%A4%E7%9F%A5%EF%BC%8C%E5%95%86%E5%AE%B6%E5%92%8C%E5%AD%A6%E8%80%85%E5%88%99%E4%BF%9D%E6%8C%81%E7%9D%80%E4%B8%80%E5%AE%9A%E7%9A%84%E9%BB%98%E5%A5%91%E5%BA%A6%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E2%80%9C%E4%B8%AD%E5%9B%BD%E9%92%A2%E9%93%81%E8%A1%8C%E4%B8%9A%E6%AD%A3%E5%A4%84%E4%BA%8E%E9%87%8D%E9%87%8D%E6%8C%91%E6%88%98%E7%9A%84%E5%85%B3%E9%94%AE%E6%97%B6%E6%9C%9F%EF%BC%8C%E5%9B%A0%E6%AD%A4%E9%92%A2%E9%93%81%E4%BA%A7%E4%B8%9A%E8%BD%AC%E5%8F%98%E7%BB%8F%E8%90%A5%E6%96%B9%E5%BC%8F%E3%80%81%E7%A0%94%E7%A9%B6%E9%92%A2%E9%93%81%E8%A1%8C%E4%B8%9A%E5%BE%AA%E7%8E%AF%E6%80%A7%E5%8F%91%E5%B1%95%E5%B0%B1%E6%98%BE%E5%BE%97%E5%B0%A4%E4%B8%BA%E9%87%8D%E8%A6%81%E3%80%82%E2%80%9D%E5%9B%BD%E5%8A%A1%E9%99%A2%E7%A0%94%E7%A9%B6%E5%AE%A4%E7%BB%BC%E5%90%88%E5%8F%B8%E5%8F%B8%E9%95%BF%E3%80%81%E5%8D%9A%E5%A3%AB%E7%94%9F%E5%AF%BC%E5%B8%88%E9%99%88%E6%96%87%E7%8E%B2%E8%AE%A4%E4%B8%BA%EF%BC%8C%E4%BA%A7%E4%B8%9A%E9%93%BE%E4%B8%8D%E5%8F%AA%E6%98%AF%E4%BE%9B%E5%BA%94%E5%92%8C%E9%9C%80%E6%B1%82%EF%BC%8C%E8%BF%98%E8%A6%81%E7%A0%94%E7%A9%B6%E9%87%8D%E6%96%B0%E7%BB%84%E5%90%88%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E8%80%8C%E8%BF%99%E8%83%8C%E5%90%8E%EF%BC%8C%E6%88%96%E8%AE%B8%E6%AD%A3%E6%98%AF%E7%8E%B0%E4%BB%A3%E9%92%A2%E8%B4%B8%E6%B5%81%E9%80%9A%E7%9A%84%E6%9C%BA%E4%BC%9A%E7%82%B9%E6%89%80%E5%9C%A8%E3%80%82%E6%9C%AC%E6%8A%A5%E8%AE%B0%E8%80%85%E6%96%87%E6%B4%81+%E9%95%BF%E6%B2%99%E6%8A%A5%E9%81%93%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E7%94%A8%E5%8D%8E%E8%8F%B1%E9%9B%86%E5%9B%A2%E8%91%A3%E4%BA%8B%E9%95%BF%E6%9B%B9%E6%85%A7%E6%B3%89%E7%9A%84%E8%AF%9D%E6%9D%A5%E8%AF%B4%EF%BC%8C%E9%92%A2%E9%93%81%E8%A1%8C%E4%B8%9A%E7%A1%AE%E5%AE%9E%E6%98%AF%E4%B8%80%E4%B8%AA%E5%91%A8%E6%9C%9F%E6%80%A7%E8%A1%8C%E4%B8%9A%EF%BC%8C%E2%80%9C%E8%80%8C%E7%8E%B0%E5%9C%A8%EF%BC%8C%E6%AF%94%E8%BE%83%E7%B3%9F%E7%9A%84%E6%83%85%E5%86%B5%E6%98%AF%E9%92%A2%E9%93%81%E8%A1%8C%E4%B8%9A%E5%91%A8%E6%9C%9F%E5%90%8C%E7%BB%8F%E6%B5%8E%E5%91%A8%E6%9C%9F%E9%87%8D%E5%8F%A0%E4%BA%86%E3%80%82%E2%80%9D%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%AF%B9%E4%BA%8E%E9%92%A2%E9%93%81%E8%A1%8C%E4%B8%9A%E6%9D%A5%E8%AF%B4%EF%BC%8C2012%E5%B9%B4%E5%B9%B6%E4%B8%8D%E7%AE%97%E4%B8%80%E4%B8%AA%E5%A5%BD%E5%85%89%E6%99%AF%E3%80%82%E5%B0%B1%E5%9C%A8%E4%B8%8A%E5%91%A8%E6%B9%96%E5%8D%97%E7%9C%81%E5%95%86%E5%8A%A1%E5%8E%85%E8%81%94%E5%90%88%E6%B9%96%E5%8D%97%E7%89%A9%E8%B5%84%E4%BF%A1%E6%81%AF%E4%B8%AD%E5%BF%83%E5%AF%B9%E4%B8%8A%E5%91%A8%EF%BC%888%E6%9C%8829%E6%97%A5-9%E6%9C%884%E6%97%A5%EF%BC%89%E6%88%91%E7%9C%8157%E5%AE%B6%E9%92%A2%E6%9D%90%E6%B5%81%E9%80%9A%E6%A0%B7%E6%9C%AC%E4%BC%81%E4%B8%9A%E5%B8%82%E5%9C%BA%E7%9B%91%E6%B5%8B%EF%BC%8C%E9%92%A2%E6%9D%90%E5%B8%82%E5%9C%BA%E4%BB%B7%E6%A0%BC%E7%BB%A7%E7%BB%AD%E4%B8%8B%E8%B7%8C%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E4%BD%86%E5%BE%AE%E5%A6%99%E7%9A%84%E6%98%AF%EF%BC%8C%E5%BA%93%E5%AD%98%E5%B7%B2%E7%BB%8F%E6%9C%89%E6%89%80%E6%B6%88%E8%A7%A3%E2%80%94%E2%80%94%E6%95%B0%E6%8D%AE%E6%98%BE%E7%A4%BA%EF%BC%8C%E7%9B%AE%E5%89%8D%EF%BC%8C%E9%95%BF%E6%B2%99%E5%B8%82%E5%9C%BA%E5%BB%BA%E7%AD%91%E9%92%A2%E6%9D%90%E5%BA%93%E5%AD%98%E6%80%BB%E9%87%8F%E7%BA%A621.22%E4%B8%87%E5%90%A8%EF%BC%8C%E8%BE%83%E4%B8%8A%E4%B8%8A%E5%91%A8%E5%87%8F%E5%B0%911.67%E4%B8%87%E5%90%A8%E3%80%82%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E4%BD%8E%E8%BF%B7%E4%B8%8B%E7%9A%84%E6%B5%81%E9%80%9A%E6%9C%BA%E4%BC%9A%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E4%BA%8B%E5%AE%9E%E4%B8%8A%EF%BC%8C%E4%BB%8E2011%E5%B9%B4%E4%B8%8B%E5%8D%8A%E5%B9%B4%E5%BC%80%E5%A7%8B%EF%BC%8C%E9%92%A2%E9%93%81%E8%A1%8C%E4%B8%9A%E6%80%A5%E8%BD%AC%E7%9B%B4%E4%B8%8B%EF%BC%8C%E8%87%B3%E4%BB%8A%E4%BB%8D%E6%9C%AA%E8%83%BD%E8%B5%B0%E5%87%BA%E5%BE%AE%E5%88%A9%E3%80%81%E4%BA%8F%E6%8D%9F%E7%9A%84%E5%B1%80%E9%9D%A2%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%9C%A8%E9%95%BF%E6%B2%99%E7%BB%8F%E8%90%A5%E9%92%A2%E8%B4%B8%E5%A4%9A%E5%B9%B4%E7%9A%84%E6%9D%A8%E6%98%8E%E5%AE%87%E6%84%9F%E8%A7%89%E5%B0%A4%E5%85%B6%E6%98%8E%E6%98%BE%EF%BC%8C%E2%80%9C%E7%9B%AE%E5%89%8D%E5%A4%A7%E8%A1%8C%E6%83%85%E4%B8%8D%E5%A5%BD%EF%BC%8C%E5%8E%BB%E5%B9%B4%E4%B8%8A%E5%8D%8A%E5%B9%B4%E6%8B%BF%E8%B4%A7%E8%BF%98%E8%A6%81%E6%8E%92%E9%98%9F%EF%BC%8C%E4%BD%86%E6%98%AF%E8%87%AA%E5%8E%BB%E5%B9%B4%E4%B8%8B%E5%8D%8A%E5%B9%B4%E4%BB%A5%E5%90%8E%EF%BC%8C%E5%86%8D%E6%B2%A1%E6%9C%89%E8%BF%99%E7%A7%8D%E6%83%85%E5%86%B5%EF%BC%8C%E5%9C%A8%E4%BB%93%E5%BA%93%E5%A4%96%E9%9D%A2%E4%B8%80%E7%9C%8B%EF%BC%8C%E5%9F%BA%E6%9C%AC%E6%B2%A1%E6%9C%89%E8%BD%A6%E5%9C%A8%E8%A3%85%E8%B4%A7%E3%80%82%E2%80%9D%E5%9C%A8%E4%BB%96%E7%9C%8B%E6%9D%A5%EF%BC%8C%E7%9B%AE%E5%89%8D%E5%B8%82%E5%9C%BA%E4%B8%8D%E5%A4%AA%E4%B9%90%E8%A7%82%EF%BC%8C%E9%92%A2%E5%8E%82%E5%8F%8A%E4%B8%80%E4%BA%9B%E5%A4%A7%E6%88%B7%E4%BA%8F%E6%8D%9F%EF%BC%8C%E4%BD%86%E4%B9%9F%E4%BC%9A%E6%9C%89%E5%B0%8F%E5%B9%85%E4%B8%8A%E6%B6%A8%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E4%BD%86%E8%BF%99%E6%88%96%E8%AE%B8%E6%84%8F%E5%91%B3%E7%9D%80%E6%96%B0%E7%9A%84%E6%9C%BA%E4%BC%9A%E2%80%94%E2%80%94%E4%BA%8B%E5%AE%9E%E4%B8%8A%EF%BC%8C%E5%9C%A8%E5%BE%88%E5%A4%9A%E4%B8%9A%E7%95%8C%E8%A7%82%E7%82%B9%E7%9C%8B%E6%9D%A5%EF%BC%8C%E4%BD%8E%E8%BF%B7%E6%9C%9F%E5%AF%B9%E4%BA%8E%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E8%80%8C%E8%A8%80%EF%BC%8C%E6%AD%A3%E6%98%AF%E4%B8%80%E4%B8%AA%E5%AE%9E%E7%8E%B0%E6%B5%81%E9%80%9A%E8%BD%AC%E5%9E%8B%E7%9A%84%E5%A5%BD%E6%97%B6%E6%9C%BA%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%9C%A8%E9%AB%98%E6%98%9F%E7%89%A9%E6%B5%81%E5%9B%AD%E8%B4%9F%E8%B4%A3%E4%BA%BA%E7%9C%8B%E6%9D%A5%EF%BC%8C%E9%9A%8F%E7%9D%80%E9%92%A2%E6%9D%90%E5%B8%82%E5%9C%BA%E5%92%8C%E4%BE%9B%E9%9C%80%E5%85%B3%E7%B3%BB%E7%9A%84%E7%A8%B3%E5%AE%9A%E5%BA%A6%E5%A2%9E%E5%8A%A0%EF%BC%8C%E5%B0%86%E4%BC%9A%E7%BB%99%E6%9C%AA%E6%9D%A5%E7%9A%84%E9%92%A2%E9%93%81%E7%BB%8F%E9%94%80%E7%89%A9%E6%B5%81%E6%A8%A1%E5%BC%8F%E7%9A%84%E5%88%9B%E6%96%B0%E5%A5%A0%E5%AE%9A%E5%9F%BA%E7%A1%80%EF%BC%8C%E2%80%9C%E6%8E%8C%E6%8F%A1%E6%B5%81%E9%80%9A%EF%BC%8C%E5%B0%B1%E6%8E%8C%E6%8F%A1%E4%BA%86%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E7%9A%84%E6%9C%AA%E6%9D%A5%E3%80%82%E2%80%9D%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E2%80%9C%E5%9C%A8%E5%90%8E%E5%8D%B1%E6%9C%BA%E6%97%B6%E4%BB%A3%EF%BC%8C%E5%AF%B9%E4%BA%8E%E6%97%A0%E5%BA%8F%E7%AB%9E%E4%BA%89%E7%9A%84%E9%92%A2%E8%B4%B8%E8%A1%8C%E4%B8%9A%E6%9D%A5%E8%AF%B4%EF%BC%8C%E5%B0%86%E9%9D%A2%E4%B8%B4%E6%9B%B4%E5%A4%9A%E6%8C%91%E6%88%98%EF%BC%8C%E9%94%80%E5%94%AE%E9%A2%9D%E4%B8%8D%E6%96%AD%E5%A2%9E%E5%8A%A0%E3%80%81%E8%80%8C%E5%88%A9%E6%B6%A6%E5%8D%B4%E4%B8%80%E7%9B%B4%E4%B8%8B%E6%BB%91%EF%BC%8C%E7%94%B1%E6%9A%B4%E5%88%A9%E6%97%B6%E4%BB%A3%E6%AD%A5%E5%85%A5%E5%BE%AE%E5%88%A9%E6%97%B6%E4%BB%A3%E7%94%9A%E8%87%B3%E2%80%98%E8%B4%9F%E5%88%A9%E6%97%B6%E4%BB%A3%E2%80%99%E3%80%82%E2%80%9D%E4%B8%AD%E5%9B%BD%E9%92%A2%E6%9D%90%E7%BD%91%E8%91%A3%E4%BA%8B%E9%95%BF%E5%A7%9A%E7%BA%A2%E8%B6%85%E8%AE%A4%E4%B8%BA%EF%BC%8C%E4%BB%8A%E5%90%8E%E5%A6%82%E4%B8%8D%E8%83%BD%E5%8F%8A%E6%97%B6%E8%B0%83%E6%95%B4%E7%BB%8F%E8%90%A5%E6%80%9D%E8%B7%AF%E3%80%81%E7%A8%B3%E5%9B%BA%E9%94%80%E5%94%AE%E6%B8%A0%E9%81%93%EF%BC%8C%E5%90%8E%E6%9C%9F%E7%9A%84%E7%94%9F%E5%AD%98%E7%8E%AF%E5%A2%83%E5%B0%86%E4%BC%9A%E5%8F%98%E5%BE%97%E6%9B%B4%E5%8A%A0%E8%89%B0%E9%9A%BE%E3%80%82%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E9%92%A2%E9%93%81%E7%89%A9%E6%B5%81%E5%9B%AD%E5%8C%BA%E7%9A%84%E5%88%9B%E6%96%B0%E5%8A%A8%E5%8A%9B%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%808%E6%9C%886%E6%97%A5%EF%BC%8C%E4%B8%80%E4%BB%BD%E3%80%8A%E5%8C%BA%E5%9F%9F%E6%99%BA%E8%83%BD%E7%89%A9%E6%B5%81%E5%9B%AD%E5%8C%BA%E8%81%94%E7%9B%9F%E6%88%98%E7%95%A5%E5%90%88%E4%BD%9C%E5%8D%8F%E8%AE%AE%E3%80%8B%E7%94%B1%E6%B9%96%E5%8D%97%E5%92%8C%E6%B1%9F%E8%8B%8F%E7%9A%84%E4%B8%A4%E5%AE%B6%E7%89%A9%E6%B5%81%E5%9B%AD%E5%85%B1%E5%90%8C%E7%AD%BE%E8%AE%A2%EF%BC%8C%E9%95%BF%E6%B2%99%E8%AF%95%E6%B0%B4%E6%99%BA%E8%83%BD%E7%89%A9%E6%B5%81%E8%BF%88%E5%87%BA%E7%AC%AC%E4%B8%80%E6%AD%A5%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E8%BF%99%E6%99%AE%E9%81%8D%E8%A2%AB%E8%A7%86%E4%B8%BA%E9%95%BF%E6%B2%99%E7%89%A9%E6%B5%81%E5%9B%AD%E5%8C%BA%E4%B8%80%E6%AC%A1%E5%85%A8%E6%96%B0%E7%9A%84%E5%B0%9D%E8%AF%95%E2%80%94%E2%80%94%E4%BA%8B%E5%AE%9E%E4%B8%8A%EF%BC%8C%E5%9C%A8%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E6%B5%81%E9%80%9A%E9%A2%86%E5%9F%9F%EF%BC%8C%E8%BF%BD%E9%80%90%E7%8E%B0%E4%BB%A3%E7%89%A9%E6%B5%81%E7%9A%84%E5%88%9B%E6%96%B0%EF%BC%8C%E4%B9%9F%E6%AD%A3%E5%9C%A8%E6%88%90%E4%B8%BA%E8%B6%8A%E6%9D%A5%E8%B6%8A%E8%BF%AB%E5%88%87%E7%9A%84%E9%9C%80%E6%B1%82%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E2%80%9C%E4%B8%80%E4%BD%93%E5%8C%96%E8%BF%90%E4%BD%9C%E3%80%81%E4%B8%93%E4%B8%9A%E5%8C%96%E6%9C%8D%E5%8A%A1%E3%80%81%E7%BD%91%E7%BB%9C%E5%8C%96%E7%BB%8F%E8%90%A5%E3%80%81%E4%BF%A1%E6%81%AF%E5%8C%96%E7%AE%A1%E7%90%86%E6%98%AF%E7%8E%B0%E4%BB%A3%E7%89%A9%E6%B5%81%E7%9A%84%E4%B8%BB%E8%A6%81%E7%89%B9%E5%BE%81%E3%80%82%E2%80%9D%E6%B9%96%E5%8D%97%E7%9C%81%E7%89%A9%E6%B5%81%E4%B8%8E%E9%87%87%E8%B4%AD%E8%81%94%E5%90%88%E4%BC%9A%E7%A7%98%E4%B9%A6%E9%95%BF%E5%BC%A0%E9%BE%99%E5%8F%91%E8%AF%B4%EF%BC%8C%E4%BD%9C%E4%B8%BA%E7%94%9F%E4%BA%A7%E6%9C%8D%E5%8A%A1%E8%A1%8C%E4%B8%9A%EF%BC%8C%E7%8E%B0%E4%BB%A3%E5%8C%96%E7%9A%84%E5%95%86%E8%B4%B8%E7%89%A9%E6%B5%81%EF%BC%8C%E5%8F%AF%E4%BB%A5%E6%9B%B4%E6%9C%89%E6%95%88%E9%99%8D%E4%BD%8E%E4%BC%81%E4%B8%9A%E7%BB%8F%E8%90%A5%E6%88%90%E6%9C%AC%E3%80%81%E6%8F%90%E5%8D%87%E4%BA%A7%E5%93%81%E7%9A%84%E9%99%84%E5%8A%A0%E5%80%BC%E5%92%8C%E7%AB%9E%E4%BA%89%E5%8A%9B%E3%80%81%E5%BB%B6%E4%BC%B8%E4%BA%A7%E4%B8%9A%E9%93%BE%E6%9D%A1%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%9C%A8%E9%95%BF%E6%B2%99%E4%B8%80%E7%89%A9%E6%B5%81%E5%9B%AD%E5%B7%B2%E7%BB%8F%E5%81%9A%E7%94%9F%E6%84%8F%E5%87%A0%E5%B9%B4%E7%9A%84%E9%95%BF%E6%B2%99%E4%BA%BA%E5%BC%A0%E5%B3%A5%E8%AF%B4%EF%BC%8C%E2%80%9C%E6%88%91%E6%9C%80%E7%9C%8B%E9%87%8D%E5%9B%AD%E5%8C%BA%E7%9A%84%E6%9C%8D%E5%8A%A1%E5%92%8C%E9%85%8D%E5%A5%97%E6%98%AF%E5%90%A6%E5%AE%8C%E5%96%84%E3%80%81%E5%81%A5%E5%85%A8%E3%80%82%E2%80%9D%E5%9C%A8%E4%BB%96%E7%9C%8B%E6%9D%A5%EF%BC%8C%E6%96%B0%E7%89%A9%E6%B5%81%E5%9B%AD%E9%9C%80%E8%A6%81%E6%9C%89%E5%85%B6%E6%A0%B8%E5%BF%83%E7%AB%9E%E4%BA%89%E5%8A%9B%EF%BC%8C%E5%AF%B9%E4%BA%8E%E9%92%A2%E8%B4%B8%E7%89%A9%E6%B5%81%E5%9B%AD%E6%9D%A5%E8%AF%B4%EF%BC%8C%E5%BB%BA%E7%AB%8B%E8%B5%B7%E6%95%B4%E4%B8%AA%E7%AB%8B%E4%BD%93%E7%9A%84%E6%B9%96%E5%8D%97%E4%BA%A4%E9%80%9A%E7%BD%91%E9%9D%9E%E5%B8%B8%E9%87%8D%E8%A6%81%E3%80%82%E2%80%9C%E6%AF%94%E5%A6%82%E8%B4%A7%E7%89%A9%E8%A6%81%E8%BF%90%E5%BE%80%E9%83%B4%E5%B7%9E%EF%BC%8C%E6%88%91%E4%BB%AC%E5%9C%A8%E5%93%AA%E9%87%8C%E5%87%BA%E8%B4%A7%E5%93%AA%E9%87%8C%E5%8D%B8%E8%B4%A7%E6%AF%94%E8%BE%83%E4%BE%BF%E6%8D%B7%EF%BC%8C%E8%8A%82%E7%BA%A6%E6%97%B6%E9%97%B4%E5%92%8C%E7%89%A9%E6%B5%81%E6%88%90%E6%9C%AC%EF%BC%9B%E6%88%91%E4%BB%AC%E7%9A%84%E5%87%BA%E8%B4%A7%E6%98%AF%E5%90%A6%E4%BE%BF%E6%8D%B7%EF%BC%8C%E4%B8%8D%E8%83%BD%E5%8F%AA%E8%80%83%E8%99%91%E5%88%B0%E8%BF%9B%E8%B4%A7%E4%BE%BF%E6%8D%B7%E7%9A%84%E9%97%AE%E9%A2%98%E3%80%82%E2%80%9D%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%BE%88%E6%98%8E%E6%98%BE%EF%BC%8C%E5%AF%B9%E4%BA%8E%E5%95%86%E8%B4%B8%E7%89%A9%E6%B5%81%E4%BC%81%E4%B8%9A%E8%87%AA%E8%BA%AB%E8%80%8C%E8%A8%80%EF%BC%8C%E5%9C%A8%E6%96%B0%E7%AB%9E%E4%BA%89%E6%A0%BC%E5%B1%80%E7%9A%84%E5%BD%A2%E6%88%90%E4%B8%AD%EF%BC%8C%E4%BC%81%E4%B8%9A%E4%B9%9F%E5%B0%86%E4%B8%8D%E5%86%8D%E4%BB%85%E5%B1%80%E9%99%90%E4%BA%8E%E8%BF%87%E5%8E%BB%E5%9C%B0%E6%AE%B5%E3%80%81%E9%85%8D%E5%A5%97%E3%80%81%E4%BA%A4%E9%80%9A%E7%AD%89%E7%A1%AC%E4%BB%B6%E6%96%B9%E9%9D%A2%E7%9A%84%E7%AB%9E%E4%BA%89%EF%BC%8C%E7%AB%9E%E4%BA%89%E4%B9%9F%E5%9C%A8%E5%90%91%E6%9B%B4%E5%A4%9A%E7%9A%84%E2%80%9C%E8%BD%AF%E5%AE%9E%E5%8A%9B%E2%80%9D%E6%96%B9%E5%90%91%E6%89%A9%E6%95%A3%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E8%80%8C%E8%BF%99%EF%BC%8C%E5%90%8C%E6%A0%B7%E6%98%AF%E6%96%B0%E5%85%B4%E7%9A%84%E6%B9%96%E5%8D%97%E9%AB%98%E6%98%9F%E7%89%A9%E6%B5%81%E5%9B%AD%E6%89%80%E7%9E%84%E5%87%86%E7%9A%84%E6%96%B9%E5%90%91%E3%80%82%E8%BF%99%E4%B8%AA%E7%BB%8F%E7%9C%81%E5%8F%91%E6%94%B9%E5%A7%94%E6%A0%B8%E5%87%86%E7%9A%84%E5%A4%A7%E5%9E%8B%E9%92%A2%E6%9D%90%E6%B5%81%E9%80%9A%E4%BA%A7%E4%B8%9A%E9%9B%86%E7%BE%A4%E9%A1%B9%E7%9B%AE%EF%BC%8C%E4%BE%9D%E6%89%98%E5%9B%BD%E5%AE%B6%E4%B8%80%E7%BA%A7%E7%AB%99%E2%80%94%E2%80%94%E9%95%BF%E6%B2%99%E7%81%AB%E8%BD%A6%E8%A5%BF%E8%B4%A7%E7%AB%99%E8%80%8C%E5%BB%BA%EF%BC%8C%E5%88%9A%E5%A5%BD%E4%BD%8D%E4%BA%8E%E5%A4%A7%E6%B2%B3%E8%A5%BF%E5%85%88%E5%AF%BC%E5%8C%BA%E7%9A%84%E6%A0%B8%E5%BF%83%E4%BD%8D%E7%BD%AE%E3%80%82%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E5%9F%8E%E5%B8%82%E8%A7%82%E5%AF%9F%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E6%9C%AA%E6%9D%A5%E9%95%BF%E6%B2%99%E5%B0%86%E6%89%93%E9%80%A0%E6%88%90%E4%B8%AD%E9%83%A8%E5%9C%B0%E5%8C%BA%E7%9A%84%E7%89%A9%E6%B5%81%E4%B8%AD%E5%BF%83%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E2%80%9C%E6%9C%AA%E6%9D%A5%E7%9A%84%E9%95%BF%E6%B2%99%E5%B0%86%E6%89%93%E9%80%A0%E4%B8%AD%E9%83%A8%E5%9C%B0%E5%8C%BA%E7%9A%84%E7%89%A9%E6%B5%81%E4%B8%AD%E5%BF%83%E3%80%82%E2%80%9D%E9%95%BF%E6%B2%99%E5%B8%82%E5%B7%A5%E4%BF%A1%E5%A7%94%E4%B8%BB%E4%BB%BB%E8%B5%B5%E8%B7%83%E9%A9%B7%E8%AF%B4%EF%BC%8C%E4%BE%9D%E6%89%98%E6%9C%AA%E6%9D%A5%E9%95%BF%E6%B2%99%E4%BE%BF%E6%8D%B7%E7%9A%84%E7%89%A9%E6%B5%81%EF%BC%8C%E4%BB%A5%E5%8F%8A%E9%95%BF%E6%B2%99%E5%91%A8%E8%BE%B9%E8%BE%90%E5%B0%84%E5%8C%BA%E5%9F%9F%E7%9A%84%E5%B7%A8%E5%A4%A7%E4%BA%BA%E6%B5%81%EF%BC%8C%E5%9F%8E%E5%B8%82%E7%9A%84%E5%BC%BA%E5%A4%A7%E7%AB%9E%E4%BA%89%E5%8A%9B%E5%B0%86%E4%B8%8D%E8%A8%80%E8%80%8C%E5%96%BB%E3%80%82%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E6%9F%90%E7%A7%8D%E6%84%8F%E4%B9%89%E4%B8%8A%E8%80%8C%E8%A8%80%EF%BC%8C%E7%89%A9%E6%B5%81%E4%B8%9A%E7%9A%84%E5%BF%AB%E9%80%9F%E5%8F%91%E5%B1%95%EF%BC%8C%E6%AD%A3%E5%9C%A8%E5%B8%A6%E6%9D%A5%E5%9F%8E%E5%B8%82%E7%BB%8F%E6%B5%8E%E6%A0%BC%E5%B1%80%E7%9A%84%E9%87%8D%E6%96%B0%E6%B4%97%E7%89%8C%EF%BC%9A%E5%BD%93%E6%B2%B3%E8%A5%BF%E7%9A%84%E7%89%A9%E6%B5%81%E5%9F%BA%E5%9C%B0%E5%BC%80%E5%A7%8B%E9%80%90%E6%B8%90%E6%88%90%E4%B8%BA%E6%8A%95%E8%B5%84%E8%80%85%E6%96%B0%E4%B9%90%E5%9B%AD%E7%9A%84%E6%97%B6%E5%80%99%EF%BC%8C%E5%9B%A0%E4%B8%BA%E7%89%A9%E6%B5%81%E5%9B%AD%E5%8C%BA%E5%92%8C%E5%B8%82%E5%9C%BA%E7%BE%A4%E8%90%BD%E7%9A%84%E6%90%AC%E8%BF%81%EF%BC%8C%E4%BE%9D%E6%89%98%E4%BA%8E%E6%B2%B3%E8%A5%BF%E7%9A%84%E4%BA%A4%E9%80%9A%E3%80%81%E7%BB%8F%E6%B5%8E%E4%BC%98%E5%8A%BF%EF%BC%8C%E4%B8%80%E4%B8%AA%E4%B8%93%E4%B8%9A%E5%8C%96%E7%9A%84%E7%89%A9%E6%B5%81%E5%9B%AD%E5%8C%BA%E5%BC%80%E5%A7%8B%E6%B5%AE%E5%87%BA%E6%B0%B4%E9%9D%A2%E3%80%82%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E6%95%B0%E5%AD%97%E9%92%A2%E8%B4%B8%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E4%BE%9B%E5%BA%94%E9%93%BE%E8%9E%8D%E8%B5%84%E5%8D%A0%E4%BC%81%E4%B8%9A%E5%85%A8%E9%83%A8%E8%9E%8D%E8%B5%8490%EF%BC%85%E4%BB%A5%E4%B8%8A%E7%9A%84%E6%9C%895%E5%AE%B6%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E6%B9%96%E5%8D%97%E7%9C%81%E9%87%91%E5%B1%9E%E6%9D%90%E6%96%99%E5%95%86%E4%BC%9A%E8%81%94%E5%90%88%E5%A4%A9%E8%B4%B8%E9%92%A2%E9%93%81%E7%BD%91%E7%BB%84%E7%BB%87%E7%9A%84%E3%80%8A%E6%B9%96%E5%8D%97%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E7%BB%8F%E8%90%A5%E6%83%85%E5%86%B5%E8%B0%83%E6%9F%A5%E3%80%8B%E4%BB%8E%E8%B0%83%E6%9F%A5%E7%9A%84%E6%B9%96%E5%8D%97%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E6%95%B0%E6%8D%AE%E7%BB%9F%E8%AE%A1%E5%BE%97%E7%9F%A5%EF%BC%9A%E9%87%87%E7%94%A8%E4%BE%9B%E5%BA%94%E9%93%BE%E8%9E%8D%E8%B5%84%E5%8D%A0%E4%BC%81%E4%B8%9A%E5%85%A8%E9%83%A8%E8%9E%8D%E8%B5%84%E7%9A%84%E6%AF%94%E4%BE%8B%EF%BC%9A90%EF%BC%85%E4%BB%A5%E4%B8%8A%E7%9A%845%E5%AE%B6%EF%BC%8C%E5%8D%A03%EF%BC%85%EF%BC%9B70%EF%BC%85%E4%BB%A5%E4%B8%8A%E7%9A%8417%E5%AE%B6%EF%BC%8C%E5%8D%A010%EF%BC%85%EF%BC%9B40%EF%BC%85%E4%BB%A5%E4%B8%8A%E7%9A%8432%E5%AE%B6%EF%BC%8C%E5%8D%A019%EF%BC%85%EF%BC%9B40%EF%BC%85%E4%BB%A5%E4%B8%8B%E7%9A%8458%E5%AE%B6%E5%8D%A034%EF%BC%85%E3%80%82%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E5%BB%B6%E4%BC%B8%E9%98%85%E8%AF%BB%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%3CSTRONG%3E%E3%80%80%E3%80%80%E9%92%A2%E8%B4%B8%E5%95%86%E8%A6%81%E4%BB%8E%E8%B4%B8%E6%98%93%E5%95%86%E8%BD%AC%E5%8F%98%E4%B8%BA%E6%B5%81%E9%80%9A%E5%95%86%3C%2FSTRONG%3E%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%9C%A88%E6%9C%888%E6%97%A5%E5%85%B0%E6%A0%BC%E9%92%A2%E9%93%81%E7%BD%91%E4%B8%BE%E5%8A%9E%E7%9A%84%E2%80%9C%E9%92%A2%E9%93%81%E5%95%86%E9%81%93%E7%A0%94%E7%A9%B6%E4%BC%9A%E7%AD%96%E5%88%92%E6%96%B9%E6%A1%88%E5%8F%91%E5%B8%83%E6%9A%A8%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E5%8D%B1%E6%9C%BA%E4%B8%8E%E6%9C%BA%E4%BC%9A%E4%B8%93%E9%A2%98%E7%A0%94%E8%AE%A8%E4%BC%9A%E2%80%9D%E4%B8%8A%EF%BC%8C%E5%BA%94%E9%82%80%E5%87%BA%E5%B8%AD%E5%B9%B6%E5%8F%91%E8%A1%A8%E6%BC%94%E8%AE%B2%E7%9A%84%E4%B8%AD%E5%9B%BD%E7%89%A9%E6%B5%81%E4%B8%8E%E9%87%87%E8%B4%AD%E8%81%94%E5%90%88%E4%BC%9A%E5%89%AF%E4%BC%9A%E9%95%BF%E8%94%A1%E8%BF%9B%E5%BB%BA%E8%AE%AE%E9%92%A2%E8%B4%B8%E5%95%86%E8%A6%81%E4%BB%8E%E8%B4%B8%E6%98%93%E5%95%86%E8%BD%AC%E5%8F%98%E4%B8%BA%E6%B5%81%E9%80%9A%E5%95%86%E3%80%82%E2%80%9C%E4%BC%81%E4%B8%9A%E7%9A%84%E4%B8%9A%E6%80%81%E5%B0%B1%E4%BB%8E%E4%BA%A4%E6%98%93%E4%B8%9A%E6%80%81%E8%BD%AC%E4%B8%BA%E6%B5%81%E9%80%9A%E4%B8%9A%E6%80%81%EF%BC%8C%E7%BB%8F%E8%90%A5%E5%86%85%E5%AE%B9%E5%B0%B1%E6%9B%B4%E5%8A%A0%E4%B8%B0%E5%AF%8C%E3%80%82%E2%80%9D%3C%2FP%3E%3CP%3E%E3%80%80%E3%80%80%E5%9C%A8%E8%BF%99%E6%AC%A1%E8%A1%8C%E4%B8%9A%E7%A0%94%E8%AE%A8%E4%BC%9A%E4%B8%8A%EF%BC%8C%E4%B8%80%E7%A7%8D%E5%A3%B0%E9%9F%B3%E8%AE%A4%E4%B8%BA%E6%9C%AA%E6%9D%A5%E7%BB%8F%E8%90%A5%E6%A8%A1%E5%BC%8F%E7%9A%84%E5%A4%9A%E5%85%83%E5%8C%96%E6%9C%89%E5%8F%AF%E8%83%BD%E6%88%90%E4%B8%BA%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E5%A4%A7%E5%8A%BF%E6%89%80%E8%B6%8B%E3%80%82%E2%80%9C%E9%92%A2%E8%B4%B8%E4%BC%81%E4%B8%9A%E5%8F%AF%E4%BB%A5%E6%A0%B9%E6%8D%AE%E8%87%AA%E8%BA%AB%E7%9A%84%E8%83%BD%E5%8A%9B%E3%80%81%E7%89%B9%E7%82%B9%E5%8F%8A%E7%BB%8F%E8%90%A5%E7%90%86%E5%BF%B5%EF%BC%8C%E9%80%89%E6%8B%A9%E5%81%9A%E8%B4%B8%E6%98%93%2B%E7%89%A9%E6%B5%81%E3%80%81%E8%B4%B8%E6%98%93%2B%E7%89%A9%E6%B5%81%2B%E5%8A%A0%E5%B7%A5%EF%BC%8C%E7%94%9A%E8%87%B3%E4%B8%8D%E5%81%9A%E8%B4%B8%E6%98%93%EF%BC%8C%E5%8F%AA%E5%81%9A%E6%B5%81%E9%80%9A%E3%80%82%E2%80%9D%E8%94%A1%E8%BF%9B%E8%AF%B4%E3%80%82%3C%2FP%3E";

            //var value = "name=中华人民共和国";
            ////var collection = HttpUtility.ParseQueryString(value, Encoding.UTF8);

            //var header = new WebHeaderCollection();

            //var a = HttpUtility.ParseQueryString("aa=" + HttpUtility.UrlEncode("中华人民共和国"));
            //header["X-AuthParameter"] = HttpUtility.UrlEncode("中华人民共和国");
            //var ret = new HttpHelper().Reader("http://127.0.0.1:8012/user.getuserfromname?name=" + HttpUtility.UrlEncode("中华人民共和国"));

            ////Console.WriteLine(collection.ToString());
            //Console.WriteLine();
            //Console.WriteLine(ret);

            //Console.ReadLine();

            #region test

            //var watch = Stopwatch.StartNew();

            //var t = new Test();
            //for (int i = 0; i < 1000; i++)
            //{
            //    t.Show();
            //}

            //watch.Stop();

            //Console.WriteLine("timeout: {0}.", watch.ElapsedMilliseconds);

            //watch = Stopwatch.StartNew();

            //var instance = DynamicCalls.GetInstanceCreator(typeof(Test));
            //var invoke = DynamicCalls.GetMethodInvoker(typeof(Test).GetMethod("Show"));
            //for (int i = 0; i < 1000; i++)
            //{
            //    //t.Show();
            //    invoke(t, null);
            //}

            //watch.Stop();

            //Console.WriteLine("timeout: {0}.", watch.ElapsedMilliseconds);

            //Console.ReadLine();
            //return;

            #endregion

            #region test1

            //var list = new List<int>();
            //for (int i = 0; i < 100; i++)
            //{
            //    list.Add(i);
            //}

            //var value = SerializationManager.SerializeBin(list);

            //CacheHelper.Insert("Cache", value, 100);

            //var obj = CacheHelper.Get<byte[]>("Cache");

            //var l = SerializationManager.DeserializeBin<List<int>>(obj);
            //l.Remove(1);
            //l.Remove(2);

            //l = SerializationManager.DeserializeBin<List<int>>(obj);
            //Console.WriteLine(l.Count);

            //Console.ReadLine();

            //var list = new List<User>();
            //for (int i = 0; i < 1000; i++)
            //{
            //    list.Add(new User { Id = i, Name = "test" + i });
            //}

            //Stopwatch watch = Stopwatch.StartNew();
            //var lb = new List<byte>();
            //for (int i = 0; i < 100; i++)
            //{
            //    var s = SerializationManager.SerializeBin(list);
            //    lb.AddRange(s);
            //}
            //watch.Stop();
            //Console.WriteLine(lb.Count + " - " + watch.ElapsedMilliseconds);

            //watch = Stopwatch.StartNew();

            //var set = new Polenter.Serialization.SharpSerializerBinarySettings();
            //set.Mode = Polenter.Serialization.BinarySerializationMode.SizeOptimized;
            //set.Encoding = Encoding.Default;

            //lb = new List<byte>();
            //var se = new Polenter.Serialization.SharpSerializer(set);
            //for (int i = 0; i < 100; i++)
            //{
            //    var stream = new MemoryStream();
            //    se.Serialize(list, stream);

            //    lb.AddRange(stream.ToArray());
            //}

            //watch.Stop();
            //Console.WriteLine(lb.Count + " - " + watch.ElapsedMilliseconds);

            #endregion

            #region test2

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

            //var watch = Stopwatch.StartNew();

            //GetRequestString();

            //var dd = watch.ElapsedMilliseconds;

            //Console.WriteLine("耗时: {0} ms", dd);
            //Console.ReadLine();
            //return;

            //var url = "http://openapi.mysoft.com/user.addusers.json?aaa=1&bbb=2";
            //var users = new { users = new[] { new { ID = 1, Name = "test1=aaa" }, new { ID = 2, Name = "test2" } } };
            ////var value = "users=" + SerializationManager.SerializeJson(users, false);

            ////var text = new HttpHelper(120).Poster(url, value);

            //var factory = RESTfulFactory.Create(url, DataFormat.JSON);
            //var token = factory.Invoke("user.addusers", users, HttpMethod.POST);

            //Console.WriteLine(token.ToString());

            //var url = "http://openapi.fund123.cn/post.json/myfund.addopenandfinancialfund?uid=yayiyao&pwd=2";
            //var v = HttpUtility.UrlEncode("[{\"AccountBookId\":\"d269da43-4e80-4c8e-ba69-32fca1fcf6e3\",\"ParentID\":\"\",\"Code\":\"100022\",\"Quotient\":1.26,\"BuyMoney\":1,\"BuyDate\":\"2012-04-24\",\"BonusType\":0,\"FrontOrBack\":1,\"RateType\":0,\"DeductFeeType\":0,\"FrontRate\":0.6,\"BuyRate\":0,\"BuyRate1\":0,\"BuyRate2\":0,\"BuyRate3\":0,\"Rate\":0.5,\"Rate1\":0.25,\"Rate2\":0,\"Rate3\":0,\"ChannelType\":0,\"BuyChannel\":\"\",\"Remark\":\"\"}]");
            //var value = "funds=" + v;

            ////请求服务
            //value = HttpHelper.Default.Poster(url, value);
            //Console.WriteLine(value);
            //Console.ReadLine();

            //return;

            #endregion

            CastleFactory.Create().RegisterLogger(new ServiceLog());
            CastleFactory.Create().OnDisconnected += Program_OnDisconnected;

            //var watch = Stopwatch.StartNew();

            var e = new ManualResetEvent(false);

            for (int i = 0; i < 100; i++)
            {
                Thread thread = new Thread(DoWork1);
                thread.Start(e);
            }

            e.Set();

            #region test3

            //for (int i = 0; i < 100; i++)
            //{
            //    Thread thread = new Thread(DoWork1);
            //    thread.Start(are);
            //}

            //are.WaitOne();

            //for (int i = 0; i < 100; i++)
            //{
            //    Thread thread = new Thread(DoWork1);
            //    thread.Start(are);
            //}

            //are.WaitOne();

            //watch.Stop();

            //Console.WriteLine("ElapsedMilliseconds => " + watch.ElapsedMilliseconds + " ms.");

            //var node = CastleFactory.Create().GetDefaultNode();
            //var clients = CastleFactory.Create().GetChannel<IStatusService>(node).GetAppClients();

            //DoWork1();

            //string a = SerializationManager.SerializeJson(null);

            //var request = (HttpWebRequest)WebRequest.Create("http://webapi.mysoft.com/user.getuser1");
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";

            //using (var stream = request.GetRequestStream())
            //{
            //    var user = MySoft.SerializationManager.SerializeJson(new { user = new { Name = "123" } });
            //    stream.Write(Encoding.UTF8.GetBytes(user), 0, Encoding.UTF8.GetByteCount(user));
            //    stream.Flush();
            //}

            //var response = (HttpWebResponse)request.GetResponse();
            //using (var sr = new StreamReader(response.GetResponseStream()))
            //{
            //    var str = sr.ReadToEnd();
            //    Console.WriteLine(str);
            //}

            //var container = new CookieContainer();
            //container.Add(new Cookie("uid", "123", "/", "a.com"));
            //container.Add(new Cookie("pwd", "dafsdf", "/", "a.com"));
            //request.CookieContainer = container;

            //using (var stream = request.GetRequestStream())
            //{
            //    var buffer = Encoding.UTF8.GetBytes("{ user : { Name : \"mmm\" } }");
            //    stream.Write(buffer, 0, buffer.Length);
            //}

            //var response = request.GetResponse();
            //using (var sr = new StreamReader(response.GetResponseStream()))
            //{
            //    var str = sr.ReadToEnd();
            //    Console.WriteLine(str);
            //}

            #endregion

            Console.ReadKey();
        }

        static void Program_OnDisconnected(object sender, ConnectEventArgs e)
        {
            Console.WriteLine(e.Channel.CommunicationState);
        }

        static Program()
        {
            ServicePointManager.Expect100Continue = false;
        }

        static void GetRequestString()
        {
            var url = "http://192.168.1.230:7004/fundapi/restful/system/session";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            var hashtable = new Dictionary<string, string>();
            hashtable.Add("merid", "FUND123");
            hashtable.Add("usertype", "");
            hashtable.Add("signmode", "md5");
            hashtable.Add("format", "json");
            hashtable.Add("version", "1.0");
            hashtable.Add("timestamp", "20120410164433");
            hashtable.Add("sessionkey", "");
            hashtable.Add("function", "P005");
            hashtable.Add("channel", "3");
            hashtable.Add("signmsg", "fc5885c9a346c65e67ec5d364616508a");

            var list = new List<string>();
            foreach (var kv in hashtable)
            {
                list.Add(string.Format("{0}={1}", kv.Key, kv.Value));
            }

            var buffer = Encoding.UTF8.GetBytes(string.Join("&", list.ToArray()));
            request.ContentLength = buffer.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }

            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
        }

        static void DoWork1(object state)
        {
            var e = state as ManualResetEvent;
            //e.WaitOne();

            //var node = CastleFactory.Create().GetDefaultNode();
            var service = CastleFactory.Create().GetChannel<IUserService>();

            //var service = new MySoft.PlatformService.UserService.UserService();

            while (true)
            {
                try
                {
                    Stopwatch watch = Stopwatch.StartNew();

                    //int length = 1;
                    //UserInfo user;

                    //service.GetUserInfo("maoyong", ref length, out user);

                    //string userid;
                    //Guid guid;
                    //UserInfo user;
                    //UserInfo info = service.GetUserInfo("maoyong_" + Guid.NewGuid(), out userid, out guid, out user);

                    //int length;
                    int count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 1);
                    //var value = service.GetUsersString(count, out length);

                    //var value = service.GetUser(new Random().Next(1, 10000));

                    var value = service.GetUser(count, count);

                    //var user = service.GetUser(counter);

                    //var users = service.GetUsers();
                    //var str = service.GetUsersString();

                    watch.Stop();

                    Interlocked.Increment(ref counter);

                    Console.WriteLine("【" + counter + "】times => " + value.Id + " timeout: " + watch.ElapsedMilliseconds + " ms.");

                    //var clients = service1.GetClientList();

                    //Console.WriteLine("{0} => {1}", DateTime.Now, clients.Count);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    Console.WriteLine("[{0}] {1}", DateTime.Now, msg);
                }

                //Thread.Sleep(1000);
            }
        }

        static void Program_OnError(Exception error)
        {
            Console.WriteLine(error.Message);
        }

        static void castle_OnError(Exception error)
        {
            string message = "[" + DateTime.Now.ToString() + "] " + error.Message;
            if (error.InnerException != null)
            {
                message += "\r\n错误信息 => " + error.InnerException.Message;
            }
            lock (syncobj)
            {
                if (error is WarningException)
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
                    //string userid;
                    //UserInfo info = service.GetUserInfo("maoyong_" + Guid.NewGuid(), out userid);

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
