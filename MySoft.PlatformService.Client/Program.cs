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
using System.Net;
using System.IO;
using System.Collections.Specialized;
using MySoft.IoC.Messages;
using System.Collections;
using MySoft.IoC.Logger;
using System.Web;
using MySoft.RESTful.SDK;

namespace MySoft.PlatformService.Client
{
    public class ServiceResolver : IServiceResolver
    {

        #region IServiceResolver 成员

        public ServerNode GetServerNode<T>(ServerNode currNode)
        {
            //throw new NotImplementedException();
            return currNode;
        }

        public T ResolveService<T>(IContainer container)
        {
            //throw new NotImplementedException();
            return default(T);
        }

        #endregion
    }

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

        static void Main(string[] args)
        {
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

            CastleFactory.Create().RegisterLogger(new ServiceLog());
            CastleFactory.Create().RegisterResolver(new ServiceResolver());

            AutoResetEvent are = new AutoResetEvent(false);
            are.Reset();
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                Thread thread = new Thread(DoWork1);
                thread.Start(are);
            }

            are.WaitOne();

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

            watch.Stop();

            Console.WriteLine("ElapsedMilliseconds => " + watch.ElapsedMilliseconds + " ms.");

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

            Console.ReadKey();
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
            var node = CastleFactory.Create().GetDefaultNode();

            var service = CastleFactory.Create().GetChannel<IUserService>();
            //service = new UserService.UserService();
            //var service1 = CastleFactory.Create().GetChannel<IStatusService>(node);

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

                    int length;
                    int count = new Random().Next(1, 5);
                    var value = service.GetUsersString(count, out length);

                    //var user = service.GetUser(counter);

                    //var users = service.GetUsers();
                    //var str = service.GetUsersString();

                    watch.Stop();

                    Interlocked.Increment(ref counter);

                    Console.WriteLine("【" + counter + "】times => " + length + " timeout: " + watch.ElapsedMilliseconds + " ms.");

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

            AutoResetEvent are = state as AutoResetEvent;
            are.Set();
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
