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
using System.Xml.Serialization;
using System.Linq;
using Amib.Threading;
using System.Xml;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace MySoft.PlatformService.Client
{
    public class ServiceCall : IServiceCall
    {
        #region IServiceCall ≥…‘±

        public void BeginCall(CallMessage reqMsg)
        {
            //throw new NotImplementedException();
        }

        public void EndCall(CallMessage reqMsg, ReturnMessage resMsg, long elapsedTime)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }

    public class UserA
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UserB
    {
        public int Id1 { get; set; }
        public string Name1 { get; set; }
    }

    class Program
    {
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
            CastleFactory.Create().OnError += Program_OnError;
            CastleFactory.Create().Register(new ServiceCall());

            var e = new ManualResetEvent(false);

            for (int i = 0; i < 1; i++)
            {
                Thread thread = new Thread(DoWork);
                thread.Start(e);

                Thread.Sleep(100);
            }

            e.Set();

            Console.ReadKey();
        }

        static void DoWork(object state)
        {
            var e = state as ManualResetEvent;
            //e.WaitOne();

            //var node = CastleFactory.Create().GetDefaultNode();
            //var service = new MySoft.PlatformService.UserService.UserService();

            var rand = new Random();
            while (true)
            {
                var service = CastleFactory.Create().GetChannel<IUserService>();
                Stopwatch watch = Stopwatch.StartNew();

                try
                {
                    //var users = service.GetUsers();
                    var user = service.GetUser(rand.Next(1, 10000));

                    //var xml = SerializationManager.SerializeXml(users, Encoding.GetEncoding(936));

                    //var obj = SerializationManager.DeserializeXml<IList<UserInfo>>(xml);

                    //Console.WriteLine(xml);
                    //users.Clear();

                    //int length = 1;
                    //UserInfo user;

                    //service.GetUserInfo("maoyong", ref length, out user);

                    //string userid;
                    //Guid guid;
                    //UserInfo user;
                    //UserInfo info = service.GetUserInfo("maoyong_" + Guid.NewGuid(), out userid, out guid, out user);

                    //int length;
                    // int count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 1000);
                    ////var value = service.GetUsersString(count, out length);

                    //var p = SerializationManager.SerializeJson(new { id = count });

                    //var v = CastleFactory.Create().Invoke(node, new InvokeMessage
                    //{
                    //    ServiceName = typeof(IUserService).FullName,
                    //    MethodName = typeof(IUserService).GetMethod("GetUser", new Type[] { typeof(int) }).ToString(),
                    //    Parameters = p,
                    //    CacheTime = 10
                    //});

                    ////var a = count.ToString().PadRight(1000000, '#');

                    //var value = service.GetUser(count);

                    //var value = service.GetUsers();

                    //var user = service.GetUser(counter);

                    //var users = service.GetUsers();
                    //var str = service.GetUsersString();

                    Interlocked.Increment(ref counter);

                    Console.WriteLine(DateTime.Now + "°æ" + counter + "°øtimes => " + user.Id + " timeout: "
                                    + watch.ElapsedMilliseconds + " ms. " + Thread.CurrentThread.ManagedThreadId);

                    //if (value != null)
                    //{
                    //    Console.WriteLine("°æ" + counter + "°øtimes => " + value.Name.Length + " timeout: " + watch.ElapsedMilliseconds + " ms.");
                    //}
                    //else
                    //{
                    //    Console.WriteLine("°æ" + counter + "°øtimes => timeout: " + watch.ElapsedMilliseconds + " ms.");
                    //}

                    //value.Clear();

                    //var clients = service1.GetClientList();

                    //Console.WriteLine("{0} => {1}", DateTime.Now, clients.Count);
                }
                catch (TimeoutException ex)
                {
                    //SimpleLog.Instance.WriteLogForDir("Timeout", ex);

                    string msg = ErrorHelper.GetInnerException(ex).Message;
                    Console.WriteLine("[{0}] {1}  timeout: {2} ms.", DateTime.Now, msg, watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    string msg = ErrorHelper.GetInnerException(ex).Message;
                    Console.WriteLine("[{0}] {1}", DateTime.Now, msg);
                }
                finally
                {
                    if (watch.IsRunning)
                    {
                        watch.Stop();
                    }
                }
            }
        }

        static void Program_OnError(Exception error)
        {
            SimpleLog.Instance.WriteLog(error);
        }
    }
}
